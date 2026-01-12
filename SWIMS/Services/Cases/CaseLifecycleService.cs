using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWIMS.Data;
using SWIMS.Data.Cases;
using SWIMS.Data.Lookups;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Cases;

public sealed class CaseLifecycleService : ICaseLifecycleService
{
    private const int FallbackDefaultMonths = 6;

    private readonly SwimsCasesDbContext _cases;
    private readonly SwimsDb_moreContext _core;
    private readonly SwimsLookupDbContext _lookup;
    private readonly ILogger<CaseLifecycleService> _logger;

    public CaseLifecycleService(
        SwimsCasesDbContext cases,
        SwimsDb_moreContext core,
        SwimsLookupDbContext lookup,
        ILogger<CaseLifecycleService> logger)
    {
        _cases = cases;
        _core = core;
        _lookup = lookup;
        _logger = logger;
    }

    public async Task<CaseLifecycleResult> RefreshFromPrimaryApplicationAsync(
        int caseId,
        string? triggeredByUserId = null,
        CancellationToken ct = default)
    {
        var caseEntity = await _cases.SW_cases.FirstOrDefaultAsync(c => c.Id == caseId, ct);
        if (caseEntity == null)
            return new(false, null, null, "Case not found.");

        var primaryLink = await _cases.SW_caseForms
            .AsNoTracking()
            .Where(x => x.SW_caseId == caseId && x.is_primary_application)
            .OrderByDescending(x => x.linked_at)
            .FirstOrDefaultAsync(ct);

        if (primaryLink == null)
            return new(false, caseEntity.status, caseEntity.status, "No primary application form is linked to this case.");

        // DbSet is SW_formTableData, entity type is SW_formTableDatum
        SW_formTableDatum? formData = await _core.SW_formTableData
            .Include(d => d.SW_forms)
            .FirstOrDefaultAsync(d => d.Id == primaryLink.SW_formTableDatumId, ct);

        if (formData == null)
            return new(false, caseEntity.status, caseEntity.status, "Primary application submission not found.");

        // ---- Use real approval fields on SW_formTableDatum (no reflection for approvals)
        var decision = DeriveApprovalDecision(formData);

        // Benefit start: when approved, use the "final approval" timestamp (last step in play)
        var finalApprovalAt = GetFinalApprovalTimestampUtc(formData) ?? DateTime.UtcNow;

        // Optional: allow future "explicit start/end" fields on the submission via reflection
        // (this is for your planned custom period fields on the form record, NOT for approvals)
        var explicitStart =
            TryGetDateTime(formData,
                "benefit_start_at", "benefit_start_date",
                "approval_start_date", "approval_start",
                "approved_start_date", "date_approved", "approved_at");

        var explicitEnd =
            TryGetDateTime(formData,
                "benefit_end_at", "benefit_end_date",
                "approval_end_date", "approval_end",
                "approved_end_date", "expires_at", "expiry_date");

        // Case-level override (plan-ahead for custom period; properties may not exist yet -> reflection-safe)
        var overrideMonths = TryGetInt(caseEntity, "benefit_period_months_override");
        var overrideStart = TryGetDateTime(caseEntity, "benefit_start_at_override");
        var overrideEnd = TryGetDateTime(caseEntity, "benefit_end_at_override");

        // Start date precedence: Case override -> explicit form field -> final approval timestamp
        DateTime benefitStart = overrideStart ?? explicitStart ?? finalApprovalAt;

        // Months precedence:
        // Case override -> form explicit months field -> program default -> fallback
        int? months =
            overrideMonths
            ?? TryGetInt(formData, "benefit_period_months", "approval_period_months", "months_approved", "period_months")
            ?? TryParseMonthsFromString(TryGetString(formData, "benefit_period", "approval_period", "approved_period"));

        if (months == null)
            months = await TryGetProgramDefaultMonthsAsync(caseEntity.ProgramTagId, ct) ?? FallbackDefaultMonths;

        // End date precedence: Case override -> explicit form end date -> computed from months
        DateTime? benefitEnd =
            overrideEnd
            ?? explicitEnd
            ?? (months > 0 ? benefitStart.AddMonths(months.Value) : null);

        // ---- Decide case status (Pending / Active / Inactive / Closed)
        var oldStatus = caseEntity.status ?? "Pending";
        var newStatus = oldStatus;

        if (decision == true)
        {
            // Approved => Active unless already expired => Inactive
            if (benefitEnd.HasValue && benefitEnd.Value <= DateTime.UtcNow)
                newStatus = "Inactive";
            else
                newStatus = "Active";
        }
        else if (decision == false)
        {
            // Not approved (rejected/denied) => Closed
            newStatus = "Closed";
        }
        else
        {
            // Still in progress => Pending
            newStatus = "Pending";
        }

        // ---- Apply updates
        var changed = false;

        if (!string.Equals(caseEntity.status, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            caseEntity.status = newStatus;
            changed = true;
        }

        // closed_at consistency (your existing behavior)
        if (string.Equals(newStatus, "Closed", StringComparison.OrdinalIgnoreCase))
        {
            if (caseEntity.closed_at == null)
            {
                caseEntity.closed_at = DateTime.UtcNow;
                changed = true;
            }
        }
        else
        {
            if (caseEntity.closed_at != null)
            {
                caseEntity.closed_at = null;
                changed = true;
            }
        }

        // Only set benefit fields when approved (keeps pending cases clean)
        // Uses reflection so it won't break if these columns aren't added yet.
        if (decision == true)
        {
            changed |= TrySetValue(caseEntity, "benefit_start_at", benefitStart);
            changed |= TrySetValue(caseEntity, "benefit_end_at", benefitEnd);
            changed |= TrySetValue(caseEntity, "benefit_period_months", months);

            var source =
                (overrideMonths != null || overrideStart != null || overrideEnd != null) ? "CaseOverride" :
                (explicitEnd != null || explicitStart != null) ? "FormExplicit" :
                "Computed";

            changed |= TrySetValue(caseEntity, "benefit_period_source", source);
        }

        if (!changed)
            return new(false, oldStatus, newStatus, "No changes were required.");

        await _cases.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Case {CaseId} refreshed from primary application by {UserId}. Status {Old} -> {New}.",
            caseId, triggeredByUserId ?? "(system)", oldStatus, newStatus);

        return new(true, oldStatus, newStatus, $"Case refreshed from primary application. Status: {oldStatus} → {newStatus}.");
    }

