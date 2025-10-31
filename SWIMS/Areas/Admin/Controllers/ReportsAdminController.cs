using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data.Reports;
using SWIMS.Models;
using SWIMS.Models.Reports;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "ReportsAdmin")]
    public class ReportsAdminController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly RoleManager<SwRole> _roles;
        public ReportsAdminController(SwimsReportsDbContext db, RoleManager<SwRole> roles) { _db = db; _roles = roles; }

        public async Task<IActionResult> Index() =>
            View(await _db.SwReports.OrderBy(r => r.Desc ?? r.Name).ToListAsync());

        public async Task<IActionResult> Create()
        {
            var roles = await _roles.Roles.OrderBy(r => r.Name).ToListAsync();

            // Value = Name, Text = Name  (we store role *name* in SwReport.RoleId)
            ViewBag.RoleId = new SelectList(roles, "Name", "Name");

            var adminRoleName = roles.FirstOrDefault(r => r.Name == "Admin")?.Name;
            return View(new SwReport { RoleId = adminRoleName ?? string.Empty });
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SwReport m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoleId = new SelectList(await _roles.Roles.ToListAsync(), "Name", "Name", m.RoleId);
                return View(m);
            }

            try
            {
                _db.Add(m);
                await _db.SaveChangesAsync();
                TempData["Ok"] = "Report created.";
                // Optional: jump to Params if they checked the box
                if (m.ParamCheck)
                    return RedirectToAction("Index", "ReportParams", new { area = "Admin", reportId = m.Id });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Create failed: " + ex.Message);
                ViewBag.RoleId = new SelectList(await _roles.Roles.ToListAsync(), "Name", "Name", m.RoleId);
                return View(m);
            }
        }


        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.SwReports.FindAsync(id); if (m == null) return NotFound();
            ViewBag.RoleId = new SelectList(await _roles.Roles.ToListAsync(), "Name", "Name", m.RoleId);
            return View(m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SwReport m)
        {
            if (id != m.Id) return BadRequest();
            if (!ModelState.IsValid) {
                ViewBag.RoleId = new SelectList(_roles.Roles, "Name", "Name", m.RoleId); return View(m); }
            _db.Update(m); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _db.SwReports.FindAsync(id); if (m != null) { _db.Remove(m); await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
