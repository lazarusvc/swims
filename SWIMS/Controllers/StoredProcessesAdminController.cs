using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models;
using SWIMS.Models.Security;
using SWIMS.Models.ViewModels;
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;
using SWIMS.Services.Notifications;
using SWIMS.Services.Diagnostics.Auditing;

namespace SWIMS.Controllers
{
    public class StoredProcessesAdminController : Controller
    {
        private readonly SwimsStoredProcsDbContext _db;
        private readonly IDataProtector? _protector;
        private readonly IElsaWorkflowQueue _elsaQueue;
        private readonly IAuditLogger _audit;

        public StoredProcessesAdminController(
            SwimsStoredProcsDbContext db,
            IDataProtectionProvider dp,
            IElsaWorkflowQueue elsaQueue,
            IAuditLogger audit)
        {
            _db = db;
            _protector = dp.CreateProtector(DataProtectionPurposes.StoredProcedures);
            _elsaQueue = elsaQueue;
            _audit = audit;
        }

        // GET: /StoredProcessesAdmin
        public async Task<IActionResult> Index()
        {
            var rows = await _db.StoredProcesses.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
            return View(rows);
        }

        // GET: /StoredProcessesAdmin/Create
        public IActionResult Create() => View(new StoredProcessEditViewModel());

        // POST: /StoredProcessesAdmin/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoredProcessEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (string.IsNullOrWhiteSpace(vm.ConnectionKey) &&
                (string.IsNullOrWhiteSpace(vm.DataSource) || string.IsNullOrWhiteSpace(vm.Database)))
            {
                ModelState.AddModelError(string.Empty, "Provide either a ConnectionKey or a DataSource + Database.");
                return View(vm);
            }

            var row = new StoredProcess
            {
                Name = vm.Name.Trim(),
                Description = vm.Description?.Trim(),
                ConnectionKey = string.IsNullOrWhiteSpace(vm.ConnectionKey) ? null : vm.ConnectionKey!.Trim(),
                DataSource = string.IsNullOrWhiteSpace(vm.DataSource) ? null : vm.DataSource!.Trim(),
                Database = string.IsNullOrWhiteSpace(vm.Database) ? null : vm.Database!.Trim(),
                DbUserEncrypted = string.IsNullOrWhiteSpace(vm.DbUser) ? null : Protect(vm.DbUser!),
                DbPasswordEncrypted = string.IsNullOrWhiteSpace(vm.DbPassword) ? null : Protect(vm.DbPassword!)
            };

            _db.StoredProcesses.Add(row);
            await _db.SaveChangesAsync();

            // 📝 Audit: Stored procedure created
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "StoredProcedureCreated",
                entity: "StoredProcess",
                entityId: row.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: null,
                newObj: new
                {
                    processId = row.Id,
                    row.Name,
                    row.Description,
                    row.ConnectionKey,
                    row.DataSource,
                    row.Database,
                    hasDbUser = !string.IsNullOrWhiteSpace(row.DbUserEncrypted),
                    hasDbPassword = !string.IsNullOrWhiteSpace(row.DbPasswordEncrypted)
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Stored procedure created
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.StoredProcedures.Created,
                subject: "Stored procedure created",
                body: $"Stored process '{row.Name}' was created.",
                processId: row.Id,
                processName: row.Name,
                url: Url.Action(nameof(Edit), new { id = row.Id }),
                texts: new
                {
                    actor = new { subject = "Stored procedure created", body = $"You created stored process '{row.Name}'." },
                    routed = new { subject = "Stored procedure created", body = $"{actorName} created stored process '{row.Name}'." },
                    superadmin = new { subject = "Stored procedure created", body = $"{actorName} created stored process '{row.Name}'." }
                },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


            return RedirectToAction("Index", "StoredProcessParams", new { processId = row.Id });
        }

        // GET: /StoredProcessesAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var row = await _db.StoredProcesses.FindAsync(id);
            if (row == null) return NotFound();

