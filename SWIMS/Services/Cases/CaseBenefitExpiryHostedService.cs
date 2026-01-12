using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SWIMS.Data.Cases;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Cases;

public sealed class CaseBenefitExpiryHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CaseBenefitExpiryHostedService> _logger;

    public CaseBenefitExpiryHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<CaseBenefitExpiryHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run every hour; cheap query, simple logic.
        var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireCasesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while expiring beneficiary periods.");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task ExpireCasesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SwimsCasesDbContext>();

        var now = DateTime.UtcNow;

        // NOTE: requires benefit_end_at column/property to exist.
        var candidates = await db.SW_cases
            .Where(c => c.status == "Active" && c.benefit_end_at != null && c.benefit_end_at <= now)
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return;

        foreach (var c in candidates)
        {
            c.status = "Inactive";
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Marked {Count} cases as Inactive due to expired beneficiary period.", candidates.Count);
    }
}
