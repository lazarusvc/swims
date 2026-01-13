using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWIMS.Data;
using SWIMS.Data.Cases;
using SWIMS.Data.Lookups;
using SWIMS.Models;
using System;
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
        var now = DateTime.UtcNow;

        var caseEntity = await _cases.SW_cases.FirstOrDefaultAsync(c => c.Id == caseId, ct);
        if (caseEntity == null)
            return new(false, null, null, "Case not found.");

        // Auto-expire timed overrides (plan-ahead). If override_until is in the past, clear it.
        var overrideExpired = TryAutoExpireStatusOverride(caseEntity, now);

        var primaryLink = await _cases.SW_caseForms
            .AsNoTracking()
            .Where(x => x.SW_caseId == caseId && x.is_primary_application)
            .OrderByDescending(x => x.linked_at)
            .FirstOrDefaultAsync(ct);

        if (primaryLink == null)
            return new(false, caseEntity.status, caseEntity.status, "No primary application form is linked to this case.");

        var formData = await _core.SW_formTableData
            .Include(d => d.SW_forms)
            .FirstOrDefaultAsync(d => d.Id == primaryLink.SW_formTableDatumId, ct);

        if (formData == null)
            return new(false, caseEntity.status, caseEntity.status, "Primary application submission not found.");

        // ---- Approval decision (defensive)
        bool? decision = TryExtractApprovalDecision(formData);
        if (decision == null)
        {
            var derived = DeriveCaseStatusFromApprovals(formData);
            decision = derived.Equals("Active", StringComparison.OrdinalIgnoreCase) ? true
                     : derived.Equals("Closed", StringComparison.OrdinalIgnoreCase) ? false
                     : (bool?)null;
        }

        // ---- Period inputs
        var approvalStart =
            TryGetDateTime(formData,
                "approval_start_date", "approval_start", "benefit_start_date", "benefit_start_at",
                "approved_start_date", "date_approved", "approved_at")
            ?? now;

        var explicitEnd =
            TryGetDateTime(formData,
                "approval_end_date", "approval_end", "benefit_end_date", "benefit_end_at",
                "approved_end_date", "expires_at", "expiry_date");

        // Case-level benefit override
        var overrideMonths = TryGetInt(caseEntity, "benefit_period_months_override");
        var overrideStart = TryGetDateTime(caseEntity, "benefit_start_at_override");
        var overrideEnd = TryGetDateTime(caseEntity, "benefit_end_at_override");

        var benefitStart = overrideStart ?? approvalStart;

        int? months =
            overrideMonths
            ?? TryGetInt(formData, "approval_period_months", "benefit_period_months", "months_approved", "period_months")
            ?? TryParseMonthsFromString(TryGetString(formData, "approval_period", "benefit_period", "approved_period"));

        if (months == null)
            months = await TryGetProgramDefaultMonthsAsync(caseEntity.ProgramTagId, ct) ?? FallbackDefaultMonths;

        DateTime? benefitEnd =
            overrideEnd
            ?? explicitEnd
            ?? ((months.HasValue && months.Value > 0) ? benefitStart.AddMonths(months.Value) : (DateTime?)null);

        // ---- Computed status
        var oldStatus = (caseEntity.status ?? "Pending").Trim();
        var computedStatus = oldStatus;

        if (decision == true)
        {
            computedStatus = (benefitEnd.HasValue && benefitEnd.Value <= now) ? "Inactive" : "Active";
        }
        else if (decision == false)
        {
            computedStatus = "Closed";
        }
        else
        {
            computedStatus = "Pending";
        }

        // ---- Manual override wins (if active)
        var overrideStatus = (TryGetString(caseEntity, "status_override") ?? "").Trim();
        var overrideUntil = TryGetDateTime(caseEntity, "status_override_until");
        var hasActiveManualOverride =
            !string.IsNullOrWhiteSpace(overrideStatus)
            && (overrideUntil == null || overrideUntil.Value > now);

        var changed = false;

        // Always keep benefit fields in sync
        changed |= TrySetValue(caseEntity, "benefit_start_at", benefitStart);
        changed |= TrySetValue(caseEntity, "benefit_end_at", benefitEnd);
        changed |= TrySetValue(caseEntity, "benefit_period_months", months);
        changed |= TrySetValue(caseEntity, "benefit_period_source",
            (overrideMonths != null || overrideStart != null || overrideEnd != null)
                ? "CaseOverride"
                : (explicitEnd != null ? "FormEndDate" : "Computed"));

        // Apply status rule
        if (hasActiveManualOverride)
        {
            // Ensure the stored status matches the override (prevents drift like in your screenshot)
            if (!string.Equals(caseEntity.status, overrideStatus, StringComparison.OrdinalIgnoreCase))
            {
                caseEntity.status = overrideStatus;
                changed = true;
            }

            // Keep closed_at consistent with manual override
            if (string.Equals(overrideStatus, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                if (caseEntity.closed_at == null)
                {
                    caseEntity.closed_at = now;
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
        }
        else
        {
            // No manual override => write computed status
            if (!string.Equals(caseEntity.status, computedStatus, StringComparison.OrdinalIgnoreCase))
            {
                caseEntity.status = computedStatus;
                changed = true;
            }

            if (string.Equals(computedStatus, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                if (caseEntity.closed_at == null)
                {
                    caseEntity.closed_at = now;
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
        }

        if (!changed)
        {
            return new(false, oldStatus, caseEntity.status, hasActiveManualOverride
                ? "No changes were required (manual status override is active)."
                : "No changes were required.");
        }

        await _cases.SaveChangesAsync(ct);

        if (hasActiveManualOverride)
        {
            var msg = $"Case refreshed from primary application. Benefit period updated. Manual status override remains in effect ({overrideStatus}).";
            if (overrideExpired) msg = "Timed status override expired and was cleared. " + msg;
            return new(true, oldStatus, caseEntity.status, msg);
        }

        return new(true, oldStatus, computedStatus, $"Case refreshed from primary application. Status: {oldStatus} → {computedStatus}.");
    }

    private async Task<int?> TryGetProgramDefaultMonthsAsync(int? programTagId, CancellationToken ct)
    {
        if (programTagId == null) return null;

        var tag = await _lookup.SW_programTags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == programTagId.Value, ct);
        if (tag == null) return null;

        return TryGetInt(tag, "default_benefit_months", "defaultBenefitMonths", "DefaultBenefitMonths");
    }

    private static bool TryAutoExpireStatusOverride(object caseEntity, DateTime nowUtc)
    {
        var s = (TryGetString(caseEntity, "status_override") ?? "").Trim();
        if (string.IsNullOrWhiteSpace(s)) return false;

        var until = TryGetDateTime(caseEntity, "status_override_until");
        if (until == null) return false;

        if (until.Value > nowUtc) return false;

        var changed = false;
        changed |= TrySetValue(caseEntity, "status_override", null);
        changed |= TrySetValue(caseEntity, "status_override_reason", null);
        changed |= TrySetValue(caseEntity, "status_override_until", null);
        changed |= TrySetValue(caseEntity, "status_override_at", null);
        changed |= TrySetValue(caseEntity, "status_override_by", null);

        return changed;
    }

    private static bool? TryExtractApprovalDecision(object formData)
    {
        var b = TryGetBool(formData,
            "is_approved", "approved", "approval_isApproved", "isApproved", "IsApproved",
            "approvalApproved", "ApprovalApproved");
        if (b.HasValue) return b.Value;

        var s = TryGetString(formData,
            "approval_status", "approval_decision", "approval_outcome", "approval_state",
            "ApprovalStatus", "ApprovalDecision");

        if (string.IsNullOrWhiteSpace(s)) return null;

        s = s.Trim();
        if (s.Equals("Approved", StringComparison.OrdinalIgnoreCase)) return true;
        if (s.Equals("Rejected", StringComparison.OrdinalIgnoreCase)) return false;
        if (s.Equals("Denied", StringComparison.OrdinalIgnoreCase)) return false;

        if (s.IndexOf("approv", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (s.IndexOf("reject", StringComparison.OrdinalIgnoreCase) >= 0) return false;
        if (s.IndexOf("deny", StringComparison.OrdinalIgnoreCase) >= 0) return false;

        return null;
    }

    private static int? TryParseMonthsFromString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var m = Regex.Match(input, @"(\d+)");
        if (!m.Success) return null;

        return int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var months)
            ? months
            : null;
    }

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

    private static int? TryGetInt(object? v)
    {
        if (v is null) return null;
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
            if (int.TryParse(str, out var parsed)) return parsed;
        }

        return null;
    }

    private static DateTime? TryGetDateTime(object? v)
    {
        if (v is null) return null;
        if (v is DateTime dt) return dt;

        if (v is string str)
        {
            str = str.Trim();
            if (DateTime.TryParse(str, out var parsed)) return parsed;
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
            if (v is string s) return s;
        }
        return null;
    }

    private static bool TrySetValue(object target, string propertyName, object? value)
    {
        var p = GetProp(target, propertyName);
        if (p == null || !p.CanWrite) return false;

        var current = p.GetValue(target);
        if (Equals(current, value)) return false;

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

    private static string DeriveCaseStatusFromApprovals(SW_formTableDatum d)
    {
        var slots = GetApprovalSlots(d).Where(s => s.IsInPlay).ToList();
        if (slots.Count == 0)
            return "Pending";

        // 0 = pending, 1 = approved
        if (slots.Any(s => !s.Status.HasValue || s.Status.Value == 0))
            return "Pending";

        if (slots.All(s => s.Status.Value == 1))
            return "Active";

        return "Closed";
    }
}
