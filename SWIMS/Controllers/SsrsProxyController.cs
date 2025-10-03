using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWIMS.Controllers
{
    [ApiController]
    [AllowAnonymous]                  // keep this if you have global auth
    [Route("ssrs")]                   // base route => /ssrs
    public class SsrsProxyController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<SsrsProxyController> _logger;
        private readonly string _baseUrl;

        public SsrsProxyController(IHttpClientFactory http, ILogger<SsrsProxyController> logger, IConfiguration cfg)
        {
            _http = http;
            _logger = logger;
            _baseUrl = (cfg["SSRS:BaseUrl"] ?? "").TrimEnd('/')
                       ?? throw new InvalidOperationException("Missing SSRS:BaseUrl in configuration.");
        }

        // GET /ssrs?%2FReportPath&... (HTML shell)
        [HttpGet("")]
        public async Task<IActionResult> RenderAsync()
        {
            var client = _http.CreateClient("ssrs-proxy");
            var upstreamUrl = _baseUrl + Request.QueryString.Value;

            using var upstream = await client.GetAsync(upstreamUrl, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

            if (!upstream.IsSuccessStatusCode)
            {
                var body = await SafeReadStringAsync(upstream, 2048);
                _logger.LogError("SSRS HTML fetch failed {Status} for {Url}. Body (first 2k): {Body}",
                    (int)upstream.StatusCode, upstreamUrl, body);
                return StatusCode((int)upstream.StatusCode, "Upstream SSRS error fetching HTML.");
            }

            var contentTypeHeader = upstream.Content.Headers.ContentType?.ToString() ?? "text/html; charset=utf-8";
            var mediaType = upstream.Content.Headers.ContentType?.MediaType ?? "text/html";

            if (mediaType.Contains("html", StringComparison.OrdinalIgnoreCase))
            {
                var html = await upstream.Content.ReadAsStringAsync(HttpContext.RequestAborted);
                html = RewriteAssetUrls(html);
                Response.Headers["X-SWIMS-SSRS-Relay"] = "hit";
                // (Optional) avoid browser auth popups if SSRS sent this
                Response.Headers.Remove("WWW-Authenticate");
                return Content(html, contentTypeHeader);
            }

            // Non-HTML fallback (rare)
            var bytes = await upstream.Content.ReadAsByteArrayAsync(HttpContext.RequestAborted);
            if (upstream.Content.Headers.ContentDisposition is { } cd)
                Response.Headers["Content-Disposition"] = cd.ToString();
            Response.Headers["X-SWIMS-SSRS-Relay"] = "hit";
            return File(bytes, contentTypeHeader);
        }

        // GET /ssrs/resource?u=<encoded upstream url>
        [HttpGet("resource")]
        public async Task<IActionResult> ResourceAsync([FromQuery] string u)
        {
            if (string.IsNullOrWhiteSpace(u))
                return BadRequest("Missing 'u'.");

            var target = NormalizeUpstreamUrl(u);
            var client = _http.CreateClient("ssrs-proxy");

            using var upstream = await client.GetAsync(target, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

            if (!upstream.IsSuccessStatusCode)
            {
                var body = await SafeReadStringAsync(upstream, 2048);
                _logger.LogError("SSRS asset fetch failed {Status} for {Url}. Body (first 2k): {Body}",
                    (int)upstream.StatusCode, target, body);
                return StatusCode((int)upstream.StatusCode, "Upstream SSRS error fetching asset.");
            }

            var bytes = await upstream.Content.ReadAsByteArrayAsync(HttpContext.RequestAborted);
            var contentType = upstream.Content.Headers.ContentType?.ToString()
                              ?? upstream.Content.Headers.ContentType?.MediaType
                              ?? "application/octet-stream";

            if (upstream.Content.Headers.ContentDisposition is { } cd)
                Response.Headers["Content-Disposition"] = cd.ToString();

            Response.Headers["X-SWIMS-SSRS-Relay"] = "hit";
            return File(bytes, contentType); // lets ASP.NET set Content-Length
        }

        // ------- helpers -------

        private string NormalizeUpstreamUrl(string u)
        {
            if (Uri.TryCreate(u, UriKind.Absolute, out _)) return u;
            var decoded = WebUtility.UrlDecode(u);
            if (decoded.StartsWith("?", StringComparison.Ordinal)) return _baseUrl + u;
            return _baseUrl + "?" + u;
        }

        private string RewriteAssetUrls(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            var attrRx = new Regex("(?:\\s(?:src|href))\\s*=\\s*\"([^\"]+)\"",
                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var cssUrlRx = new Regex("url\\((['\"]?)([^)'\"]+)\\1\\)",
                                     RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string Rewriter(string url)
            {
                var decoded = WebUtility.HtmlDecode(url);
                var looksLikeSsrs =
                    decoded.Contains("ReportServer", StringComparison.OrdinalIgnoreCase) ||
                    decoded.Contains("rs:ImageID=", StringComparison.OrdinalIgnoreCase) ||
                    decoded.Contains("ResourceStream", StringComparison.OrdinalIgnoreCase);

                if (!looksLikeSsrs) return url;
                return "/ssrs/resource?u=" + WebUtility.UrlEncode(decoded);
            }

            html = attrRx.Replace(html, m =>
            {
                var original = m.Groups[1].Value;
                var repl = Rewriter(original);
                return m.Value.Replace(original, repl);
            });

            html = cssUrlRx.Replace(html, m =>
            {
                var original = m.Groups[2].Value;
                var repl = Rewriter(original);
                var quote = m.Groups[1].Value;
                return $"url({quote}{repl}{quote})";
            });

            return html;
        }

        private static async Task<string> SafeReadStringAsync(HttpResponseMessage msg, int maxChars)
        {
            try
            {
                var s = await msg.Content.ReadAsStringAsync();
                return s.Length > maxChars ? s[..maxChars] : s;
            }
            catch { return "<unreadable>"; }
        }
    }
}