using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SWIMS.Services.Elsa;

public class ElsaWorkflowClient : IElsaWorkflowClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ElsaWorkflowClient> _logger;

    public ElsaWorkflowClient(IHttpClientFactory httpClientFactory, ILogger<ElsaWorkflowClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private record ElsaLink(
        [property: JsonPropertyName("href")] string Href,
        [property: JsonPropertyName("rel")] string Rel,
        [property: JsonPropertyName("method")] string Method);

    private record ElsaWorkflowDefinitionSummary(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("definitionId")] string DefinitionId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("links")] ElsaLink[] Links);

    private record ElsaWorkflowDefinitionList(
        [property: JsonPropertyName("items")] ElsaWorkflowDefinitionSummary[] Items,
        [property: JsonPropertyName("totalCount")] int TotalCount);

    public async Task ExecuteByNameAsync(string workflowName, object? input = null, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Elsa");

        // 1. Fetch latest defs and find by name
        var defs = await client.GetFromJsonAsync<ElsaWorkflowDefinitionList>(
            "workflow-definitions?versionOptions=Latest&Page=0&PageSize=100&OrderDirection=Ascending",
            ct);

        var wf = defs?.Items?.SingleOrDefault(x => x.Name == workflowName);
        if (wf is null)
        {
            _logger.LogWarning("Elsa workflow '{WorkflowName}' not found.", workflowName);
            return;
        }

        var executeHref = wf.Links.FirstOrDefault(l => l.Rel == "execute")?.Href;
        if (string.IsNullOrWhiteSpace(executeHref))
        {
            _logger.LogWarning("Elsa workflow '{WorkflowName}' has no execute link.", workflowName);
            return;
        }

        var relativeUrl = executeHref.TrimStart('/');

        var body = new { input = input ?? new { } };

        _logger.LogInformation("Executing Elsa workflow {WorkflowName} via {Url}.", workflowName, $"{client.BaseAddress}{relativeUrl}");

        var response = await client.PostAsJsonAsync(relativeUrl, body, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Elsa workflow '{WorkflowName}' execution failed: {Status} {Content}",
                workflowName, (int)response.StatusCode, content);
        }
        else
        {
            _logger.LogInformation("Elsa workflow '{WorkflowName}' executed successfully. Status={Status}",
                workflowName, response.StatusCode);
        }
    }
}
