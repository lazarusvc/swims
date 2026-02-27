using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWIMS.Data.Reports;
using SWIMS.Models.Reports;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;
using SWIMS.Models.Notifications;
using SWIMS.Services.Notifications;



namespace SWIMS.Areas.Admin.Controllers
{
    // Add a route template so POST cannot miss your action because of conventional routing quirks
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class ReportParamsController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly ILogger<ReportParamsController> _logger;
        private readonly IElsaWorkflowQueue _elsaQueue;

        public ReportParamsController(
            SwimsReportsDbContext db,
            ILogger<ReportParamsController> logger,
            IElsaWorkflowQueue elsaQueue)
        {
            _db = db;
            _logger = logger;
            _elsaQueue = elsaQueue;
        }

        // GET: /Admin/ReportParams/Index?reportId=123
        [HttpGet]
        public async Task<IActionResult> Index(int reportId)
        {
            var report = await _db.SwReports.FindAsync(reportId);
            if (report == null) return NotFound();
            ViewBag.Report = report;

            var items = await _db.SwReportParams
                .Where(p => p.SwReportId == reportId)
                .OrderBy(p => p.ParamKey)
                .ToListAsync();

            return View(items);
        }

        // GET: /Admin/ReportParams/Create?reportId=123
        [HttpGet]
        public IActionResult Create(int reportId) =>
            View(new SwReportParam { SwReportId = reportId });

