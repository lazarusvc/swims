using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Cases;

public interface ICaseLifecycleService
{
    Task<CaseLifecycleResult> RefreshFromPrimaryApplicationAsync(
        int caseId,
        string? triggeredByUserId = null,
        CancellationToken ct = default);
}

public sealed record CaseLifecycleResult(
    bool Changed,
    string? OldStatus,
    string? NewStatus,
    string Message);
