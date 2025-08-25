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
            ViewBag.RoleId = new SelectList(await _roles.Roles.ToListAsync(), "Id", "Name");
            return View(new SwReport());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SwReport m)
        {
            if (!ModelState.IsValid) { ViewBag.RoleId = new SelectList(_roles.Roles, "Id", "Name", m.RoleId); return View(m); }
            _db.Add(m); await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.SwReports.FindAsync(id); if (m == null) return NotFound();
            ViewBag.RoleId = new SelectList(await _roles.Roles.ToListAsync(), "Id", "Name", m.RoleId);
            return View(m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SwReport m)
        {
            if (id != m.Id) return BadRequest();
            if (!ModelState.IsValid) { ViewBag.RoleId = new SelectList(_roles.Roles, "Id", "Name", m.RoleId); return View(m); }
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
