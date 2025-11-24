using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Setup
{
    public interface ISetupStateService
    {
        /// <summary>
        /// Returns a detailed breakdown of environment, DB connectivity and migrations.
        /// </summary>
        Task<SetupSummary> GetSummaryAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lightweight check used by middleware to decide whether to redirect to /Setup.
        /// </summary>
        Task<bool> IsAppConfiguredAsync(CancellationToken cancellationToken = default);
    }
}
