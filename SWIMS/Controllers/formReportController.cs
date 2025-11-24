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


namespace SWIMS.Controllers
{
    [Authorize]
    public class formReportController : Controller
    {
        private readonly SwimsDb_moreContext _context;
        private readonly IElsaWorkflowClient _elsa;

        public formReportController(SwimsDb_moreContext context, IElsaWorkflowClient elsa)
        {
            _context = context;
            _elsa = elsa;
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
                await NotifyFormReportAsync(
                    subject: "Form report created",
                    body: $"Form report '{sW_formReport.name}' was created.",
                    metadata: new
                    {
                        action = "FormReportCreated",
                        id = sW_formReport.Id,
                        formId = sW_formReport.SW_formsId,
                        url = sW_formReport.url
                    },
                    ct: HttpContext.RequestAborted);

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
                    await NotifyFormReportAsync(
                        subject: "Form report updated",
                        body: $"Form report '{sW_formReport.name}' was updated.",
                        metadata: new
                        {
                            action = "FormReportUpdated",
                            id = sW_formReport.Id,
                            formId = sW_formReport.SW_formsId,
                            url = sW_formReport.url
                        },
                        ct: HttpContext.RequestAborted);
                }
                catch (DbUpdateConcurrencyException)
                {

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
                await NotifyFormReportAsync(
                    subject: "Form report deleted",
                    body: $"Form report '{name}' was deleted.",
                    metadata: new
                    {
                        action = "FormReportDeleted",
                        id,
                        formId
                    },
                    ct: HttpContext.RequestAborted);
            }

            return RedirectToAction(nameof(Index));
        }


        private bool SW_formReportExists(int id)
        {
            return _context.SW_formReports.Any(e => e.Id == id);
        }


        private async Task NotifyFormReportAsync(
            string subject,
            string body,
            object? metadata = null,
            CancellationToken ct = default)
        {
            // Get the numeric user id from claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
                return; // no valid user, skip

            var payload = new
            {
                Recipient = userId.ToString(), // always send numeric user id
                Channel = "InApp",
                Subject = subject,
                Body = body,
                MetadataJson = metadata == null ? null : JsonSerializer.Serialize(metadata)
            };

            try
            {
                // 🔔 Notify: Form report admin event
                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
                // keep swallowing for now so admin UX isn't blocked
            }
        }



    }
}
