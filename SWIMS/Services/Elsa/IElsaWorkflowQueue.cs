using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Elsa;

public interface IElsaWorkflowQueue
{
    Task EnqueueByNameAsync(string workflowName, object? input = null, CancellationToken ct = default);
}