using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using SWIMS.Data;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Models.Notifications;
using SWIMS.Services.Elsa;
using SWIMS.Services.Notifications;


namespace SWIMS.Controllers
{
    public class formProcessController : Controller
    {
        private readonly SwimsDb_moreContext _context;
        private readonly SwimsStoredProcsDbContext _context_sp;
        private readonly IElsaWorkflowClient _elsa;


        private static int? TryExtractProcIdFromRunUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var m = Regex.Match(url, @"/StoredProcesses/Run/(\d+)", RegexOptions.IgnoreCase);
            return m.Success ? int.Parse(m.Groups[1].Value) : (int?)null;
        }

        public formProcessController(SwimsDb_moreContext context, SwimsStoredProcsDbContext sp, IElsaWorkflowClient elsa)
        {
            _context = context;
            _context_sp = sp;
            _elsa = elsa;
        }

        // GET: formProcess
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_formProcesses.ToListAsync());
        }

        // GET: formProcess/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }

            return View(sW_formProcess);
        }

        // GET: formProcess/Create
        public IActionResult Create()
        {
            ViewBag.processes = _context_sp.StoredProcesses
                .Select(c => new SelectListItem()
                {
                    Text = c.Name,
                    Value = "../StoredProcesses/Run/" + Convert.ToString(c.Id)
                })
                .ToList();

            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name");
            return View();
        }

        // POST: formProcess/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,url,name,SW_formsId")] SW_formProcess sW_formProcess)
        {
            if (ModelState.IsValid)
            {
                // --- Auto-fill name from Stored Procedure when blank ---
                if (string.IsNullOrWhiteSpace(sW_formProcess.name))
                {
                    var procId = TryExtractProcIdFromRunUrl(sW_formProcess.url);
                    if (procId.HasValue)
                    {
                        var spName = await _context_sp.StoredProcesses
                            .Where(x => x.Id == procId.Value)
                            .Select(x => x.Name)
                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrWhiteSpace(spName))
                            sW_formProcess.name = spName;
                    }
                }
                // -------------------------------------------------------

                _context.Add(sW_formProcess);
                await _context.SaveChangesAsync();

                // 🔔 Notify: Form process created
                var actorName = User?.Identity?.Name ?? "A user";

                var formInfo = await _context.SW_forms
                    .AsNoTracking()
                    .Where(x => x.Id == sW_formProcess.SW_formsId)
                    .Select(x => new { x.uuid, x.name })
                    .FirstOrDefaultAsync();

                var notifProcId = TryExtractProcIdFromRunUrl(sW_formProcess.url);


                var subject = "Form process created";
                var actorBody = $"You created form process '{sW_formProcess.name ?? $"ID {sW_formProcess.Id}"}' for form '{formInfo?.name ?? $"Form {sW_formProcess.SW_formsId}"}'.";
                var routedBody = $"{actorName} created form process '{sW_formProcess.name ?? $"ID {sW_formProcess.Id}"}' for form '{formInfo?.name ?? $"Form {sW_formProcess.SW_formsId}"}'.";

                await NotifyFormProcessEventAsync(
                    eventKey: SwimsEventKeys.FormProcess.ProcessCreated,
                    subject: subject,
                    actorBody: actorBody,
                    routedBody: routedBody,
                    url: Url.Action(nameof(Details), new { id = sW_formProcess.Id }),
                    extraMeta_: new
                    {
                        formProcessId = sW_formProcess.Id,
                        formId = sW_formProcess.SW_formsId,
                        formUuid = formInfo?.uuid,
                        formName = formInfo?.name,

                        processName = sW_formProcess.name,
                        runUrl = sW_formProcess.url,
                        storedProcId = notifProcId
                    },
                    ct: HttpContext.RequestAborted
                );
                // 🔔 Notify: END


                return RedirectToAction(nameof(Index));
            }
            return View(sW_formProcess);
        }

        // GET: formProcess/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses.FindAsync(id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }
            return View(sW_formProcess);
        }

        // POST: formProcess/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,url,name,SW_formsId")] SW_formProcess sW_formProcess)
        {
            if (id != sW_formProcess.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // --- Auto-fill name from Stored Procedure when blank (on edit too) ---
                    if (string.IsNullOrWhiteSpace(sW_formProcess.name))
                    {
                        var procId = TryExtractProcIdFromRunUrl(sW_formProcess.url);
                        if (procId.HasValue)
                        {
                            var spName = await _context_sp.StoredProcesses
                                .Where(x => x.Id == procId.Value)
                                .Select(x => x.Name)
                                .FirstOrDefaultAsync();

                            if (!string.IsNullOrWhiteSpace(spName))
                                sW_formProcess.name = spName;
                        }
                    }
                    // ---------------------------------------------------------------------

                    _context.Update(sW_formProcess);
                    await _context.SaveChangesAsync();

                    // 🔔 Notify: Form process updated
                    var actorName = User?.Identity?.Name ?? "A user";

                    var formInfo = await _context.SW_forms
                        .AsNoTracking()
                        .Where(x => x.Id == sW_formProcess.SW_formsId)
                        .Select(x => new { x.uuid, x.name })
                        .FirstOrDefaultAsync();

                    var notifProcId = TryExtractProcIdFromRunUrl(sW_formProcess.url);


                    var subject = "Form process updated";
                    var actorBody = $"You updated form process '{sW_formProcess.name ?? $"ID {sW_formProcess.Id}"}' for form '{formInfo?.name ?? $"Form {sW_formProcess.SW_formsId}"}'.";
                    var routedBody = $"{actorName} updated form process '{sW_formProcess.name ?? $"ID {sW_formProcess.Id}"}' for form '{formInfo?.name ?? $"Form {sW_formProcess.SW_formsId}"}'.";

                    await NotifyFormProcessEventAsync(
                        eventKey: SwimsEventKeys.FormProcess.ProcessUpdated,
                        subject: subject,
                        actorBody: actorBody,
                        routedBody: routedBody,
                        url: Url.Action(nameof(Details), new { id = sW_formProcess.Id }),
                        extraMeta_: new
                        {
                            formProcessId = sW_formProcess.Id,
                            formId = sW_formProcess.SW_formsId,
                            formUuid = formInfo?.uuid,
                            formName = formInfo?.name,

                            processName = sW_formProcess.name,
                            runUrl = sW_formProcess.url,
                            storedProcId = notifProcId
                        },
                        ct: HttpContext.RequestAborted
                    );
                    // 🔔 Notify: END


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_formProcessExists(sW_formProcess.Id))
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
            return View(sW_formProcess);
        }

        // GET: formProcess/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }

            return View(sW_formProcess);
        }

        // POST: formProcess/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_formProcess = await _context.SW_formProcesses.FindAsync(id);

            if (sW_formProcess != null)
            {
                var processId = sW_formProcess.Id;
                var processName = sW_formProcess.name;
                var runUrl = sW_formProcess.url;
                var formId = sW_formProcess.SW_formsId;

                var formInfo = await _context.SW_forms
                    .AsNoTracking()
                    .Where(x => x.Id == formId)
                    .Select(x => new { x.uuid, x.name })
                    .FirstOrDefaultAsync();

                var procId = TryExtractProcIdFromRunUrl(runUrl);

                _context.SW_formProcesses.Remove(sW_formProcess);
                await _context.SaveChangesAsync();

                // 🔔 Notify: Form process deleted
                var actorName = User?.Identity?.Name ?? "A user";

                var subject = "Form process deleted";
                var actorBody = $"You deleted form process '{processName ?? $"ID {processId}"}' for form '{formInfo?.name ?? $"Form {formId}"}'.";
                var routedBody = $"{actorName} deleted form process '{processName ?? $"ID {processId}"}' for form '{formInfo?.name ?? $"Form {formId}"}'.";

                await NotifyFormProcessEventAsync(
                    eventKey: SwimsEventKeys.FormProcess.ProcessDeleted,
                    subject: subject,
                    actorBody: actorBody,
                    routedBody: routedBody,
                    url: Url.Action(nameof(Index)),
                    extraMeta_: new
                    {
                        formProcessId = processId,
                        formId = formId,
                        formUuid = formInfo?.uuid,
                        formName = formInfo?.name,

                        processName = processName,
                        runUrl = runUrl,
                        storedProcId = procId
                    },
                    ct: HttpContext.RequestAborted
                );
                // 🔔 Notify: END

                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        private bool SW_formProcessExists(int id)
        {
            return _context.SW_formProcesses.Any(e => e.Id == id);
        }

        private async Task NotifyFormProcessEventAsync(
    string eventKey,
    string subject,
    string actorBody,
    string routedBody,
    string? url = null,
    object? extraMeta_ = null,
    CancellationToken ct = default)
        {
            try
            {
                var userIdClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var recipient = !string.IsNullOrWhiteSpace(userIdClaim)
                    ? userIdClaim
                    : User?.Identity?.Name;

                if (string.IsNullOrWhiteSpace(recipient))
                    return;

                int? actorUserId = null;
                if (int.TryParse(userIdClaim, out var parsedActorId))
                    actorUserId = parsedActorId;

                var actorUserName = User?.Identity?.Name ?? "";

                var payload = new
                {
                    Recipient = recipient,
                    Channel = "InApp",
                    Subject = subject,
                    Body = actorBody,
                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        type = NotificationTypes.Forms,
                        eventKey,
                        url,
                        metadata = new
                        {
                            actorUserId,
                            actorUserName,

                            texts = new
                            {
                                actor = new { subject, body = actorBody },
                                routed = new { subject, body = routedBody },
                                superadmin = new { subject, body = routedBody }
                            },

                            extra = extraMeta_
                        }
                    })
                };

                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
                // Best-effort: never block config UX if Elsa is down.
            }
        }

    }
}