        // POST: /Admin/ReportParams/Create
        // Force binding from form; log and display any model errors
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] SwReportParam m, int? reportId)
        {
            // Safety: if hidden field didn't bind for any reason, fall back to route/query param
            if (m.SwReportId == 0 && reportId.HasValue) m.SwReportId = reportId.Value;

            // ignore the navigation property during validation
            ModelState.Remove(nameof(SwReportParam.SwReport));

            if (!ModelState.IsValid)
            {
                var errs = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                if (!string.IsNullOrWhiteSpace(errs))
                {
                    _logger.LogWarning("Create ReportParam ModelState errors: {Errors}", errs);
                    TempData["Err"] = errs;
                }
                return View(m);
            }

            m.ParamKey = m.ParamKey?.Trim() ?? "";
            m.ParamValue = m.ParamValue?.Trim() ?? "";

            _db.Add(m);
            await _db.SaveChangesAsync();

            // 🔔 Notify: Report parameter created
            await NotifyReportParamAsync(
                eventKey: SwimsEventKeys.Admin.Reports.Parameters.Created,
                subject: "Report parameter created",
                body: $"Parameter '{m.ParamKey}' was added to report ID {m.SwReportId}.",
                url: Url.Action(nameof(Edit), new { id = m.Id }),
                reportId: m.SwReportId,
                paramId: m.Id,
                paramKey: m.ParamKey,
                dataType: m.ParamDataType,
                extraMeta_: new
                {
                    action = "ReportParamCreated"
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Report parameter created",
                        body = $"You added parameter '{m.ParamKey}' to report ID {m.SwReportId}."
                    },
                    routed = new
                    {
                        subject = "Report parameter created",
                        body = $"{User?.Identity?.Name ?? "An admin"} added parameter '{m.ParamKey}' to report ID {m.SwReportId}."
                    },
                    superadmin = new
                    {
                        subject = "Report parameter created",
                        body = $"{User?.Identity?.Name ?? "An admin"} added parameter '{m.ParamKey}' to report ID {m.SwReportId}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = "Parameter added.";
            return RedirectToAction(nameof(Index), new { reportId = m.SwReportId });

        }

        // GET: /Admin/ReportParams/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.SwReportParams.FindAsync(id);
            if (m == null) return NotFound();
            return View(m);
        }

        // POST: /Admin/ReportParams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] SwReportParam m)
        {
            if (id != m.Id) return BadRequest();

                // ignore the navigation property during validation
                ModelState.Remove(nameof(SwReportParam.SwReport));

                if (!ModelState.IsValid)
                {
                    var errs = string.Join(" | ",
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    if (!string.IsNullOrWhiteSpace(errs))
                    {
                        _logger.LogWarning("Edit ReportParam ModelState errors: {Errors}", errs);
                        TempData["Err"] = errs;
                    }
                    return View(m);
                }

                m.ParamKey = m.ParamKey?.Trim() ?? "";
                m.ParamValue = m.ParamValue?.Trim() ?? "";

            _db.Update(m);
            await _db.SaveChangesAsync();

            // 🔔 Notify: Report parameter updated
            await NotifyReportParamAsync(
                eventKey: SwimsEventKeys.Admin.Reports.Parameters.Updated,
                subject: "Report parameter updated",
                body: $"Parameter '{m.ParamKey}' on report ID {m.SwReportId} was updated.",
                url: Url.Action(nameof(Edit), new { id = m.Id }),
                reportId: m.SwReportId,
                paramId: m.Id,
                paramKey: m.ParamKey,
                dataType: m.ParamDataType,
                extraMeta_: new
                {
                    action = "ReportParamUpdated"
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Report parameter updated",
                        body = $"You updated parameter '{m.ParamKey}' on report ID {m.SwReportId}."
                    },
                    routed = new
                    {
                        subject = "Report parameter updated",
                        body = $"{User?.Identity?.Name ?? "An admin"} updated parameter '{m.ParamKey}' on report ID {m.SwReportId}."
                    },
                    superadmin = new
                    {
                        subject = "Report parameter updated",
                        body = $"{User?.Identity?.Name ?? "An admin"} updated parameter '{m.ParamKey}' on report ID {m.SwReportId}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = "Parameter saved.";
            return RedirectToAction(nameof(Index), new { reportId = m.SwReportId });

        }

        // POST: /Admin/ReportParams/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _db.SwReportParams.FindAsync(id);
            if (m == null) return NotFound();
            var rid = m.SwReportId;

            _db.Remove(m);
            await _db.SaveChangesAsync();

            // 🔔 Notify: Report parameter deleted
            await NotifyReportParamAsync(
                eventKey: SwimsEventKeys.Admin.Reports.Parameters.Deleted,
                subject: "Report parameter deleted",
                body: $"Parameter '{m.ParamKey}' was removed from report ID {rid}.",
                url: Url.Action(nameof(Index), new { reportId = rid }),
                reportId: rid,
                paramId: m.Id,
                paramKey: m.ParamKey,
                dataType: m.ParamDataType,
                extraMeta_: new
                {
                    action = "ReportParamDeleted"
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Report parameter deleted",
                        body = $"You removed parameter '{m.ParamKey}' from report ID {rid}."
                    },
                    routed = new
                    {
                        subject = "Report parameter deleted",
                        body = $"{User?.Identity?.Name ?? "An admin"} removed parameter '{m.ParamKey}' from report ID {rid}."
                    },
                    superadmin = new
                    {
                        subject = "Report parameter deleted",
                        body = $"{User?.Identity?.Name ?? "An admin"} removed parameter '{m.ParamKey}' from report ID {rid}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = "Parameter deleted.";
            return RedirectToAction(nameof(Index), new { reportId = rid });

        }

#if DEBUG
        // DEV-ONLY sanity probe (easy local test if POST still feels like a reload):
        // GET /Admin/ReportParams/QuickAdd?reportId=123&key=year&value=2025&dt=String
        [HttpGet]
        public async Task<IActionResult> QuickAdd(int reportId, string key, string value, string? dt)
        {
            _db.Add(new SwReportParam
            {
                SwReportId = reportId,
                ParamKey = key.Trim(),
                ParamValue = value.Trim(),
                ParamDataType = string.IsNullOrWhiteSpace(dt) ? null : dt.Trim()
            });

            await _db.SaveChangesAsync();

            // 🔔 Notify: Report parameter quick-added
            await NotifyReportParamAsync(
                eventKey: SwimsEventKeys.Admin.Reports.Parameters.QuickAdded,
                subject: "Report parameter quick-added",
                body: $"QuickAdd parameter '{key}' was added to report ID {reportId}.",
                url: Url.Action(nameof(Index), new { reportId }),
                reportId: reportId,
                paramId: null,
                paramKey: key,
                dataType: dt,
                extraMeta_: new
                {
                    action = "ReportParamQuickAdd",
                    value
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Report parameter quick-added",
                        body = $"You quick-added parameter '{key}' to report ID {reportId}."
                    },
                    routed = new
                    {
                        subject = "Report parameter quick-added",
                        body = $"{User?.Identity?.Name ?? "An admin"} quick-added parameter '{key}' to report ID {reportId}."
                    },
                    superadmin = new
                    {
                        subject = "Report parameter quick-added",
                        body = $"{User?.Identity?.Name ?? "An admin"} quick-added parameter '{key}' to report ID {reportId}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = $"QuickAdd: {key}={value}";
            return RedirectToAction(nameof(Index), new { reportId });

        }
#endif

        private async Task NotifyReportParamAsync(
    string eventKey,
    string subject,
    string body,
    CancellationToken ct = default,
    string? url = null,
    int? reportId = null,
    int? paramId = null,
    string? paramKey = null,
    string? dataType = null,
    object? extraMeta_ = null,
    object? texts_ = null)
        {
            try
            {
                var recipient = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User?.Identity?.Name;

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
                            paramId,
                            paramKey,
                            dataType,

                            actorUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                            actorUserName = User?.Identity?.Name,

                            texts = texts_,
                            extra = extraMeta_
                        }
                    })
                };

                try
                {
                    // 🔔 Notify: Report parameter admin event
                    await _elsaQueue.EnqueueByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
                    // 🔔 Notify: END
                }
                catch
                {
                }
            }
            catch
            {
                // Best-effort.
            }
        }



    }
}
