using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SWIMS.Data.Cases;
using SWIMS.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Cases;

public sealed class CaseBeneficiaryPeriodSweepHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CaseBeneficiaryPeriodSweepHostedService> _logger;

    private static readonly TimeSpan SweepInterval = TimeSpan.FromHours(6);

    public CaseBeneficiaryPeriodSweepHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<CaseBeneficiaryPeriodSweepHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepOnce(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Case beneficiary period sweep failed.");
            }

            try
            {
                await Task.Delay(SweepInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task SweepOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwimsCasesDbContext>();
        var lifecycle = scope.ServiceProvider.GetService<ICaseLifecycleService>(); // optional if not registered

        var entityType = db.Model.FindEntityType(typeof(SW_case));
        var endProp = entityType?.FindProperty("benefit_end_at");
        if (endProp == null)
        {
            _logger.LogDebug("Skipping case expiry sweep: SW_case.benefit_end_at is not mapped yet.");
            return;
        }

        var hasOverrideProp = entityType?.FindProperty("status_override") != null;
        var hasOverrideUntilProp = entityType?.FindProperty("status_override_until") != null;

        var now = DateTime.UtcNow;

        // 1) If override_until has passed, clear override + recompute (best-effort)
        if (hasOverrideProp && hasOverrideUntilProp && lifecycle != null)
        {
            var expiredOverrideIds = await db.SW_cases
                .Where(c => c.status_override != null && c.status_override_until != null && c.status_override_until <= now)
                .Select(c => c.Id)
                .ToListAsync(ct);

            if (expiredOverrideIds.Count > 0)
            {
                foreach (var id in expiredOverrideIds)
                {
                    var c = await db.SW_cases.FirstOrDefaultAsync(x => x.Id == id, ct);
                    if (c == null) continue;

                    c.status_override = null;
                    c.status_override_reason = null;
                    c.status_override_until = null;
                    c.status_override_at = null;
                    c.status_override_by = null;
                }

                await db.SaveChangesAsync(ct);

                foreach (var id in expiredOverrideIds)
                {
                    try
                    {
                        await lifecycle.RefreshFromPrimaryApplicationAsync(id, "(system-sweep)", ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to recompute status after expiring override for case {CaseId}.", id);
                    }
                }

                _logger.LogInformation("Expired {Count} manual status overrides.", expiredOverrideIds.Count);
            }
        }

        // 2) Expire beneficiary periods -> Inactive (but NOT if a manual override is active)
        var q = db.SW_cases.AsQueryable();

        q = q.Where(c => c.status == "Active" && c.benefit_end_at != null && c.benefit_end_at <= now);

        if (hasOverrideProp)
        {
            q = q.Where(c => c.status_override == null);
        }

        var candidates = await q.ToListAsync(ct);

        if (candidates.Count == 0)
            return;

        foreach (var c in candidates)
            c.status = "Inactive";

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Marked {Count} cases as Inactive due to expired beneficiary period (manual overrides excluded).",
            candidates.Count);
    }
}
