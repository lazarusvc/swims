using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SWIMS.Services.Reporting;

namespace SWIMS.Controllers;

[ApiController]
[AllowAnonymous] // mark as public; your policy system can also allow /ssrs
[Route("ssrs")]
public class SsrsProxyController : ControllerBase
{
    private readonly HttpClient _http;
    private readonly ReportingOptions _opt;

    public SsrsProxyController(IHttpClientFactory httpFactory, IOptions<ReportingOptions> opt)
    {
        _http = httpFactory.CreateClient("ssrs-proxy");
        _opt = opt.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Proxy()
    {
        if (string.IsNullOrWhiteSpace(_opt.ReportServerUrl))
            return StatusCode(500, "ReportServerUrl is not configured.");

        // Build upstream: http://host/ReportServer?%2F...
        var upstream = _opt.ReportServerUrl.TrimEnd('/') + HttpContext.Request.QueryString.Value;

        using var req = new HttpRequestMessage(HttpMethod.Get, upstream);
        foreach (var h in Request.Headers)
        {
            if (!WebHeaderCollection.IsRestricted(h.Key))
                req.Headers.TryAddWithoutValidation(h.Key, (IEnumerable<string>)h.Value);
        }

        using var upstreamRes = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

        Response.StatusCode = (int)upstreamRes.StatusCode;
        foreach (var h in upstreamRes.Headers) Response.Headers[h.Key] = h.Value.ToArray();
        foreach (var h in upstreamRes.Content.Headers) Response.Headers[h.Key] = h.Value.ToArray();

        // hop-by-hop
        Response.Headers.Remove("transfer-encoding");
        Response.Headers.Remove("Keep-Alive");
        Response.Headers.Remove("Connection");
        Response.Headers.Remove("Proxy-Connection");

        var stream = await upstreamRes.Content.ReadAsStreamAsync(HttpContext.RequestAborted);
        var contentType = upstreamRes.Content.Headers.ContentType?.ToString() ?? "text/html";
        return new FileStreamResult(stream, contentType);
    }
}