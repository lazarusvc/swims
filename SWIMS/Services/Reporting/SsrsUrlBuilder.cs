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
            if (!string.IsNullOrWhiteSpace(_opt.UrlAccess.Zoom)) qp.Add($"rc:Zoom={Uri.EscapeDataString(_opt.UrlAccess.Zoom!)}");


            return qp.Count > 0 ? baseUrl + "&" + string.Join("&", qp) : baseUrl;
        }


        public string BuildExportUrl(string url, string format)
        => url + "&rs:Command=Render&rs:Format=" + Uri.EscapeDataString(format);
    }
}