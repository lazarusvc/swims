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

    // Keep it simple for MVP: sweep every 6 hours
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
        // Small startup delay so app boot doesn't compete with the first sweep
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
                // normal shutdown
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

        // Guard: only run if EF model actually maps benefit_end_at
        // (This keeps the service safe to register even before your DB work is finished.)
        var entityType = db.Model.FindEntityType(typeof(SW_case));
        var endProp = entityType?.FindProperty("benefit_end_at");
        if (endProp == null)
        {
            _logger.LogDebug("Skipping case expiry sweep: SW_case.benefit_end_at is not mapped yet.");
            return;
        }

        var now = DateTime.UtcNow;

        var candidates = await db.SW_cases
            .Where(c => c.status == "Active" && c.benefit_end_at != null && c.benefit_end_at <= now)
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return;

        foreach (var c in candidates)
            c.status = "Inactive";

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Marked {Count} cases as Inactive due to expired beneficiary period.",
            candidates.Count);
    }
}
