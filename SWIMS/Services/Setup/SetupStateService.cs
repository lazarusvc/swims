using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SWIMS.Data;
using SWIMS.Data.Reports;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Setup
{
    public sealed class SetupStateService : ISetupStateService
    {
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _env;
        private readonly SwimsIdentityDbContext _identityDb;
        private readonly SwimsDb_moreContext _coreDb;
        private readonly SwimsStoredProcsDbContext _spDb;
        private readonly SwimsReportsDbContext _reportsDb;

        public SetupStateService(
            IConfiguration config,
            IHostEnvironment env,
            SwimsIdentityDbContext identityDb,
            SwimsDb_moreContext coreDb,
            SwimsStoredProcsDbContext spDb,
            SwimsReportsDbContext reportsDb)
        {
            _config = config;
            _env = env;
            _identityDb = identityDb;
            _coreDb = coreDb;
            _spDb = spDb;
            _reportsDb = reportsDb;
        }

        public async Task<SetupSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var checks = new List<SetupCheck>();

            // Environment
            checks.Add(new SetupCheck
            {
                Key = "environment",
                Name = "Hosting environment",
                Status = SetupCheckStatus.Ok,
                Details = _env.EnvironmentName
            });

            // Connection string present
            var connString = _config.GetConnectionString("DefaultConnection");
            var hasConnString = !string.IsNullOrWhiteSpace(connString);

            checks.Add(new SetupCheck
            {
                Key = "connectionString",
                Name = "Database connection string",
                Status = hasConnString ? SetupCheckStatus.Ok : SetupCheckStatus.Error,
                Details = hasConnString
                    ? "ConnectionStrings:DefaultConnection is configured."
                    : "ConnectionStrings:DefaultConnection is missing or empty."
            });

            bool identityHealthy = false;
            bool coreHealthy = false;
            bool spHealthy = false;
            bool reportsHealthy = false;

            if (hasConnString)
            {
                identityHealthy = await AddDbChecksAsync(
                    _identityDb,
                    "identity",
                    "Identity database",
                    checks,
                    cancellationToken);

                coreHealthy = await AddDbChecksAsync(
                    _coreDb,
                    "core",
                    "Core SWIMS database",
                    checks,
                    cancellationToken);

                spHealthy = await AddDbChecksAsync(
                    _spDb,
                    "storedProcs",
                    "Stored procedures database",
                    checks,
                    cancellationToken);

                reportsHealthy = await AddDbChecksAsync(
                    _reportsDb,
                    "reports",
                    "Reporting database",
                    checks,
                    cancellationToken);
            }

            // Basic "bootstrap" configuration hints (non-fatal)
            var adminEmail = _config["AdminUser:Email"];
            checks.Add(new SetupCheck
            {
                Key = "adminUser",
                Name = "Bootstrap admin user",
                Status = string.IsNullOrWhiteSpace(adminEmail) ? SetupCheckStatus.Warning : SetupCheckStatus.Ok,
                Details = string.IsNullOrWhiteSpace(adminEmail)
                    ? "AdminUser:Email is not configured. Ensure your seed user configuration is correct."
                    : $"Admin user email is configured ({adminEmail})."
            });

            var emailMode = _config["Emailing:Mode"];
            checks.Add(new SetupCheck
            {
                Key = "emailing",
                Name = "Email delivery configuration",
                Status = string.IsNullOrWhiteSpace(emailMode) ? SetupCheckStatus.Warning : SetupCheckStatus.Ok,
                Details = string.IsNullOrWhiteSpace(emailMode)
                    ? "Emailing:Mode is not set. Email features may not work until configured."
                    : $"Emailing mode is set to '{emailMode}'."
            });

            // Optional advisory flag - we don't rely solely on it
            var setupFlag = _config.GetValue<bool?>("App:SetupCompleted") ?? false;

            var hasErrors = checks.Any(c => c.Status == SetupCheckStatus.Error);
            var dbHealthy = hasConnString && identityHealthy && coreHealthy && spHealthy && reportsHealthy;

            // Treat "configured" as: DB connectivity + all migrations applied + no hard errors
            var isConfigured = dbHealthy && !hasErrors;

            if (setupFlag && !isConfigured)
            {
                checks.Add(new SetupCheck
                {
                    Key = "setupFlagMismatch",
                    Name = "Setup flag vs environment",
                    Status = SetupCheckStatus.Warning,
                    Details = "App:SetupCompleted is true but environment checks are failing. " +
                              "Consider re-running migrations or updating configuration."
                });
            }

            return new SetupSummary
            {
                EnvironmentName = _env.EnvironmentName,
                IsConfigured = isConfigured,
                Checks = checks
            };
        }

        public async Task<bool> IsAppConfiguredAsync(CancellationToken cancellationToken = default)
        {
            var summary = await GetSummaryAsync(cancellationToken);
            return summary.IsConfigured;
        }

        private static async Task<bool> AddDbChecksAsync(
            DbContext db,
            string keyPrefix,
            string label,
            List<SetupCheck> checks,
            CancellationToken cancellationToken)
        {
            // Connectivity
            bool canConnect;
            try
            {
                canConnect = await db.Database.CanConnectAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                checks.Add(new SetupCheck
                {
                    Key = $"{keyPrefix}.connect",
                    Name = $"{label} connectivity",
                    Status = SetupCheckStatus.Error,
                    Details = $"Failed to connect: {ex.GetBaseException().Message}"
                });
                return false;
            }

            checks.Add(new SetupCheck
            {
                Key = $"{keyPrefix}.connect",
                Name = $"{label} connectivity",
                Status = canConnect ? SetupCheckStatus.Ok : SetupCheckStatus.Error,
                Details = canConnect
                    ? "Database is reachable."
                    : "Database server could not be reached. Check connection string and SQL Server."
            });

            if (!canConnect)
            {
                return false;
            }

            // Migrations
            try
            {
                var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                var hasPending = pending.Any();

                checks.Add(new SetupCheck
                {
                    Key = $"{keyPrefix}.migrations",
                    Name = $"{label} migrations",
                    Status = hasPending ? SetupCheckStatus.Warning : SetupCheckStatus.Ok,
                    Details = hasPending
                        ? $"There are {pending.Count} pending migrations. Apply them with your usual 'Update-Database' workflow."
                        : "All EF Core migrations are applied."
                });

                // For "healthy", we require no pending migrations
                return !hasPending;
            }
            catch (Exception ex)
            {
                checks.Add(new SetupCheck
                {
                    Key = $"{keyPrefix}.migrations",
                    Name = $"{label} migrations",
                    Status = SetupCheckStatus.Error,
                    Details = $"Could not inspect migrations: {ex.GetBaseException().Message}"
                });

                return false;
            }
        }
    }
}
