using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using SWIMS.Services.Elsa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using SWIMS.Services.Notifications;



namespace SWIMS.Controllers
{
    [Authorize]
    public class formReportController : Controller
    {
        private readonly SwimsDb_moreContext _context;
        private readonly IElsaWorkflowQueue _elsaQueue;

        public formReportController(SwimsDb_moreContext context, IElsaWorkflowQueue elsaQueue)
        {
            _context = context;
            _elsaQueue = elsaQueue;
        }

        // GET: formReport
        public async Task<IActionResult> Index()
        {
            var swimsDb_moreContext = _context.SW_formReports.Include(s => s.SW_forms);
            return View(await swimsDb_moreContext.ToListAsync());
        }

        // GET: formReport/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formReport = await _context.SW_formReports
                .Include(s => s.SW_forms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formReport == null)
            {
                return NotFound();
            }

            return View(sW_formReport);
        }

        // GET: formReport/Create
        public IActionResult Create()
        {
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name");
            return View();
        }

        // POST: formReport/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,url,name,SW_formsId")] SW_formReport sW_formReport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_formReport);
                await _context.SaveChangesAsync();

                // 🔔 Notify: Form report created
                var actorName = User?.Identity?.Name ?? "Someone";

                await NotifyFormReportAsync(
                    eventKey: SwimsEventKeys.FormReports.Created,
                    subject: "Form report created",
                    body: $"Form report '{sW_formReport.name}' was created.",
                    reportId: sW_formReport.Id,
                    reportName: sW_formReport.name,
                    formId: sW_formReport.SW_formsId,
                    reportUrl: sW_formReport.url,
                    url: Url.Action(nameof(Details), new { id = sW_formReport.Id }),
                    texts: new
                    {
                        actor = new { subject = "Form report created", body = $"You created form report '{sW_formReport.name}'." },
                        routed = new { subject = "Form report created", body = $"{actorName} created form report '{sW_formReport.name}'." },
                        superadmin = new { subject = "Form report created", body = $"{actorName} created form report '{sW_formReport.name}'." }
                    },
                    ct: HttpContext.RequestAborted);

                // 🔔 Notify: END


                return RedirectToAction(nameof(Index));
            }

            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formReport.SW_formsId);
            return View(sW_formReport);
        }

        // GET: formReport/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formReport = await _context.SW_formReports.FindAsync(id);
            if (sW_formReport == null)
            {
                return NotFound();
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formReport.SW_formsId);
            return View(sW_formReport);
        }

        // POST: formReport/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,url,name,SW_formsId")] SW_formReport sW_formReport)
        {
            if (id != sW_formReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_formReport);
                    await _context.SaveChangesAsync();

                    // 🔔 Notify: Form report updated
                    var actorName = User?.Identity?.Name ?? "Someone";

                    await NotifyFormReportAsync(
                        eventKey: SwimsEventKeys.FormReports.Updated,
                        subject: "Form report updated",
                        body: $"Form report '{sW_formReport.name}' was updated.",
                        reportId: sW_formReport.Id,
                        reportName: sW_formReport.name,
                        formId: sW_formReport.SW_formsId,
                        reportUrl: sW_formReport.url,
                        url: Url.Action(nameof(Details), new { id = sW_formReport.Id }),
                        texts: new
                        {
                            actor = new { subject = "Form report updated", body = $"You updated form report '{sW_formReport.name}'." },
                            routed = new { subject = "Form report updated", body = $"{actorName} updated form report '{sW_formReport.name}'." },
                            superadmin = new { subject = "Form report updated", body = $"{actorName} updated form report '{sW_formReport.name}'." }
                        },
                        ct: HttpContext.RequestAborted);

                    // 🔔 Notify: END

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_formReportExists(sW_formReport.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));

            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formReport.SW_formsId);
            return View(sW_formReport);
        }

        // GET: formReport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formReport = await _context.SW_formReports
                .Include(s => s.SW_forms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formReport == null)
            {
                return NotFound();
            }

            return View(sW_formReport);
        }

        // POST: formReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_formReport = await _context.SW_formReports.FindAsync(id);
            if (sW_formReport != null)
            {
                var formId = sW_formReport.SW_formsId;
                var name = sW_formReport.name;

                _context.SW_formReports.Remove(sW_formReport);
                await _context.SaveChangesAsync();

                // 🔔 Notify: Form report deleted
                var actorName = User?.Identity?.Name ?? "Someone";

                await NotifyFormReportAsync(
                    eventKey: SwimsEventKeys.FormReports.Deleted,
                    subject: "Form report deleted",
                    body: $"Form report '{name}' was deleted.",
                    reportId: id,
                    reportName: name,
                    formId: formId,
                    reportUrl: null,
                    url: Url.Action(nameof(Index)),
                    texts: new
                    {
                        actor = new { subject = "Form report deleted", body = $"You deleted form report '{name}'." },
                        routed = new { subject = "Form report deleted", body = $"{actorName} deleted form report '{name}'." },
                        superadmin = new { subject = "Form report deleted", body = $"{actorName} deleted form report '{name}'." }
                    },
                    ct: HttpContext.RequestAborted);

                // 🔔 Notify: END

            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool SW_formReportExists(int id)
        {
            return _context.SW_formReports.Any(e => e.Id == id);
        }


        private async Task NotifyFormReportAsync(
    string eventKey,
    string subject,
    string body,
    int? reportId = null,
    string? reportName = null,
    int? formId = null,
    string? reportUrl = null,
    string? url = null,
    object? texts = null,
    object? extraMeta = null,
    CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipient = !string.IsNullOrWhiteSpace(userIdClaim)
                ? userIdClaim
                : User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(recipient))
                return;

            int? actorUserId = null;
            if (int.TryParse(userIdClaim, out var parsedActorId))
                actorUserId = parsedActorId;

            var actorUserName = User?.Identity?.Name ?? "system";

            var payload = new
            {
                Recipient = recipient,
                Channel = "InApp",
                Subject = subject,
                Body = body,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    type = "Forms",
                    eventKey,
                    url,
                    metadata = new
                    {
                        actorUserId,
                        actorUserName,
                        reportId,
                        reportName,
                        formId,
                        reportUrl,
                        texts,
                        extra = extraMeta
                    }
                })
            };

            try
            {
                // 🔔 Notify: Form report admin event
                await _elsaQueue.EnqueueByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
                // keep swallowing for now so admin UX isn't blocked
            }
        }




    }
}
