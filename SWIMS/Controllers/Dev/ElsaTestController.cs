using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace YourSwimsNamespace.Controllers.Dev
{
    [ApiController]
    [Route("dev/elsa-test")]
    // Optionally lock this down to local/dev-only:
    // [Authorize(Policy = "DevOnly")]
    [AllowAnonymous]
    public class ElsaTestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ElsaTestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("definitions")]
        public async Task<IActionResult> GetDefinitions()
        {
            var client = _httpClientFactory.CreateClient("Elsa");

            var response = await client.GetAsync(
                "workflow-definitions?versionOptions=Latest&Page=0&PageSize=20&OrderDirection=Ascending");

            var body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            // Just forward Elsa's JSON
            return Content(body, "application/json");
        }

        [HttpPost("run/{definitionId}")]
        [AllowAnonymous] // dev-only
        public async Task<IActionResult> RunWorkflow(string definitionId)
        {
            var client = _httpClientFactory.CreateClient("Elsa");

            var recipient =
                User?.FindFirstValue(ClaimTypes.NameIdentifier) // preferred: int userId as string
                ?? User?.Identity?.Name                          // fallback: username (if your app sets it)
                ?? "1";                                          // last resort fallback (replace with a known userId)

            var requestBody = new
            {
                input = new
                {
                    Channel = "InApp",
                    Recipient = recipient,
                    Subject = $"Dynamic Elsa → SWIMS ({DateTime.UtcNow:O})",
                    Body = $"Hello from SWIMS at {DateTime.UtcNow:O}",
                    MetadataJson = "{\"source\":\"swims-dev\",\"event\":\"ManualTest\"}"
                }
            };

            var relativeUrl = $"workflow-definitions/{definitionId}/execute";

            // Debug log to be 100% sure of the URL
            Console.WriteLine($"Calling Elsa: {client.BaseAddress}{relativeUrl}");

            var response = await client.PostAsJsonAsync(relativeUrl, requestBody);

            var content = await response.Content.ReadAsStringAsync();

            // For now, don't throw on 400 – just show what Elsa returned
            return Content(
                $"Status: {(int)response.StatusCode}\n\n{content}",
                "text/plain");
        }
    }
}