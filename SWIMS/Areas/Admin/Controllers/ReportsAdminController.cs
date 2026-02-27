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
using SWIMS.Models.Notifications;
using SWIMS.Services.Notifications;



namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsAdminController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly RoleManager<SwRole> _roles;
        private readonly IElsaWorkflowQueue _elsaQueue;

        public ReportsAdminController(
            SwimsReportsDbContext db,
            RoleManager<SwRole> roles,
            IElsaWorkflowQueue elsaQueue)
        {
            _db = db;
            _roles = roles;
            _elsaQueue = elsaQueue;
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
                var reportTitle = m.Desc ?? m.Name ?? $"ID {m.Id}";
                await NotifyReportAdminAsync(
                    eventKey: SwimsEventKeys.Admin.Reports.DefinitionCreated,
                    subject: "Report definition created",
                    body: $"Report '{reportTitle}' was created.",
                    url: Url.Action(nameof(Edit), new { id = m.Id }),
                    reportId: m.Id,
                    reportName: m.Name,
                    reportDesc: m.Desc,
                    extraMeta_: new
                    {
                        roleId = m.RoleId
                    },
                    texts_: new
                    {
                        actor = new
                        {
                            subject = "Report definition created",
                            body = $"You created report '{reportTitle}'."
                        },
                        routed = new
                        {
                            subject = "Report definition created",
                            body = $"{User?.Identity?.Name ?? "An admin"} created report '{reportTitle}'."
                        },
                        superadmin = new
                        {
                            subject = "Report definition created",
                            body = $"{User?.Identity?.Name ?? "An admin"} created report '{reportTitle}'."
                        }
                    },
                    ct: HttpContext.RequestAborted);
                // 🔔 Notify: END



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
            var reportTitle = m.Desc ?? m.Name ?? $"ID {m.Id}";
            await NotifyReportAdminAsync(
                eventKey: SwimsEventKeys.Admin.Reports.DefinitionUpdated,
                subject: "Report definition updated",
                body: $"Report '{reportTitle}' was updated.",
                url: Url.Action(nameof(Edit), new { id = m.Id }),
                reportId: m.Id,
                reportName: m.Name,
                reportDesc: m.Desc,
                extraMeta_: new
                {
                    roleId = m.RoleId
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Report definition updated",
                        body = $"You updated report '{reportTitle}'."
                    },
                    routed = new
                    {
                        subject = "Report definition updated",
                        body = $"{User?.Identity?.Name ?? "An admin"} updated report '{reportTitle}'."
                    },
                    superadmin = new
                    {
                        subject = "Report definition updated",
                        body = $"{User?.Identity?.Name ?? "An admin"} updated report '{reportTitle}'."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


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
                var reportTitle = desc ?? name ?? $"ID {id}";
                await NotifyReportAdminAsync(
                    eventKey: SwimsEventKeys.Admin.Reports.DefinitionDeleted,
                    subject: "Report definition deleted",
                    body: $"Report '{reportTitle}' was deleted.",
                    url: Url.Action(nameof(Index)),
                    reportId: id,
                    reportName: name,
                    reportDesc: desc,
                    extraMeta_: new { },
                    texts_: new
                    {
                        actor = new
                        {
                            subject = "Report definition deleted",
                            body = $"You deleted report '{reportTitle}'."
                        },
                        routed = new
                        {
                            subject = "Report definition deleted",
                            body = $"{User?.Identity?.Name ?? "An admin"} deleted report '{reportTitle}'."
                        },
                        superadmin = new
                        {
                            subject = "Report definition deleted",
                            body = $"{User?.Identity?.Name ?? "An admin"} deleted report '{reportTitle}'."
                        }
                    },
                    ct: HttpContext.RequestAborted);
                // 🔔 Notify: END

            }
            return RedirectToAction(nameof(Index));
        }


        private async Task NotifyReportAdminAsync(
    string eventKey,
    string subject,
    string body,
    CancellationToken ct = default,
    string? url = null,
    int? reportId = null,
    string? reportName = null,
    string? reportDesc = null,
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
                            reportName,
                            reportDesc,

                            actorUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                            actorUserName = User?.Identity?.Name,

                            texts = texts_,
                            extra = extraMeta_
                        }
                    })
                };

                // 🔔 Notify: Report admin config event
                await _elsaQueue.EnqueueByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
                // 🔔 Notify: END
            }
            catch
            {
                // Best-effort: never break admin flows if Elsa is down.
            }
        }



    }
}