            var vm = new StoredProcessEditViewModel
            {
                Id = row.Id,
                Name = row.Name,
                Description = row.Description,
                ConnectionKey = row.ConnectionKey,
                DataSource = row.DataSource,
                Database = row.Database
                // Do NOT echo creds
            };
            return View(vm);
        }

        // POST: /StoredProcessesAdmin/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StoredProcessEditViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            if (string.IsNullOrWhiteSpace(vm.ConnectionKey) &&
                (string.IsNullOrWhiteSpace(vm.DataSource) || string.IsNullOrWhiteSpace(vm.Database)))
            {
                ModelState.AddModelError(string.Empty, "Provide either a ConnectionKey or a DataSource + Database.");
                return View(vm);
            }

            var row = await _db.StoredProcesses.FirstOrDefaultAsync(x => x.Id == id);
            if (row == null) return NotFound();

            var oldObj = new
            {
                processId = row.Id,
                row.Name,
                row.Description,
                row.ConnectionKey,
                row.DataSource,
                row.Database,
                hasDbUser = !string.IsNullOrWhiteSpace(row.DbUserEncrypted),
                hasDbPassword = !string.IsNullOrWhiteSpace(row.DbPasswordEncrypted)
            };

            row.Name = vm.Name.Trim();
            row.Description = vm.Description?.Trim();
            row.ConnectionKey = string.IsNullOrWhiteSpace(vm.ConnectionKey) ? null : vm.ConnectionKey!.Trim();
            row.DataSource = string.IsNullOrWhiteSpace(vm.DataSource) ? null : vm.DataSource!.Trim();
            row.Database = string.IsNullOrWhiteSpace(vm.Database) ? null : vm.Database!.Trim();

            if (!string.IsNullOrWhiteSpace(vm.DbUser)) row.DbUserEncrypted = Protect(vm.DbUser!);
            if (!string.IsNullOrWhiteSpace(vm.DbPassword)) row.DbPasswordEncrypted = Protect(vm.DbPassword!);

            await _db.SaveChangesAsync();

            // 📝 Audit: Stored procedure updated
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            var newObj = new
            {
                processId = row.Id,
                row.Name,
                row.Description,
                row.ConnectionKey,
                row.DataSource,
                row.Database,
                hasDbUser = !string.IsNullOrWhiteSpace(row.DbUserEncrypted),
                hasDbPassword = !string.IsNullOrWhiteSpace(row.DbPasswordEncrypted)
            };

            await _audit.TryLogAsync(
                action: "StoredProcedureUpdated",
                entity: "StoredProcess",
                entityId: row.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: oldObj,
                newObj: newObj,
                extra: new
                {
                    dbUserUpdated = !string.IsNullOrWhiteSpace(vm.DbUser),
                    dbPasswordUpdated = !string.IsNullOrWhiteSpace(vm.DbPassword)
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Stored procedure updated
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.StoredProcedures.Updated,
                subject: "Stored procedure updated",
                body: $"Stored process '{row.Name}' was updated.",
                processId: row.Id,
                processName: row.Name,
                url: Url.Action(nameof(Edit), new { id = row.Id }),
                texts: new
                {
                    actor = new { subject = "Stored procedure updated", body = $"You updated stored process '{row.Name}'." },
                    routed = new { subject = "Stored procedure updated", body = $"{actorName} updated stored process '{row.Name}'." },
                    superadmin = new { subject = "Stored procedure updated", body = $"{actorName} updated stored process '{row.Name}'." }
                },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


            return RedirectToAction(nameof(Index));
        }

        // GET: /StoredProcessesAdmin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.StoredProcesses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (row == null) return NotFound();
            return View(row);
        }

        // POST: /StoredProcessesAdmin/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var row = await _db.StoredProcesses.Include(x => x.Params).FirstOrDefaultAsync(x => x.Id == id);
            if (row != null)
            {
                var name = row.Name;
                var pid = row.Id;

                var oldObj = new
                {
                    processId = row.Id,
                    row.Name,
                    row.Description,
                    row.ConnectionKey,
                    row.DataSource,
                    row.Database,
                    hasDbUser = !string.IsNullOrWhiteSpace(row.DbUserEncrypted),
                    hasDbPassword = !string.IsNullOrWhiteSpace(row.DbPasswordEncrypted),
                    paramCount = row.Params?.Count ?? 0
                };

                _db.StoredProcesses.Remove(row); // cascade deletes params
                await _db.SaveChangesAsync();

                // 📝 Audit: Stored procedure deleted
                AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

                await _audit.TryLogAsync(
                    action: "StoredProcedureDeleted",
                    entity: "StoredProcess",
                    entityId: pid.ToString(),
                    userId: actorId,
                    username: actorUsername,
                    oldObj: oldObj,
                    newObj: null,
                    ct: HttpContext.RequestAborted);
                // 📝 Audit: END

                // 🔔 Notify: Stored procedure deleted
                var actorName = User?.Identity?.Name ?? "Someone";

                await NotifyAdminAsync(
                    eventKey: SwimsEventKeys.StoredProcedures.Deleted,
                    subject: "Stored procedure deleted",
                    body: $"Stored process '{name}' was deleted.",
                    processId: pid,
                    processName: name,
                    url: Url.Action(nameof(Index)),
                    texts: new
                    {
                        actor = new { subject = "Stored procedure deleted", body = $"You deleted stored process '{name}'." },
                        routed = new { subject = "Stored procedure deleted", body = $"{actorName} deleted stored process '{name}'." },
                        superadmin = new { subject = "Stored procedure deleted", body = $"{actorName} deleted stored process '{name}'." }
                    },
                    ct: HttpContext.RequestAborted);

                // 🔔 Notify: END

            }
            return RedirectToAction(nameof(Index));

        }

        // --------------------------------------------------------------------
        // Generic admin notification helper for stored procedure admin actions.
        // --------------------------------------------------------------------
        private async Task NotifyAdminAsync(
    string eventKey,
    string subject,
    string body,
    int? processId = null,
    string? processName = null,
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
                        processName,
                        texts,
                        extra = extraMeta
                    }
                })
            };

            try
            {
                // 🔔 Notify: Stored procedure admin event
                await _elsaQueue.EnqueueByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
            }
        }



        private string Protect(string plaintext) =>
            _protector == null ? plaintext : _protector.Protect(plaintext);
    }
}
