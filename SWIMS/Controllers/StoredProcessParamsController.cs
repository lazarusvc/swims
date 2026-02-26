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
using SWIMS.Services.Notifications;
using SWIMS.Services.Diagnostics.Auditing;



namespace SWIMS.Controllers
{
    [Route("[controller]")] // base = /StoredProcessParams
    public class StoredProcessParamsController : Controller
    {
        private readonly SwimsStoredProcsDbContext _db;
        private readonly IElsaWorkflowClient _elsa;
        private readonly IAuditLogger _audit;

        public StoredProcessParamsController(
            SwimsStoredProcsDbContext db,
            IElsaWorkflowClient elsa,
            IAuditLogger audit)
        {
            _db = db;
            _elsa = elsa;
            _audit = audit;
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

            var row = new StoredProcessParam
            {
                StoredProcessId = vm.StoredProcessId,
                Key = vm.Key.Trim(),
                DataType = vm.DataType,
                Value = vm.Value
            };
            _db.StoredProcessParams.Add(row);
            await _db.SaveChangesAsync();

            // 📝 Audit: Stored procedure parameter created
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "StoredProcedureParameterCreated",
                entity: "StoredProcessParam",
                entityId: row.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: null,
                newObj: new
                {
                    paramId = row.Id,
                    processId = row.StoredProcessId,
                    key = row.Key,
                    dataType = row.DataType,
                    hasValue = !string.IsNullOrWhiteSpace(row.Value),
                    valueLength = row.Value?.Length ?? 0
                },
                extra: new
                {
                    processId = row.StoredProcessId,
                    key = row.Key,
                    dataType = row.DataType
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Stored procedure parameter created
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyParamAsync(
                eventKey: SwimsEventKeys.StoredProcedures.Parameters.Created,
                subject: "Stored procedure parameter created",
                body: $"Parameter '{vm.Key}' was added to stored process ID {vm.StoredProcessId}.",
                processId: vm.StoredProcessId,
                paramId: null,
                key: vm.Key,
                dataType: vm.DataType,
                url: Url.Action(nameof(Index), new { processId = vm.StoredProcessId }),
                texts: new
                {
                    actor = new { subject = "Stored procedure parameter created", body = $"You added parameter '{vm.Key}' to process ID {vm.StoredProcessId}." },
                    routed = new { subject = "Stored procedure parameter created", body = $"{actorName} added parameter '{vm.Key}' to process ID {vm.StoredProcessId}." },
                    superadmin = new { subject = "Stored procedure parameter created", body = $"{actorName} added parameter '{vm.Key}' to process ID {vm.StoredProcessId}." }
                },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


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

            var oldObj = new
            {
                paramId = row.Id,
                processId = row.StoredProcessId,
                key = row.Key,
                dataType = row.DataType,
                hasValue = !string.IsNullOrWhiteSpace(row.Value),
                valueLength = row.Value?.Length ?? 0
            };

            row.Key = vm.Key.Trim();
            row.DataType = vm.DataType;
            row.Value = vm.Value;
            await _db.SaveChangesAsync();

            // 📝 Audit: Stored procedure parameter updated
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            var newObj = new
            {
                paramId = row.Id,
                processId = row.StoredProcessId,
                key = row.Key,
                dataType = row.DataType,
                hasValue = !string.IsNullOrWhiteSpace(row.Value),
                valueLength = row.Value?.Length ?? 0
            };

            await _audit.TryLogAsync(
                action: "StoredProcedureParameterUpdated",
                entity: "StoredProcessParam",
                entityId: row.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: oldObj,
                newObj: newObj,
                extra: new
                {
                    processId = row.StoredProcessId,
                    keyChanged = !string.Equals(oldObj.key, newObj.key, StringComparison.Ordinal),
                    dataTypeChanged = !string.Equals(oldObj.dataType, newObj.dataType, StringComparison.Ordinal),
                    valueChanged = (oldObj.valueLength != newObj.valueLength) || (oldObj.hasValue != newObj.hasValue)
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Stored procedure parameter updated
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyParamAsync(
                eventKey: SwimsEventKeys.StoredProcedures.Parameters.Updated,
                subject: "Stored procedure parameter updated",
                body: $"Parameter '{vm.Key}' on stored process ID {vm.StoredProcessId} was updated.",
                processId: vm.StoredProcessId,
                paramId: row.Id,
                key: vm.Key,
                dataType: vm.DataType,
                url: Url.Action(nameof(Index), new { processId = vm.StoredProcessId }),
                texts: new
                {
                    actor = new { subject = "Stored procedure parameter updated", body = $"You updated parameter '{vm.Key}' on process ID {vm.StoredProcessId}." },
                    routed = new { subject = "Stored procedure parameter updated", body = $"{actorName} updated parameter '{vm.Key}' on process ID {vm.StoredProcessId}." },
                    superadmin = new { subject = "Stored procedure parameter updated", body = $"{actorName} updated parameter '{vm.Key}' on process ID {vm.StoredProcessId}." }
                },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


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

                var oldObj = new
                {
                    paramId = row.Id,
                    processId = row.StoredProcessId,
                    key = row.Key,
                    dataType = row.DataType,
                    hasValue = !string.IsNullOrWhiteSpace(row.Value),
                    valueLength = row.Value?.Length ?? 0
                };

                _db.StoredProcessParams.Remove(row);
                await _db.SaveChangesAsync();

                // 📝 Audit: Stored procedure parameter deleted
                AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

                await _audit.TryLogAsync(
                    action: "StoredProcedureParameterDeleted",
                    entity: "StoredProcessParam",
                    entityId: id.ToString(),
                    userId: actorId,
                    username: actorUsername,
                    oldObj: oldObj,
                    newObj: null,
                    extra: new
                    {
                        processId = pid,
                        key = key
                    },
                    ct: HttpContext.RequestAborted);
                // 📝 Audit: END

                // 🔔 Notify: Stored procedure parameter deleted
                var actorName = User?.Identity?.Name ?? "Someone";

                await NotifyParamAsync(
                    eventKey: SwimsEventKeys.StoredProcedures.Parameters.Deleted,
                    subject: "Stored procedure parameter deleted",
                    body: $"Parameter '{key}' was removed from stored process ID {pid}.",
                    processId: pid,
                    paramId: id,
                    key: key,
                    dataType: null,
                    url: Url.Action(nameof(Index), new { processId = pid }),
                    texts: new
                    {
                        actor = new { subject = "Stored procedure parameter deleted", body = $"You removed parameter '{key}' from process ID {pid}." },
                        routed = new { subject = "Stored procedure parameter deleted", body = $"{actorName} removed parameter '{key}' from process ID {pid}." },
                        superadmin = new { subject = "Stored procedure parameter deleted", body = $"{actorName} removed parameter '{key}' from process ID {pid}." }
                    },
                    ct: HttpContext.RequestAborted);

                // 🔔 Notify: END


                return RedirectToAction(nameof(Index), new { processId = pid });
            }
            return RedirectToAction(nameof(Index));

        }

        // --------------------------------------------------------------------
        // Generic admin notification helper for stored procedure parameter actions.
        // --------------------------------------------------------------------
        private async Task NotifyParamAsync(
    string eventKey,
    string subject,
    string body,
    int? processId = null,
    int? paramId = null,
    string? key = null,
    string? dataType = null,
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
                    type = "System",
                    eventKey,
                    url,
                    metadata = new
                    {
                        actorUserId,
                        actorUserName,
                        processId,
                        paramId,
                        key,
                        dataType,
                        texts,
                        extra = extraMeta
                    }
                })
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