    // -----------------------------
    // Program default benefit months (plan-ahead)
    // -----------------------------
    private async Task<int?> TryGetProgramDefaultMonthsAsync(int? programTagId, CancellationToken ct)
    {
        if (programTagId == null) return null;

        var tag = await _lookup.SW_programTags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == programTagId.Value, ct);

        var months = tag?.default_benefit_months;
        if (months is int m && m > 0)
            return m;

        return null;
    }


    // -----------------------------
    // Approvals (strongly-typed)
    // -----------------------------
    private sealed record ApprovalSlot(
        int Level,
        int? Status,
        DateTime? At,
        string? By,
        string? Comment)
    {
        public bool IsInPlay =>
            Status.HasValue ||
            At.HasValue ||
            !string.IsNullOrWhiteSpace(By) ||
            !string.IsNullOrWhiteSpace(Comment);
    }

    private static IReadOnlyList<ApprovalSlot> GetApprovalSlots(SW_formTableDatum d) => new[]
    {
        new ApprovalSlot(1, d.isApproval_01, d.isApp_dateTime_01, d.isApprover_01, d.isAppComment_01),
        new ApprovalSlot(2, d.isApproval_02, d.isApp_dateTime_02, d.isApprover_02, d.isAppComment_02),
        new ApprovalSlot(3, d.isApproval_03, d.isApp_dateTime_03, d.isApprover_03, d.isAppComment_03),
        new ApprovalSlot(4, d.isApproval_04, d.isApp_dateTime_04, d.isApprover_04, d.isAppComment_04),
        new ApprovalSlot(5, d.isApproval_05, d.isApp_dateTime_05, d.isApprover_05, d.isAppComment_05),
    };

    /// <summary>
    /// Returns:
    /// true  => fully approved
    /// null  => still in progress (pending)
    /// false => terminal not-approved state (only if a non 0/1 value appears)
    /// </summary>
    private static bool? DeriveApprovalDecision(SW_formTableDatum d)
    {
        var slots = GetApprovalSlots(d).Where(s => s.IsInPlay).ToList();

        // No approval flow in play => keep case Pending (no decision)
        if (slots.Count == 0) return null;

        // Convention in SWIMS: 0 = pending, 1 = approved
        // If any step is missing or 0 => pending
        if (slots.Any(s => !s.Status.HasValue || s.Status.Value == 0))
            return null;

        // If all in-play steps are 1 => approved
        if (slots.All(s => s.Status.Value == 1))
            return true;

        // Any other values => treat as terminal not-approved for now
        return false;
    }

    private static DateTime? GetFinalApprovalTimestampUtc(SW_formTableDatum d)
    {
        var slots = GetApprovalSlots(d)
            .Where(s => s.IsInPlay)
            .OrderBy(s => s.Level)
            .ToList();

        if (slots.Count == 0) return null;

        // Look for the highest-level approved slot with a timestamp
        var lastApproved = slots
            .Where(s => s.Status.HasValue && s.Status.Value == 1 && s.At.HasValue)
            .OrderByDescending(s => s.Level)
            .FirstOrDefault();

        if (lastApproved?.At == null) return null;

        // Preserve kind if present; otherwise assume local/unspecified and treat as UTC-ish
        var dt = lastApproved.At.Value;
        return dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
    }

    // -----------------------------
    // Small parsing helpers
    // -----------------------------
    private static int? TryParseMonthsFromString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        // "6 months", "6 Months", "6"
        var m = Regex.Match(input, @"(\d+)");
        if (!m.Success) return null;

        if (int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var months))
            return months;

        return null;
    }

    // -----------------------------
    // Reflection-based "optional fields" helpers (not used for approvals)
    // -----------------------------
    private static bool? TryGetBool(object o, params string[] names)
    {
        foreach (var name in names)
        {
            var p = GetProp(o, name);
            if (p == null) continue;

            var v = p.GetValue(o);
            if (v is bool b) return b;

            if (v is string s && bool.TryParse(s, out var parsed))
                return parsed;
        }
        return null;
    }

    // Overload: read int from property name(s) using reflection
    private static int? TryGetInt(object o, params string[] names)
    {
        foreach (var name in names)
        {
            var p = GetProp(o, name);
            if (p == null) continue;

            var parsed = TryGetInt(p.GetValue(o));
            if (parsed.HasValue) return parsed.Value;
        }
        return null;
    }

    // Value parser: supports boxed nullable ints (boxes as int), numerics, and strings
    private static int? TryGetInt(object? v)
    {
        if (v is null) return null;

        // Nullable<int> boxes as int when not null
        if (v is int i) return i;

        if (v is long l && l >= int.MinValue && l <= int.MaxValue) return (int)l;
        if (v is short s) return s;
        if (v is byte b) return b;

        if (v is decimal dec &&
            dec >= int.MinValue && dec <= int.MaxValue &&
            decimal.Truncate(dec) == dec)
        {
            return (int)dec;
        }

        if (v is string str)
        {
            str = str.Trim();
            if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
        }

        return null;
    }

    // Overload: read DateTime from property name(s) using reflection
    private static DateTime? TryGetDateTime(object o, params string[] names)
    {
        foreach (var name in names)
        {
            var p = GetProp(o, name);
            if (p == null) continue;

            var parsed = TryGetDateTime(p.GetValue(o));
            if (parsed.HasValue) return parsed.Value;
        }
        return null;
    }

    // Value parser: supports boxed nullable DateTime (boxes as DateTime), and strings
    private static DateTime? TryGetDateTime(object? v)
    {
        if (v is null) return null;

        // Nullable<DateTime> boxes as DateTime when not null
        if (v is DateTime dt) return dt;

        if (v is string str)
        {
            str = str.Trim();
            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
                return parsed;
        }

        return null;
    }

    private static string? TryGetString(object o, params string[] names)
    {
        foreach (var name in names)
        {
            var p = GetProp(o, name);
            if (p == null) continue;

            var v = p.GetValue(o);
            if (v is string s && !string.IsNullOrWhiteSpace(s))
                return s;
        }
        return null;
    }

    private static bool TrySetValue(object target, string propertyName, object? value)
    {
        var p = GetProp(target, propertyName);
        if (p == null || !p.CanWrite) return false;

        var current = p.GetValue(target);
        if (Equals(current, value)) return false;

        // Handle nullable conversions
        if (value != null && p.PropertyType != value.GetType())
        {
            var underlying = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            if (underlying.IsEnum && value is string es)
                value = Enum.Parse(underlying, es, ignoreCase: true);
            else
                value = Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
        }

        p.SetValue(target, value);
        return true;
    }

    private static PropertyInfo? GetProp(object o, string name)
        => o.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
}
