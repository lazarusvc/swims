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

        [HttpPost]
        public Task<IActionResult> ProxyRootPost() =>
            ProxyToReportServerAsync(remainder: null);

        [HttpPost("{**remainder}")]
        public Task<IActionResult> ProxyPathPost(string? remainder) =>
            ProxyToReportServerAsync(remainder);

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

            var upstreamBase = _opt.ReportServerUrl.TrimEnd('/');
            var qs = HttpContext.Request.QueryString.Value ?? string.Empty;
            var upstream = string.IsNullOrEmpty(remainder)
                ? $"{upstreamBase}{qs}"
                : $"{upstreamBase}/{remainder}{qs}";

            // Build outbound request with the SAME HTTP method
            var method = new HttpMethod(Request.Method);
            using var req = new HttpRequestMessage(method, upstream);

            // Forward headers (skip restricted and hop-by-hop)
            foreach (var (key, val) in Request.Headers)
            {
                if (string.Equals(key, "Host", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(key, "Connection", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(key, "Proxy-Connection", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(key, "Keep-Alive", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(key, "Transfer-Encoding", StringComparison.OrdinalIgnoreCase)) continue;

                if (!System.Net.WebHeaderCollection.IsRestricted(key))
                    req.Headers.TryAddWithoutValidation(key, (IEnumerable<string>)val);
            }

            // If there is a body, forward it and copy content headers
            if (Request.ContentLength.HasValue && Request.ContentLength.Value > 0)
            {
                req.Content = new StreamContent(Request.Body);
                foreach (var (key, val) in Request.Headers)
                {
                    // Only typical content headers
                    if (key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                        req.Content.Headers.TryAddWithoutValidation(key, (IEnumerable<string>)val);
                }
            }

            using var upstreamRes = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

            // Mirror status
            Response.StatusCode = (int)upstreamRes.StatusCode;

            // Rewrite Location to stay under /ssrs
            if (upstreamRes.Headers.Location is Uri loc)
            {
                var rewritten = RewriteLocation(loc);
                if (rewritten is not null)
                {
                    // Make it absolute for robustness
                    var absolute = $"{Request.Scheme}://{Request.Host}{rewritten}";
                    Response.Headers["Location"] = absolute;
                }
            }

            // Copy headers (after Location handling)
            foreach (var h in upstreamRes.Headers)
                if (!string.Equals(h.Key, "Location", StringComparison.OrdinalIgnoreCase))
                    Response.Headers[h.Key] = h.Value.ToArray();
            foreach (var h in upstreamRes.Content.Headers)
                Response.Headers[h.Key] = h.Value.ToArray();

            // Normalize hop-by-hop
            Response.Headers.Remove("transfer-encoding");
            Response.Headers.Remove("Keep-Alive");
            Response.Headers.Remove("Connection");
            Response.Headers.Remove("Proxy-Connection");

            if (upstreamRes.StatusCode == HttpStatusCode.Unauthorized)
                return StatusCode(502, "SSRS authentication failed for the relay identity.");

            var mediaType = upstreamRes.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            if (mediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
            {
                // Fully buffer, rewrite, then write out
                var html = await upstreamRes.Content.ReadAsStringAsync(HttpContext.RequestAborted);
                var proxyBase = GetProxyBasePath();

                // (existing rewrite logic retained)
                html = RewriteHtmlToProxy(html, proxyBase);

                Response.Headers["X-SWIMS-SSRS-Relay"] = "hit";
                Response.ContentType = upstreamRes.Content.Headers.ContentType?.ToString() ?? "text/html; charset=utf-8";
                await Response.WriteAsync(html, HttpContext.RequestAborted);
                return new EmptyResult();
            }
            else
            {
                // Stream non-HTML bytes directly to the client (no premature dispose)
                Response.Headers["X-SWIMS-SSRS-Relay"] = "hit";
                Response.ContentType = upstreamRes.Content.Headers.ContentType?.ToString() ?? mediaType;
                await upstreamRes.Content.CopyToAsync(Response.Body, HttpContext.RequestAborted);
                return new EmptyResult();
            }
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

        private string RewriteHtmlToProxy(string html, string proxyBase)
        {
            // Safety
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(proxyBase))
                return html;

            // 1) Rewrite absolute links that point to .../ReportServer to the proxy base.
            //    e.g. href="http://server/ReportServer/xyz" -> href="/ssrs/xyz"
            html = Regex.Replace(
                html,
                @"(?is)(?<p>\b(?:href|src|action)\s*=\s*[""'])(?<u>https?://[^""']*/ReportServer(?<tail>[^""']*))",
                m =>
                {
                    var p = m.Groups["p"].Value;
                    var u = m.Groups["u"].Value;
                    var idx = u.IndexOf("/ReportServer", StringComparison.OrdinalIgnoreCase);
                    var tail = idx >= 0 ? u.Substring(idx + "/ReportServer".Length) : "";
                    return p + proxyBase + tail;
                });

            // 2) Rewrite relative links that start with /ReportServer to the proxy base.
            //    e.g. src="/ReportServer/Reserved.ReportViewerWebControl.axd?..." -> src="/ssrs/Reserved.ReportViewerWebControl.axd?..."
            html = Regex.Replace(
                html,
                @"(?is)(?<p>\b(?:href|src|action)\s*=\s*[""'])(?<u>/ReportServer(?<tail>[^""']*))",
                m => m.Groups["p"].Value + proxyBase + m.Groups["tail"].Value
            );

            // 3) Fix <base href=".../ReportServer/..."> to keep all relative fetches under the proxy.
            html = Regex.Replace(
                html,
                @"(?is)<base\s+href=[""'][^""']*/ReportServer[^""']*[""'][^>]*>",
                _ => $"<base href=\"{proxyBase}/\">"
            );

            // 4) Catch naked JS string usages like '/ReportServer?...'
            html = Regex.Replace(
                html,
                @"(?i)(['""])/ReportServer",
                m => $"{m.Groups[1].Value}{proxyBase}"
            );

            return html;
        }

    }
}