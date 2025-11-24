namespace SWIMS.Services.Elsa;

public interface IElsaWorkflowClient
{
    Task ExecuteByNameAsync(string workflowName, object? input = null, CancellationToken ct = default);
}
