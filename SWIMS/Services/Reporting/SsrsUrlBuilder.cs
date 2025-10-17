using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace SWIMS.Services.Reporting
{
    public interface ISsrsUrlBuilder
    {
        string BuildUrl(string reportName, IEnumerable<KeyValuePair<string, string>> parameters, string? pathOverride = null);
        string BuildExportUrl(string baseUrl, string format);
    }


    public class SsrsUrlBuilder : ISsrsUrlBuilder
    {
        private readonly ReportingOptions _opt;
        public SsrsUrlBuilder(IOptions<ReportingOptions> opt) => _opt = opt.Value;


        public string BuildUrl(string reportName, IEnumerable<KeyValuePair<string, string>> parameters, string? pathOverride = null)
        {
            var folder = string.IsNullOrWhiteSpace(pathOverride) ? _opt.ReportPathRoot : pathOverride;
            var reportPath = (folder.TrimEnd('/') + "/" + reportName.Trim()).Replace(".rdl", string.Empty);

            // Start with the classic ReportServer URL
            var baseUrl = $"{_opt.ReportServerUrl}?{Uri.EscapeDataString(reportPath)}";

            var qp = new List<string>();
            foreach (var kv in parameters ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;
                var key = Uri.EscapeDataString(kv.Key.Trim());
                var val = Uri.EscapeDataString((kv.Value ?? string.Empty).Trim());
                qp.Add($"{key}={val}");
            }
            if (_opt.UrlAccess.HideToolbar) qp.Add("rc:Toolbar=false");
            if (_opt.UrlAccess.UseEmbed) qp.Add("rs:Embed=true");
            if (!string.IsNullOrWhiteSpace(_opt.UrlAccess.Zoom))
                qp.Add($"rc:Zoom={Uri.EscapeDataString(_opt.UrlAccess.Zoom!)}");

            var fullUrl = qp.Count > 0 ? baseUrl + "&" + string.Join("&", qp) : baseUrl;

            // Rewrite to same-origin relay (/ssrs) if enabled
            if (_opt.UseReverseProxy)
            {
                var proxyBase = string.IsNullOrWhiteSpace(_opt.ReverseProxyBasePath) ? "/ssrs" : _opt.ReverseProxyBasePath;
                var rootAware = proxyBase.StartsWith("~") ? proxyBase : "~" + proxyBase;

                try
                {
                    var u = new Uri(fullUrl, UriKind.Absolute);
                    const string reportServerPrefix = "/ReportServer";
                    var remainder = u.PathAndQuery.StartsWith(reportServerPrefix, StringComparison.OrdinalIgnoreCase)
                        ? u.PathAndQuery.Substring(reportServerPrefix.Length)
                        : u.PathAndQuery;
                    return $"{rootAware.TrimEnd('/')}{remainder}";
                }
                catch
                {
                    var i = fullUrl.IndexOf("/ReportServer", StringComparison.OrdinalIgnoreCase);
                    if (i >= 0) return rootAware.TrimEnd('/') + fullUrl.Substring(i + "/ReportServer".Length);
                }
            }


            return fullUrl; // fallback: direct ReportServer
        }


        public string BuildExportUrl(string url, string format)
        => url + "&rs:Command=Render&rs:Format=" + Uri.EscapeDataString(format);
    }
}