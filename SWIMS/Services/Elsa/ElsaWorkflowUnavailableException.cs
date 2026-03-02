namespace SWIMS.Services.Elsa;

public enum ElsaFailureReason
{
    Offline,
    DefinitionNotFound,
    ExecutionFailed
}

public sealed class ElsaWorkflowUnavailableException : Exception
{
    public string WorkflowName { get; }
    public ElsaFailureReason Reason { get; }
    public int? StatusCode { get; }

    public ElsaWorkflowUnavailableException(
        string workflowName,
        ElsaFailureReason reason,
        string message,
        Exception? inner = null,
        int? statusCode = null) : base(message, inner)
    {
        WorkflowName = workflowName;
        Reason = reason;
        StatusCode = statusCode;
    }
}