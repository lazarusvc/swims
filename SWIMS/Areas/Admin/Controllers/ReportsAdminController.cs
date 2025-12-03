using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data.Reports;
using SWIMS.Models;
using SWIMS.Models.Reports;
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;


namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsAdminController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly RoleManager<SwRole> _roles;
        private readonly IElsaWorkflowClient _elsa;

        public ReportsAdminController(
            SwimsReportsDbContext db,
            RoleManager<SwRole> roles,
            IElsaWorkflowClient elsa)
        {
            _db = db;
            _roles = roles;
            _elsa = elsa;
        }

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

                // 🔔 Notify: Report definition created
                await NotifyReportAdminAsync(
                    subject: "Report definition created",
                    body: $"Report '{m.Desc ?? m.Name}' was created.",
                    metadata: new
                    {
                        action = "ReportDefinitionCreated",
                        reportId = m.Id,
                        reportName = m.Name,
                        roleId = m.RoleId
                    },
                    ct: HttpContext.RequestAborted);


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

            _db.Update(m);
            await _db.SaveChangesAsync();

            // 🔔 Notify: Report definition updated
            await NotifyReportAdminAsync(
                subject: "Report definition updated",
                body: $"Report '{m.Desc ?? m.Name}' was updated.",
                metadata: new
                {
                    action = "ReportDefinitionUpdated",
                    reportId = m.Id,
                    reportName = m.Name,
                    roleId = m.RoleId
                },
                ct: HttpContext.RequestAborted);

            return RedirectToAction(nameof(Index));

        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _db.SwReports.FindAsync(id);
            if (m != null)
            {
                var name = m.Name;
                var desc = m.Desc;

                _db.Remove(m);
                await _db.SaveChangesAsync();

                // 🔔 Notify: Report definition deleted
                await NotifyReportAdminAsync(
                    subject: "Report definition deleted",
                    body: $"Report '{desc ?? name ?? $"ID {id}"}' was deleted.",
                    metadata: new
                    {
                        action = "ReportDefinitionDeleted",
                        reportId = id,
                        reportName = name
                    },
                    ct: HttpContext.RequestAborted);
            }
            return RedirectToAction(nameof(Index));
        }


        private async Task NotifyReportAdminAsync(
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
                // 🔔 Notify: Report admin config event
                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
            }
        }


    }
}
