using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;

namespace SWIMS.Controllers.Dev;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("dev/elsa-test")]
[Authorize(Roles = "SuperAdmin")]
public sealed class ElsaTestController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;

    public ElsaTestController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
    {
        _httpClientFactory = httpClientFactory;
        _env = env;
    }

    private IActionResult? BlockIfNotDev()
        => _env.IsDevelopment() ? null : NotFound();

    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions(CancellationToken ct)
    {
        var blocked = BlockIfNotDev();
        if (blocked is not null) return blocked;

        var client = _httpClientFactory.CreateClient("Elsa");
        var response = await client.GetAsync(
            "workflow-definitions?versionOptions=Latest&Page=0&PageSize=20&OrderDirection=Ascending", ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();
        return Content(body, "application/json");
    }

    [HttpPost("run/{definitionId}")]
    public async Task<IActionResult> RunWorkflow(string definitionId, CancellationToken ct)
    {
        var blocked = BlockIfNotDev();
        if (blocked is not null) return blocked;

        var client = _httpClientFactory.CreateClient("Elsa");

        var recipient =
            User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User?.Identity?.Name
            ?? "1";

        var requestBody = new
        {
            input = new
            {
                Channel = "InApp",
                Recipient = recipient,
                Subject = $"Dynamic Elsa → SWIMS ({DateTime.UtcNow:O})",
                Body = $"Hello from SWIMS at {DateTime.UtcNow:O}",
                MetadataJson = "{\"type\":\"System\",\"eventKey\":\"Swims.Events.Dev.ManualTest\",\"metadata\":{\"source\":\"swims-dev\"}}"
            }
        };

        var relativeUrl = $"workflow-definitions/{definitionId}/execute";
        var response = await client.PostAsJsonAsync(relativeUrl, requestBody, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        return Content($"Status: {(int)response.StatusCode}\n\n{content}", "text/plain");
    }
}