using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Elsa
{
    public class ElsaWorkflowClient : IElsaWorkflowClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ElsaWorkflowClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public ElsaWorkflowClient(
            IHttpClientFactory httpClientFactory,
            ILogger<ElsaWorkflowClient> logger,
            IHttpContextAccessor httpContextAccessor,
            ITempDataDictionaryFactory tempDataFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _tempDataFactory = tempDataFactory;
        }

        public async Task ExecuteByNameAsync(string workflowName, object? input = null, CancellationToken ct = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Elsa");

                // 1) Lookup workflow definition by name
                var defId = await ResolveWorkflowDefinitionIdAsync(client, workflowName, ct);
                if (string.IsNullOrWhiteSpace(defId))
                {
                    _logger.LogWarning("Elsa workflow definition not found for name '{WorkflowName}'.", workflowName);
                    return;
                }

                // 2) Execute workflow by definition id
                var payload = new
                {
                    WorkflowDefinitionId = defId,
                    Input = input
                };

                using var resp = await client.PostAsJsonAsync("/v1/workflow-instances", payload, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Elsa execution returned non-success status {StatusCode} for workflow '{WorkflowName}'.",
                        (int)resp.StatusCode,
                        workflowName);
                }
            }
            catch (HttpRequestException ex)
            {
                HandleElsaUnavailable(ex, workflowName);
            }
            catch (TaskCanceledException ex)
            {
                HandleElsaUnavailable(ex, workflowName);
            }
            catch (Exception ex)
            {
                // Fail open, but log it (don’t crash your business action)
                _logger.LogWarning(ex, "Unexpected error calling Elsa; skipping workflow '{WorkflowName}'.", workflowName);
                SetElsaWarningOncePerRequest("Workflow/notifications are temporarily unavailable. Your changes were saved.");
            }
        }

        private async Task<string?> ResolveWorkflowDefinitionIdAsync(HttpClient client, string workflowName, CancellationToken ct)
        {
            // Elsa endpoint shape varies across versions; we parse loosely:
            // Expect { items: [ { id: "..." }, ... ] } (case-insensitive)
            var url = $"/v1/workflow-definitions?name={Uri.EscapeDataString(workflowName)}";

            using var resp = await client.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
                return null;

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            if (!TryGetPropertyIgnoreCase(doc.RootElement, "items", out var items) || items.ValueKind != JsonValueKind.Array)
                return null;

            if (items.GetArrayLength() == 0)
                return null;

            var first = items[0];
            if (first.ValueKind != JsonValueKind.Object)
                return null;

            if (!TryGetPropertyIgnoreCase(first, "id", out var idEl) || idEl.ValueKind != JsonValueKind.String)
                return null;

            return idEl.GetString();
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private void HandleElsaUnavailable(Exception ex, string workflowName)
        {
            _logger.LogWarning(ex, "Elsa is unavailable; skipping workflow '{WorkflowName}'.", workflowName);

            SetElsaWarningOncePerRequest(
                "Workflow/notifications are temporarily unavailable (Elsa is offline). Your changes were saved, but automated notifications may not be delivered.");
        }

        private void SetElsaWarningOncePerRequest(string message)
        {
            var http = _httpContextAccessor.HttpContext;
            if (http == null) return;

            const string itemKey = "__elsa_unavailable_warned";
            if (http.Items.ContainsKey(itemKey)) return;

            http.Items[itemKey] = true;
            var tempData = _tempDataFactory.GetTempData(http);
            tempData["Elsa.Warning"] = message;
        }
    }
}
