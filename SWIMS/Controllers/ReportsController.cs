using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SWIMS.Data.Reports;
using SWIMS.Services.Reporting;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    [Authorize(Policy = "ReportsView")]
    public class ReportsController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly ISsrsUrlBuilder _url;
        private readonly IOptions<ReportingOptions> _opt;

        public ReportsController(SwimsReportsDbContext db, ISsrsUrlBuilder url, IOptions<ReportingOptions> opt)
        { _db = db; _url = url; _opt = opt; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roleIds = User.Claims.Where(c => c.Type == ClaimTypes.Role)
                                     .Select(c => c.Value).ToList();

            var reports = await _db.SwReports
                .Where(r => roleIds.Contains(r.RoleId))
                .OrderBy(r => r.Desc ?? r.Name)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(int id)
        {
            var rpt = await _db.SwReports.Include(r => r.Params).FirstOrDefaultAsync(r => r.Id == id);
            if (rpt == null) return NotFound();

            var parms = rpt.ParamCheck
                ? rpt.Params.Select(p => new KeyValuePair<string, string>(p.ParamKey.Trim(), p.ParamValue.Trim()))
                : Enumerable.Empty<KeyValuePair<string, string>>();

            var url = _url.BuildUrl(rpt.Name, parms, rpt.PathOverride);
            return View("Viewer", model: url);
        }

        // returns a page that embeds Inline() in an <iframe>
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult PdfPage(int id, string format = "PDF")
        {
            ViewBag.Id = id; ViewBag.Format = format;
            return View("PdfViewer");
        }

        // server-side render and stream inline
        [HttpGet]
        public async Task<IActionResult> Inline(int id, string format = "PDF")
        {
            var rpt = await _db.SwReports.Include(r => r.Params).FirstOrDefaultAsync(r => r.Id == id);
            if (rpt == null) return NotFound();

            var parms = rpt.ParamCheck
                ? rpt.Params.Select(p => new KeyValuePair<string, string>(p.ParamKey.Trim(), p.ParamValue.Trim()))
                : Enumerable.Empty<KeyValuePair<string, string>>();

            var baseUrl = _url.BuildUrl(rpt.Name, parms, rpt.PathOverride);
            var exportUrl = _url.BuildExportUrl(baseUrl, format);

            var sa = _opt.Value.ServiceAccount;
            var handler = new HttpClientHandler
            {
                Credentials = (!string.IsNullOrWhiteSpace(sa?.Username))
                    ? new NetworkCredential(sa!.Username, sa.Password, sa.Domain)
                    : CredentialCache.DefaultCredentials,
                PreAuthenticate = true,
                UseDefaultCredentials = string.IsNullOrWhiteSpace(sa?.Username)
            };

            using var http = new HttpClient(handler);
            using var resp = await http.GetAsync(exportUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, "Export failed.");

            var bytes = await resp.Content.ReadAsByteArrayAsync();
            var contentType = (format ?? "PDF").ToUpperInvariant() switch
            {
                "PDF" => "application/pdf",
                "EXCEL" or "XLS" or "XLSX" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "WORDOPENXML" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            // Inline (no filename) so the <iframe> can display it
            return File(bytes, contentType);
        }
    }
}
