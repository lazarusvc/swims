using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace SWIMS.Services.Elsa;

public sealed class ElsaWorkflowJobs
{
    private readonly IElsaWorkflowClient _client;
    private readonly ILogger<ElsaWorkflowJobs> _logger;

    public ElsaWorkflowJobs(IElsaWorkflowClient client, ILogger<ElsaWorkflowJobs> logger)
    {
        _client = client;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 0)] // IMPORTANT: avoid accidental duplicate notifs on retry
    public async Task ExecuteByNameAsync(string workflowName, string? inputJson)
    {
        if (string.IsNullOrWhiteSpace(workflowName))
            return;

        object? input = null;

        if (!string.IsNullOrWhiteSpace(inputJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(inputJson);
                input = doc.RootElement.Clone(); // safe after dispose
            }
            catch
            {
                input = inputJson; // fallback scalar
            }
        }

        try
        {
            // Bound the damage if Elsa is slow/offline.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            await _client.ExecuteByNameAsync(workflowName, input, cts.Token);
        }
        catch (Exception ex)
        {
            // Keep job "successful" (no retries), but log for diagnostics.
            _logger.LogWarning(ex, "Elsa workflow job failed. Workflow={WorkflowName}", workflowName);
        }
    }
}