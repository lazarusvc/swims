using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SWIMS.Data.Reports;
using SWIMS.Models.ViewModels;
using SWIMS.Services.Reporting;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using SWIMS.Services.Elsa;


namespace SWIMS.Controllers
{
    public class ReportsController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly ISsrsUrlBuilder _url;
        private readonly IOptions<ReportingOptions> _opt;
        private readonly IElsaWorkflowClient _elsa;

        public ReportsController(
            SwimsReportsDbContext db,
            ISsrsUrlBuilder url,
            IOptions<ReportingOptions> opt,
            IElsaWorkflowClient elsa)
        {
            _db = db;
            _url = url;
            _opt = opt;
            _elsa = elsa;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roleIds = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var reports = await _db.SwReports.Where(r => roleIds.Contains(r.RoleId))
                                             .OrderBy(r => r.Desc ?? r.Name)
                                             .ToListAsync();
            return View(new ReportsIndexViewModel { Reports = reports });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(int id)
        {
            var roleIds = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var reports = await _db.SwReports.Where(r => roleIds.Contains(r.RoleId))
                                             .OrderBy(r => r.Desc ?? r.Name)
                                             .ToListAsync();

            var rpt = await _db.SwReports.Include(r => r.Params).FirstOrDefaultAsync(r => r.Id == id);
            if (rpt == null) return NotFound();

            var parms = rpt.ParamCheck
                ? rpt.Params.Select(p => new KeyValuePair<string, string>(p.ParamKey.Trim(), p.ParamValue.Trim()))
                : System.Linq.Enumerable.Empty<KeyValuePair<string, string>>();

            var url = _url.BuildUrl(rpt.Name, parms, rpt.PathOverride);

            var vm = new ReportsIndexViewModel
            {
                Reports = reports,
                SelectedId = id,
                ViewerUrl = url,
                ViewerMode = "Ssrs"
            };
            return View("Index", vm); // <— RETURN INDEX WITH IFRAME BELOW
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PdfPage(int id, string format = "PDF")
        {
            var roleIds = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var reports = await _db.SwReports.Where(r => roleIds.Contains(r.RoleId))
                                             .OrderBy(r => r.Desc ?? r.Name)
                                             .ToListAsync();

            var exists = await _db.SwReports.AnyAsync(r => r.Id == id);
            if (!exists) return NotFound();

            var vm = new ReportsIndexViewModel
            {
                Reports = reports,
                SelectedId = id,
                ViewerUrl = Url.Action("Inline", "Reports", new { id, format }),
                ViewerMode = "Inline",
                Format = format
            };
            return View("Index", vm); // <— SAME PAGE, embeds /Reports/Inline
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
            {
                string? errorSummary = null;
                try
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        errorSummary = text.Length > 160 ? text[..160] + "…" : text;
                }
                catch { }

                var title = rpt.Desc ?? rpt.Name ?? $"Report {rpt.Id}";

                // 🔔 Notify: Report export failed
                await NotifyReportAsync(
                    subject: "Report export failed",
                    body: $"Report '{title}' failed to export ({(int)resp.StatusCode} {resp.StatusCode})"
                          + (errorSummary != null ? $": {errorSummary}" : "."),
                    metadata: new
                    {
                        action = "ReportExportFailed",
                        reportId = rpt.Id,
                        reportName = rpt.Name,
                        reportDesc = rpt.Desc,
                        format = format ?? "PDF",
                        statusCode = (int)resp.StatusCode,
                        status = resp.StatusCode.ToString(),
                        errorSummary
                    },
                    ct: HttpContext.RequestAborted);

                return StatusCode((int)resp.StatusCode, "Export failed.");
            }

            var bytes = await resp.Content.ReadAsByteArrayAsync();
            var effectiveFormat = (format ?? "PDF").ToUpperInvariant();
            var contentType = effectiveFormat switch
            {
                "PDF" => "application/pdf",
                "EXCEL" or "XLS" or "XLSX" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "WORDOPENXML" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            var okTitle = rpt.Desc ?? rpt.Name ?? $"Report {rpt.Id}";

            // 🔔 Notify: Report export succeeded
            await NotifyReportAsync(
                subject: "Report exported",
                body: $"Report '{okTitle}' was exported as {effectiveFormat}.",
                metadata: new
                {
                    action = "ReportExportSucceeded",
                    reportId = rpt.Id,
                    reportName = rpt.Name,
                    reportDesc = rpt.Desc,
                    format = effectiveFormat
                },
                ct: HttpContext.RequestAborted);

            // Inline (no filename) so the <iframe> can display it
            return File(bytes, contentType);
        }


        private async Task NotifyReportAsync(
            string subject,
            string body,
            object? metadata = null,
            CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipient = !string.IsNullOrWhiteSpace(userIdClaim)
                ? userIdClaim
                : User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(recipient))
                return;

            var payload = new
            {
                Recipient = recipient,
                Channel = "InApp",
                Subject = subject,
                Body = body,
                MetadataJson = metadata == null ? null : JsonSerializer.Serialize(metadata)
            };

            try
            {
                // 🔔 Notify: Report run event
                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
                // Don't break report viewing if Elsa is down.
            }
        }
    }
}
