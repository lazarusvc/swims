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
using SWIMS.Models.Notifications;
using SWIMS.Services.Notifications;



namespace SWIMS.Controllers
{
   [Authorize(Policy = "ReportsView")]
    public class ReportsController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly ISsrsUrlBuilder _url;
        private readonly IOptions<ReportingOptions> _opt;
        private readonly IElsaWorkflowQueue _elsaQueue;

        public ReportsController(
            SwimsReportsDbContext db,
            ISsrsUrlBuilder url,
            IOptions<ReportingOptions> opt,
            IElsaWorkflowQueue elsaQueue)
        {
            _db = db;
            _url = url;
            _opt = opt;
            _elsaQueue = elsaQueue;
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
                    eventKey: SwimsEventKeys.Reports.ExportFailed,
                    subject: "Report export failed",
                    body: $"Report '{title}' failed to export ({(int)resp.StatusCode} {resp.StatusCode})"
                          + (errorSummary != null ? $": {errorSummary}" : "."),
                    reportId: rpt.Id,
                    reportName: rpt.Name,
                    reportDesc: rpt.Desc,
                    format: (format ?? "PDF"),
                    extraMeta_: new
                    {
                        statusCode = (int)resp.StatusCode,
                        status = resp.StatusCode.ToString(),
                        errorSummary
                    },
                    texts_: new
                    {
                        actor = new
                        {
                            subject = "Report export failed",
                            body = $"Your export of '{title}' failed ({(int)resp.StatusCode} {resp.StatusCode})."
                        },
                        routed = new
                        {
                            subject = "Report export failed",
                            body = $"{User?.Identity?.Name ?? "A user"} failed to export '{title}' ({(int)resp.StatusCode} {resp.StatusCode})."
                        },
                        superadmin = new
                        {
                            subject = "Report export failed",
                            body = $"{User?.Identity?.Name ?? "A user"} failed to export '{title}' ({(int)resp.StatusCode} {resp.StatusCode})."
                        }
                    },
                    ct: HttpContext.RequestAborted);
                // 🔔 Notify: END


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
                eventKey: SwimsEventKeys.Reports.ExportSucceeded,
                subject: "Report exported",
                body: $"Report '{okTitle}' was exported as {effectiveFormat}.",
                reportId: rpt.Id,
                reportName: rpt.Name,
                reportDesc: rpt.Desc,
                format: effectiveFormat,
                extraMeta_: new { },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Report exported",
                        body = $"Your report '{okTitle}' was exported as {effectiveFormat}."
                    },
                    routed = new
                    {
                        subject = "Report exported",
                        body = $"{User?.Identity?.Name ?? "A user"} exported '{okTitle}' as {effectiveFormat}."
                    },
                    superadmin = new
                    {
                        subject = "Report exported",
                        body = $"{User?.Identity?.Name ?? "A user"} exported '{okTitle}' as {effectiveFormat}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            // Inline (no filename) so the <iframe> can display it
            return File(bytes, contentType);
        }


        private async Task NotifyReportAsync(
    string eventKey,
    string subject,
    string body,
    CancellationToken ct = default,
    int? reportId = null,
    string? reportName = null,
    string? reportDesc = null,
    string? format = null,
    string? url = null,
    object? extraMeta_ = null,
    object? texts_ = null)
        {
            var recipient = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(recipient))
                recipient = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(recipient))
                return;

            var payload = new
            {
                Recipient = recipient,
                Channel = "InApp",
                Subject = subject,
                Body = body,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    type = NotificationTypes.System,
                    eventKey,
                    url,
                    metadata = new
                    {
                        reportId,
                        reportName,
                        reportDesc,
                        format,

                        actorUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                        actorUserName = User?.Identity?.Name,

                        texts = texts_,
                        extra = extraMeta_
                    }
                })
            };

            try
            {
                // 🔔 Notify: Report run event
                await _elsaQueue.EnqueueByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
                // 🔔 Notify: END
            }
            catch
            {
                // Don't break report viewing if Elsa is down.
            }
        }

    }
}
