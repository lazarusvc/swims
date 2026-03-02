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

        public async Task ExecuteByNameAsync(
            string workflowName,
            object? input = null,
            CancellationToken ct = default,
            bool throwOnUnavailable = false)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Elsa");

                // 1) Lookup workflow definition by name.
                // Prefer Published (launch-safe). Fall back to Latest to help you catch “not published” workflows during dev.
                var defId =
                    await ResolveWorkflowDefinitionIdAsync(client, workflowName, versionOptions: "Published", ct)
                    ?? await ResolveWorkflowDefinitionIdAsync(client, workflowName, versionOptions: "Latest", ct);


                if (string.IsNullOrWhiteSpace(defId))
                {
                    var msg = "Workflow/notifications are temporarily unavailable. Your changes were saved.";

                    _logger.LogWarning(
                        "Elsa workflow definition not found for name '{WorkflowName}' (Published/Latest). Base={BaseAddress}",
                        workflowName,
                        client.BaseAddress?.ToString() ?? "(null)");

                    if (throwOnUnavailable)
                        throw new ElsaWorkflowUnavailableException(workflowName, ElsaFailureReason.DefinitionNotFound, msg);

                    SetElsaWarningOncePerRequest(msg);
                    return;
                }


                // 2) Execute workflow by definition id (Elsa 3)
                var requestBody = new
                {
                    input // NOTE: property name matches Elsa's expected "input"
                };

                var relativeUrl = $"workflow-definitions/{defId}/execute";
                using var resp = await client.PostAsJsonAsync(relativeUrl, requestBody, ct);


                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync(ct);

                    _logger.LogWarning(
                        "Elsa execution failed ({StatusCode}) for workflow '{WorkflowName}'. Base={BaseAddress} Url={Url} Body={Body}",
                        (int)resp.StatusCode,
                        workflowName,
                        client.BaseAddress?.ToString() ?? "(null)",
                        relativeUrl,
                        body);

                    var msg = "Workflow/notifications are temporarily unavailable. Your changes were saved.";

                    if (throwOnUnavailable)
                        throw new ElsaWorkflowUnavailableException(workflowName, ElsaFailureReason.ExecutionFailed, msg, statusCode: (int)resp.StatusCode);

                    SetElsaWarningOncePerRequest(msg);
                    return;
                }

            }
            catch (HttpRequestException ex)
            {
                // Network / connection issues (e.g., connection refused when Elsa is down)
                HandleElsaUnavailable(ex, workflowName, throwOnUnavailable);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout / cancellation (e.g., CTS timeout we added in the Hangfire job)
                HandleElsaUnavailable(ex, workflowName, throwOnUnavailable);
            }
            catch (Exception ex)
            {
                // Fail open, but log it (don’t crash your business action)
                _logger.LogWarning(ex, "Unexpected error calling Elsa; skipping workflow '{WorkflowName}'.", workflowName);

                var msg = "Workflow/notifications are temporarily unavailable. Your changes were saved.";

                if (throwOnUnavailable)
                    throw new ElsaWorkflowUnavailableException(workflowName, ElsaFailureReason.ExecutionFailed, msg, ex);

                SetElsaWarningOncePerRequest(msg);
            }
        }

        private async Task<string?> ResolveWorkflowDefinitionIdAsync(
    HttpClient client,
    string workflowName,
    string versionOptions,
    CancellationToken ct)
        {
            // We DO NOT trust server-side filtering to return an exact match.
            // Fetch a page and match by name locally.
            var url =
                $"workflow-definitions?versionOptions={Uri.EscapeDataString(versionOptions)}" +
                $"&Page=0&PageSize=50&OrderDirection=Descending";

            using var resp = await client.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "Elsa definition lookup failed ({StatusCode}). Base={BaseAddress} Url={Url} Body={Body}",
                    (int)resp.StatusCode,
                    client.BaseAddress?.ToString() ?? "(null)",
                    url,
                    body);

                return null;
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            if (!TryGetPropertyIgnoreCase(doc.RootElement, "items", out var items) || items.ValueKind != JsonValueKind.Array)
                return null;

            JsonElement? bestMatch = null;

            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                if (!TryGetPropertyIgnoreCase(item, "name", out var nameEl) || nameEl.ValueKind != JsonValueKind.String)
                    continue;

                var name = nameEl.GetString();
                if (!string.Equals(name, workflowName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // If Elsa provides isPublished/isLatest, use them to prefer the best candidate.
                var isPublished = TryGetBoolIgnoreCase(item, "isPublished");
                var isLatest = TryGetBoolIgnoreCase(item, "isLatest");

                // Prefer published when asking for Published.
                if (string.Equals(versionOptions, "Published", StringComparison.OrdinalIgnoreCase) && isPublished != true)
                    continue;

                // Prefer "latest" version if multiple entries exist.
                if (bestMatch == null)
                {
                    bestMatch = item;
                }
                else
                {
                    var bestIsLatest = TryGetBoolIgnoreCase(bestMatch.Value, "isLatest");
                    if (bestIsLatest != true && isLatest == true)
                        bestMatch = item;
                }
            }

            if (bestMatch == null)
                return null;

            // IMPORTANT:
            // - Execute endpoint expects definitionId: /workflow-definitions/{definitionId}/execute
            // - id is definitionVersionId (version record)
            if (TryGetPropertyIgnoreCase(bestMatch.Value, "definitionId", out var defIdEl) && defIdEl.ValueKind == JsonValueKind.String)
                return defIdEl.GetString();

            // Fallback (should not be used with your Elsa responses)
            if (TryGetPropertyIgnoreCase(bestMatch.Value, "id", out var idEl) && idEl.ValueKind == JsonValueKind.String)
                return idEl.GetString();

            return null;
        }

        private static bool? TryGetBoolIgnoreCase(JsonElement element, string name)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (prop.Value.ValueKind == JsonValueKind.True) return true;
                if (prop.Value.ValueKind == JsonValueKind.False) return false;

                return null;
            }

            return null;
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

        private void HandleElsaUnavailable(Exception ex, string workflowName, bool throwOnUnavailable)
        {
            _logger.LogWarning(ex, "Elsa is unavailable; skipping workflow '{WorkflowName}'.", workflowName);

            var msg =
                "Workflow/notifications are temporarily unavailable (Elsa is offline). " +
                "Your changes were saved, but automated notifications may not be delivered.";

            if (throwOnUnavailable)
                throw new ElsaWorkflowUnavailableException(workflowName, ElsaFailureReason.Offline, msg, ex);

            SetElsaWarningOncePerRequest(msg);
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
