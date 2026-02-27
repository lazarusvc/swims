using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Elsa;

public sealed class ElsaWorkflowQueue : IElsaWorkflowQueue
{
    private readonly IBackgroundJobClient _jobs;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public ElsaWorkflowQueue(IBackgroundJobClient jobs)
        => _jobs = jobs;

    public Task EnqueueByNameAsync(string workflowName, object? input = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(workflowName))
            return Task.CompletedTask;

        string? inputJson = null;

        if (input is not null)
        {
            inputJson = input is string s
                ? s
                : JsonSerializer.Serialize(input, _json);
        }

        _jobs.Create(
            Job.FromExpression<ElsaWorkflowJobs>(j => j.ExecuteByNameAsync(workflowName, inputJson)),
            new EnqueuedState("default"));

        return Task.CompletedTask;
    }
}