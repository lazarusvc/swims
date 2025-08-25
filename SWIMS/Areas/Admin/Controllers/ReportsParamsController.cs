using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data.Reports;
using SWIMS.Models.Reports;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "ReportsAdmin")]
    public class ReportParamsController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        public ReportParamsController(SwimsReportsDbContext db) { _db = db; }

        public async Task<IActionResult> Index(int reportId)
        {
            var report = await _db.SwReports.FindAsync(reportId); if (report == null) return NotFound();
            ViewBag.Report = report;
            var items = await _db.SwReportParams.Where(p => p.SwReportId == reportId).OrderBy(p => p.ParamKey).ToListAsync();
            return View(items);
        }

        public IActionResult Create(int reportId) => View(new SwReportParam { SwReportId = reportId });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SwReportParam m)
        {
            if (!ModelState.IsValid) return View(m);
            _db.Add(m); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { reportId = m.SwReportId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.SwReportParams.FindAsync(id); if (m == null) return NotFound();
            return View(m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SwReportParam m)
        {
            if (id != m.Id) return BadRequest();
            if (!ModelState.IsValid) return View(m);
            _db.Update(m); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { reportId = m.SwReportId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _db.SwReportParams.FindAsync(id); if (m == null) return NotFound();
            var rid = m.SwReportId; _db.Remove(m); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { reportId = rid });
        }
    }
}
