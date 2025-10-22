using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SWIMS.Services.Reporting;

namespace SWIMS.Controllers
{
    [ApiController]
    [Authorize(Policy = "ReportsView")]
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

        // 1) Query-style: /ssrs?%2FVCAS_Report%2FReportName&...
        [HttpGet]
        public Task<IActionResult> ProxyRoot() =>
            ProxyToReportServerAsync(remainder: null);

        // 2) Path-style: /ssrs/{**remainder}  e.g. /ssrs/Pages/ReportViewer.aspx, /ssrs/Reserved.ReportViewerWebControl.axd
        [HttpGet("{**remainder}")]
        public Task<IActionResult> ProxyPath(string? remainder) =>
            ProxyToReportServerAsync(remainder);

        private async Task<IActionResult> ProxyToReportServerAsync(string? remainder)
        {
            if (string.IsNullOrWhiteSpace(_opt.ReportServerUrl))
                return StatusCode(500, "ReportServerUrl is not configured.");

            // Build upstream URL
            var upstreamBase = _opt.ReportServerUrl.TrimEnd('/');            // http://server/ReportServer
            var qs = HttpContext.Request.QueryString.Value ?? string.Empty;  // ?%2FVCAS_Report%2F...
            var upstream = string.IsNullOrEmpty(remainder)
                ? $"{upstreamBase}{qs}"                                      // http://.../ReportServer?%2F...
                : $"{upstreamBase}/{remainder}{qs}";                         // http://.../ReportServer/Pages/...?... 

            using var req = new HttpRequestMessage(HttpMethod.Get, upstream);

            // Forward non-restricted headers (no cookies by default; the handler isn’t using them)
            foreach (var (key, val) in Request.Headers)
                if (!WebHeaderCollection.IsRestricted(key))
                    req.Headers.TryAddWithoutValidation(key, (IEnumerable<string>)val);

            using var upstreamRes = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

            // Mirror status
            Response.StatusCode = (int)upstreamRes.StatusCode;

            // Rewrite Location header on redirects to stay under /ssrs
            if (upstreamRes.Headers.Location is Uri loc)
            {
                var rewritten = RewriteLocation(loc);
                if (rewritten is not null)
                    Response.Headers["Location"] = rewritten;
            }

            // Copy headers (after potential Location rewrite)
            foreach (var h in upstreamRes.Headers)
                if (!string.Equals(h.Key, "Location", StringComparison.OrdinalIgnoreCase))
                    Response.Headers[h.Key] = h.Value.ToArray();
            foreach (var h in upstreamRes.Content.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            // (optional) turn upstream 401 into a clean server-side failure
            if (upstreamRes.StatusCode == HttpStatusCode.Unauthorized)
            {
                // You can log details here if you want.
                return StatusCode(502, "SSRS authentication failed for the relay identity.");
            }

            // Hop-by-hop cleanup
            Response.Headers.Remove("transfer-encoding");
            Response.Headers.Remove("Keep-Alive");
            Response.Headers.Remove("Connection");
            Response.Headers.Remove("Proxy-Connection");

            var mediaType = upstreamRes.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var stream = await upstreamRes.Content.ReadAsStreamAsync(HttpContext.RequestAborted);

            // For HTML (and only HTML), rewrite embedded ReportServer links to /{pathbase}/ssrs
            if (mediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: false);
                var html = await reader.ReadToEndAsync(HttpContext.RequestAborted);

                var proxyBase = GetProxyBasePath(); // includes PathBase, e.g. "/SWIMS/ssrs" or "/ssrs"

                // Build absolute prefixes from configured ReportServerUrl
                Uri rsUri;
                try { rsUri = new Uri(_opt.ReportServerUrl, UriKind.Absolute); }
                catch { rsUri = new Uri("http://invalid/ReportServer", UriKind.Absolute); }

                var host = rsUri.Host;
                var httpAbs = "http://" + host + (rsUri.Port > 0 && rsUri.Port != 80 ? ":" + rsUri.Port : "") + "/ReportServer";
                var httpsAbs = "https://" + host + (rsUri.Port > 0 && rsUri.Port != 443 ? ":" + rsUri.Port : "") + "/ReportServer";
                var schemeRel = "//" + host + (rsUri.Port > 0 && rsUri.IsDefaultPort == false ? ":" + rsUri.Port : "") + "/ReportServer";

                // Minimal, safe replacements (case-insensitive)
                html = html
                    // Relative paths
                    .Replace("href=\"/ReportServer", $"href=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace("href='/ReportServer", $"href='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace("src=\"/ReportServer", $"src=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace("src='/ReportServer", $"src='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace("action=\"/ReportServer", $"action=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace("action='/ReportServer", $"action='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace("url(/ReportServer", $"url({proxyBase}", StringComparison.OrdinalIgnoreCase)

                    // Absolute http/https and scheme-relative to the SSRS host
                    .Replace($"href=\"{httpAbs}", $"href=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"href=\"{httpsAbs}", $"href=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"href=\"{schemeRel}", $"href=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"href='{httpAbs}", $"href='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"href='{httpsAbs}", $"href='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"href='{schemeRel}", $"href='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"src=\"{httpAbs}", $"src=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"src=\"{httpsAbs}", $"src=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"src=\"{schemeRel}", $"src=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"src='{httpAbs}", $"src='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"src='{httpsAbs}", $"src='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"src='{schemeRel}", $"src='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"action=\"{httpAbs}", $"action=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"action=\"{httpsAbs}", $"action=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"action=\"{schemeRel}", $"action=\"{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"action='{httpAbs}", $"action='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"action='{httpsAbs}", $"action='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"action='{schemeRel}", $"action='{proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"url({httpAbs}", $"url({proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"url({httpsAbs}", $"url({proxyBase}", StringComparison.OrdinalIgnoreCase)
                    .Replace($"url({schemeRel}", $"url({proxyBase}", StringComparison.OrdinalIgnoreCase);

                Response.Headers["X-SWIMS-SSRS-Relay"] = "hit";
                // Return the rewritten HTML
                return Content(html, upstreamRes.Content.Headers.ContentType?.ToString() ?? "text/html; charset=utf-8");
            }

            // Stream everything else as-is (scripts, images, axd, PDFs, etc.)
            var contentType = upstreamRes.Content.Headers.ContentType?.ToString() ?? mediaType;
            Response.Headers["X-SWIMS-SSRS-Relay"] = "hit";
            return new FileStreamResult(stream, contentType);
        }

        private string? RewriteLocation(Uri loc)
        {
            // If SSRS redirects to http://server/ReportServer/..., rewrite to /{pathbase}/ssrs/...
            var proxyBase = GetProxyBasePath(); // e.g. "/SWIMS/ssrs" or "/ssrs"
            // Absolute to same-host SSRS
            if (loc.IsAbsoluteUri && loc.AbsolutePath.StartsWith("/ReportServer", StringComparison.OrdinalIgnoreCase))
                return proxyBase + loc.PathAndQuery.Substring("/ReportServer".Length);

            // Relative "/ReportServer/..."
            if (!loc.IsAbsoluteUri && loc.OriginalString.StartsWith("/ReportServer", StringComparison.OrdinalIgnoreCase))
                return proxyBase + loc.OriginalString.Substring("/ReportServer".Length);

            return null;
        }

        private string GetProxyBasePath()
        {
            // Respect app PathBase and ReverseProxyBasePath ("~/ssrs" or "/ssrs")
            var basePath = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value!.TrimEnd('/') : string.Empty;
            var configured = _opt.ReverseProxyBasePath?.Trim() ?? "/ssrs";
            var withoutTilde = configured.StartsWith("~") ? configured[1..] : configured; // "~/ssrs" -> "/ssrs"
            return (basePath + withoutTilde).TrimEnd('/');
        }
    }
}