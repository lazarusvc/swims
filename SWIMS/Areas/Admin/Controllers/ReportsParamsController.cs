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


namespace SWIMS.Areas.Admin.Controllers
{
    // Add a route template so POST cannot miss your action because of conventional routing quirks
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class ReportParamsController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly ILogger<ReportParamsController> _logger;
        private readonly IElsaWorkflowClient _elsa;

        public ReportParamsController(
            SwimsReportsDbContext db,
            ILogger<ReportParamsController> logger,
            IElsaWorkflowClient elsa)
        {
            _db = db;
            _logger = logger;
            _elsa = elsa;
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
                subject: "Report parameter created",
                body: $"Parameter '{m.ParamKey}' was added to report ID {m.SwReportId}.",
                metadata: new
                {
                    action = "ReportParamCreated",
                    reportId = m.SwReportId,
                    paramId = m.Id,
                    key = m.ParamKey,
                    dataType = m.ParamDataType
                },
                ct: HttpContext.RequestAborted);

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
                subject: "Report parameter updated",
                body: $"Parameter '{m.ParamKey}' on report ID {m.SwReportId} was updated.",
                metadata: new
                {
                    action = "ReportParamUpdated",
                    reportId = m.SwReportId,
                    paramId = m.Id,
                    key = m.ParamKey,
                    dataType = m.ParamDataType
                },
                ct: HttpContext.RequestAborted);

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
                subject: "Report parameter deleted",
                body: $"Parameter '{m.ParamKey}' was removed from report ID {rid}.",
                metadata: new
                {
                    action = "ReportParamDeleted",
                    reportId = rid,
                    paramId = m.Id,
                    key = m.ParamKey
                },
                ct: HttpContext.RequestAborted);

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
                subject: "Report parameter quick-added",
                body: $"QuickAdd parameter '{key}' was added to report ID {reportId}.",
                metadata: new
                {
                    action = "ReportParamQuickAdd",
                    reportId,
                    key,
                    dataType = dt
                },
                ct: HttpContext.RequestAborted);

            TempData["Ok"] = $"QuickAdd: {key}={value}";
            return RedirectToAction(nameof(Index), new { reportId });

        }
#endif

        private async Task NotifyReportParamAsync(
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
                // 🔔 Notify: Report parameter admin event
                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
            }
        }


    }
}
