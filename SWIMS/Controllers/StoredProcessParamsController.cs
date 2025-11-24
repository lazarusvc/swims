using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SWIMS.Data;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;


namespace SWIMS.Controllers
{
    [Route("[controller]")] // base = /StoredProcessParams
    public class StoredProcessParamsController : Controller
    {
        private readonly SwimsStoredProcsDbContext _db;
        private readonly IElsaWorkflowClient _elsa;

        public StoredProcessParamsController(
            SwimsStoredProcsDbContext db,
            IElsaWorkflowClient elsa)
        {
            _db = db;
            _elsa = elsa;
        }

        // GET: /StoredProcessParams or /StoredProcessParams/{processId}
        [HttpGet("")]
        [HttpGet("{processId:int}")]
        public async Task<IActionResult> Index(int? processId)
        {
            // Build the dropdown of processes for the page header
            var processes = await _db.StoredProcesses
                                     .AsNoTracking()
                                     .OrderBy(x => x.Name)
                                     .Select(x => new { x.Id, x.Name })
                                     .ToListAsync();
            ViewBag.Processes = new SelectList(processes, "Id", "Name", processId);

            if (processId is null || processId <= 0)
            {
                ViewBag.Process = null;
                // No selection: show empty list + chooser
                return View(Enumerable.Empty<StoredProcessParam>());
            }

            var proc = await _db.StoredProcesses.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == processId.Value);
            if (proc == null)
            {
                // Unknown id -> still render chooser; empty list
                ViewBag.Process = null;
                return View(Enumerable.Empty<StoredProcessParam>());
            }

            ViewBag.Process = proc;

            var list = await _db.StoredProcessParams
                                .Where(p => p.StoredProcessId == processId.Value)
                                .OrderBy(p => p.Key)
                                .ToListAsync();

            return View(list);
        }

        // POST: /StoredProcessParams/Select (form posts processId from dropdown)
        [HttpPost("Select"), ValidateAntiForgeryToken]
        public IActionResult Select(int? processId)
        {
            return processId is > 0
                ? RedirectToAction(nameof(Index), new { processId })
                : RedirectToAction(nameof(Index));
        }

        // GET: /StoredProcessParams/Create?processId=5
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? processId)
        {
            if (processId is null || processId <= 0)
                return RedirectToAction(nameof(Index));

            var proc = await _db.StoredProcesses.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == processId.Value);
            if (proc == null) return RedirectToAction(nameof(Index));

            ViewBag.Process = proc;
            return View(new StoredProcessParamEditViewModel { StoredProcessId = processId.Value });
        }

        // POST: /StoredProcessParams/Create
        [HttpPost("Create"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoredProcessParamEditViewModel vm)
        {
            if (!ModelState.IsValid || !StoredProcDataTypes.Allowed.Contains(vm.DataType))
            {
                ModelState.AddModelError("DataType", "Invalid data type.");
                ViewBag.Process = await _db.StoredProcesses.AsNoTracking().FirstAsync(x => x.Id == vm.StoredProcessId);
                return View(vm);
            }

            _db.StoredProcessParams.Add(new StoredProcessParam
            {
                StoredProcessId = vm.StoredProcessId,
                Key = vm.Key.Trim(),
                DataType = vm.DataType,
                Value = vm.Value
            });
            await _db.SaveChangesAsync();


            // 🔔 Notify: Stored procedure parameter created
            await NotifyParamAsync(
                subject: "Stored procedure parameter created",
                body: $"Parameter '{vm.Key}' was added to stored process ID {vm.StoredProcessId}.",
                metadata: new
                {
                    action = "StoredProcessParamCreated",
                    processId = vm.StoredProcessId,
                    key = vm.Key,
                    dataType = vm.DataType
                },
                ct: HttpContext.RequestAborted);

            return RedirectToAction(nameof(Index), new { processId = vm.StoredProcessId });
        }

        // GET: /StoredProcessParams/Edit/10
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var row = await _db.StoredProcessParams.Include(p => p.StoredProcess)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (row == null) return RedirectToAction(nameof(Index));

            ViewBag.Process = row.StoredProcess;
            return View(new StoredProcessParamEditViewModel
            {
                Id = row.Id,
                StoredProcessId = row.StoredProcessId,
                Key = row.Key,
                DataType = row.DataType,
                Value = row.Value
            });
        }

        // POST: /StoredProcessParams/Edit/10
        [HttpPost("Edit/{id:int}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StoredProcessParamEditViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid || !StoredProcDataTypes.Allowed.Contains(vm.DataType))
            {
                ModelState.AddModelError("DataType", "Invalid data type.");
                ViewBag.Process = await _db.StoredProcesses.AsNoTracking().FirstAsync(x => x.Id == vm.StoredProcessId);
                return View(vm);
            }

            var row = await _db.StoredProcessParams.FirstOrDefaultAsync(x => x.Id == id);
            if (row == null) return RedirectToAction(nameof(Index));

            row.Key = vm.Key.Trim();
            row.DataType = vm.DataType;
            row.Value = vm.Value;
            await _db.SaveChangesAsync();

            // 🔔 Notify: Stored procedure parameter updated
            await NotifyParamAsync(
                subject: "Stored procedure parameter updated",
                body: $"Parameter '{vm.Key}' on stored process ID {vm.StoredProcessId} was updated.",
                metadata: new
                {
                    action = "StoredProcessParamUpdated",
                    processId = vm.StoredProcessId,
                    paramId = row.Id,
                    key = vm.Key,
                    dataType = vm.DataType
                },
                ct: HttpContext.RequestAborted);

            return RedirectToAction(nameof(Index), new { processId = vm.StoredProcessId });
        }

        // GET: /StoredProcessParams/Delete/10
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.StoredProcessParams.Include(p => p.StoredProcess)
                                .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (row == null) return RedirectToAction(nameof(Index));
            return View(row);
        }

        // POST: /StoredProcessParams/Delete/10
        [HttpPost("Delete/{id:int}"), ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var row = await _db.StoredProcessParams.FirstOrDefaultAsync(x => x.Id == id);
            if (row != null)
            {
                var pid = row.StoredProcessId;
                var key = row.Key;

                _db.StoredProcessParams.Remove(row);
                await _db.SaveChangesAsync();

                // 🔔 Notify: Stored procedure parameter deleted
                await NotifyParamAsync(
                    subject: "Stored procedure parameter deleted",
                    body: $"Parameter '{key}' was removed from stored process ID {pid}.",
                    metadata: new
                    {
                        action = "StoredProcessParamDeleted",
                        processId = pid,
                        paramId = id,
                        key
                    },
                    ct: HttpContext.RequestAborted);

                return RedirectToAction(nameof(Index), new { processId = pid });
            }
            return RedirectToAction(nameof(Index));

        }

        // --------------------------------------------------------------------
        // Generic admin notification helper for stored procedure parameter actions.
        // --------------------------------------------------------------------
        private async Task NotifyParamAsync(
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
                // 🔔 Notify: Stored procedure parameter event
                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
            }
        }


    }
}
