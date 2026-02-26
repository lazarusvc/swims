using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Areas.Admin.ViewModels.AccessControl;
using SWIMS.Data;
using SWIMS.Models.Security;
using SWIMS.Services.Auth;
using SWIMS.Services.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;
using SWIMS.Models.Notifications;
using SWIMS.Services.Notifications;
using SWIMS.Services.Diagnostics.Auditing;


namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EndpointPoliciesController : Controller
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IEndpointPolicyAssignmentStore _store;
        private readonly IEndpointCatalog _catalog;
        private readonly IElsaWorkflowClient _elsa;
        private readonly IAuditLogger _audit;

        public EndpointPoliciesController(
            SwimsIdentityDbContext db,
            IEndpointPolicyAssignmentStore store,
            IEndpointCatalog catalog,
            IElsaWorkflowClient elsa,
            IAuditLogger audit)
        {
            _db = db;
            _store = store;
            _catalog = catalog;
            _elsa = elsa;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var rows = await _db.EndpointPolicyAssignments.AsNoTracking()
                .OrderBy(x => x.Priority).ThenBy(x => x.MatchType)
                .Select(x => new EndpointPolicyListItemViewModel
                {
                    Id = x.Id,
                    MatchType = x.MatchType,
                    Area = x.Area,
                    Controller = x.Controller,
                    Action = x.Action,
                    Page = x.Page,
                    Path = x.Path,
                    Regex = x.Regex,
                    PolicyName = x.PolicyName,
                    Notes = x.Notes,
                    IsEnabled = x.IsEnabled,
                    Priority = x.Priority,
                    UpdatedAt = x.UpdatedAt
                }).ToListAsync();

            return View(rows);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new EndpointPolicyEditViewModel
            {
                Policies = await PolicyOptionsAsync()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EndpointPolicyEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Policies = await PolicyOptionsAsync(vm.PolicyName);
                return View(vm);
            }

            var policy = await _db.AuthorizationPolicies.FirstOrDefaultAsync(p => p.Name == vm.PolicyName && p.IsEnabled);
            if (policy is null)
            {
                ModelState.AddModelError(nameof(vm.PolicyName), "Policy not found or disabled.");
                vm.Policies = await PolicyOptionsAsync(vm.PolicyName);
                return View(vm);
            }

            var row = new EndpointPolicyAssignment
            {
                MatchType = vm.MatchType,
                Area = vm.Area,
                Controller = vm.Controller,
                Action = vm.Action,
                Page = vm.Page,
                Path = vm.Path,
                Regex = vm.Regex,
                PolicyId = policy.Id,
                Policy = policy,
                PolicyName = policy.Name,
                Notes = vm.Notes,
                IsEnabled = vm.IsEnabled,
                Priority = vm.Priority,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.EndpointPolicyAssignments.Add(row);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 📝 Audit: Endpoint policy assignment created
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "EndpointPolicyAssignmentCreated",
                entity: "EndpointPolicyAssignment",
                entityId: row.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: null,
                newObj: new
                {
                    assignmentId = row.Id,
                    policyId = policy.Id,
                    policyName = policy.Name,
                    matchType = row.MatchType,
                    area = row.Area,
                    controller = row.Controller,
                    actionName = row.Action,
                    page = row.Page,
                    path = row.Path,
                    regex = row.Regex,
                    notes = row.Notes,
                    isEnabled = row.IsEnabled,
                    priority = row.Priority
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Endpoint policy assignment created
            var actorName = User?.Identity?.Name ?? "An admin";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.EndpointPolicyAssignments.Created,
                subject: "Endpoint policy assignment created",
                body: $"You created endpoint policy assignment #{row.Id} for policy '{policy.Name}'.",
                url: Url.Action(nameof(Edit), new { id = row.Id }),
                extraMeta_: new
                {
                    assignmentId = row.Id,
                    policyId = policy.Id,
                    policyName = policy.Name,
                    matchType = row.MatchType,
                    area = row.Area,
                    controller = row.Controller,
                    actionName = row.Action,
                    page = row.Page,
                    path = row.Path,
                    regex = row.Regex,
                    notes = row.Notes,
                    isEnabled = row.IsEnabled,
                    priority = row.Priority
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Endpoint policy assignment created",
                        body = $"You created endpoint policy assignment #{row.Id} for policy '{policy.Name}'."
                    },
                    routed = new
                    {
                        subject = "Endpoint policy assignment created",
                        body = $"{actorName} created endpoint policy assignment #{row.Id} for policy '{policy.Name}'."
                    },
                    superadmin = new
                    {
                        subject = "Endpoint policy assignment created",
                        body = $"{actorName} created endpoint policy assignment #{row.Id} for policy '{policy.Name}'."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = "Endpoint policy assignment created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var x = await _db.EndpointPolicyAssignments.Include(e => e.Policy).FirstOrDefaultAsync(e => e.Id == id);
            if (x is null) return NotFound();

            var vm = new EndpointPolicyEditViewModel
            {
                Id = x.Id,
                MatchType = x.MatchType,
                Area = x.Area,
                Controller = x.Controller,
                Action = x.Action,
                Page = x.Page,
                Path = x.Path,
                Regex = x.Regex,
                PolicyName = x.PolicyName,
                PolicyId = x.PolicyId,
                Notes = x.Notes,
                IsEnabled = x.IsEnabled,
                Priority = x.Priority,
                Policies = await PolicyOptionsAsync(x.PolicyName)
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EndpointPolicyEditViewModel vm)
        {
            var x = await _db.EndpointPolicyAssignments.Include(e => e.Policy).FirstOrDefaultAsync(e => e.Id == id);
            if (x is null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.Policies = await PolicyOptionsAsync(vm.PolicyName);
                return View(vm);
            }

            var policy = await _db.AuthorizationPolicies.FirstOrDefaultAsync(p => p.Name == vm.PolicyName && p.IsEnabled);
            if (policy is null)
            {
                ModelState.AddModelError(nameof(vm.PolicyName), "Policy not found or disabled.");
                vm.Policies = await PolicyOptionsAsync(vm.PolicyName);
                return View(vm);
            }

            var oldObj = new
            {
                assignmentId = x.Id,
                policyId = x.PolicyId,
                policyName = x.PolicyName,
                matchType = x.MatchType,
                area = x.Area,
                controller = x.Controller,
                actionName = x.Action,
                page = x.Page,
                path = x.Path,
                regex = x.Regex,
                notes = x.Notes,
                isEnabled = x.IsEnabled,
                priority = x.Priority
            };

            x.MatchType = vm.MatchType; x.Area = vm.Area; x.Controller = vm.Controller; x.Action = vm.Action;
            x.Page = vm.Page; x.Path = vm.Path; x.Regex = vm.Regex;
            x.PolicyId = policy.Id; x.Policy = policy; x.PolicyName = policy.Name;
            x.Notes = vm.Notes; x.IsEnabled = vm.IsEnabled; x.Priority = vm.Priority;
            x.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 📝 Audit: Endpoint policy assignment updated
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            var newObj = new
            {
                assignmentId = x.Id,
                policyId = policy.Id,
                policyName = policy.Name,
                matchType = x.MatchType,
                area = x.Area,
                controller = x.Controller,
                actionName = x.Action,
                page = x.Page,
                path = x.Path,
                regex = x.Regex,
                notes = x.Notes,
                isEnabled = x.IsEnabled,
                priority = x.Priority
            };

            await _audit.TryLogAsync(
                action: "EndpointPolicyAssignmentUpdated",
                entity: "EndpointPolicyAssignment",
                entityId: x.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: oldObj,
                newObj: newObj,
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Endpoint policy assignment updated
            var actorName = User?.Identity?.Name ?? "An admin";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.EndpointPolicyAssignments.Updated,
                subject: "Endpoint policy assignment updated",
                body: $"You updated endpoint policy assignment #{x.Id} for policy '{policy.Name}'.",
                url: Url.Action(nameof(Edit), new { id = x.Id }),
                extraMeta_: new
                {
                    assignmentId = x.Id,
                    policyId = policy.Id,
                    policyName = policy.Name,
                    matchType = x.MatchType,
                    area = x.Area,
                    controller = x.Controller,
                    actionName = x.Action,
                    page = x.Page,
                    path = x.Path,
                    regex = x.Regex,
                    notes = x.Notes,
                    isEnabled = x.IsEnabled,
                    priority = x.Priority
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Endpoint policy assignment updated",
                        body = $"You updated endpoint policy assignment #{x.Id} for policy '{policy.Name}'."
                    },
                    routed = new
                    {
                        subject = "Endpoint policy assignment updated",
                        body = $"{actorName} updated endpoint policy assignment #{x.Id} for policy '{policy.Name}'."
                    },
                    superadmin = new
                    {
                        subject = "Endpoint policy assignment updated",
                        body = $"{actorName} updated endpoint policy assignment #{x.Id} for policy '{policy.Name}'."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = "Endpoint policy assignment updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var x = await _db.EndpointPolicyAssignments.FindAsync(id);
            if (x is null) return NotFound();

            var oldIsEnabled = x.IsEnabled;

            x.IsEnabled = !x.IsEnabled;
            x.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 📝 Audit: Endpoint policy assignment toggled
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "EndpointPolicyAssignmentToggled",
                entity: "EndpointPolicyAssignment",
                entityId: x.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: new { isEnabled = oldIsEnabled },
                newObj: new { isEnabled = x.IsEnabled },
                extra: new { assignmentId = x.Id },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Endpoint policy assignment toggled
            var actorName = User?.Identity?.Name ?? "An admin";
            var stateWord = x.IsEnabled ? "enabled" : "disabled";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.EndpointPolicyAssignments.Toggled,
                subject: "Endpoint policy assignment toggled",
                body: $"You {stateWord} endpoint policy assignment #{x.Id}.",
                url: Url.Action(nameof(Edit), new { id = x.Id }),
                extraMeta_: new
                {
                    assignmentId = x.Id,
                    isEnabled = x.IsEnabled
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Endpoint policy assignment toggled",
                        body = $"You {stateWord} endpoint policy assignment #{x.Id}."
                    },
                    routed = new
                    {
                        subject = "Endpoint policy assignment toggled",
                        body = $"{actorName} {stateWord} endpoint policy assignment #{x.Id}."
                    },
                    superadmin = new
                    {
                        subject = "Endpoint policy assignment toggled",
                        body = $"{actorName} {stateWord} endpoint policy assignment #{x.Id}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = $"Assignment {(x.IsEnabled ? "enabled" : "disabled")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.EndpointPolicyAssignments.FindAsync(id);
            if (x is null) return NotFound();

            var assignmentId = x.Id;

            var oldObj = new
            {
                assignmentId = x.Id,
                policyId = x.PolicyId,
                policyName = x.PolicyName,
                matchType = x.MatchType,
                area = x.Area,
                controller = x.Controller,
                actionName = x.Action,
                page = x.Page,
                path = x.Path,
                regex = x.Regex,
                notes = x.Notes,
                isEnabled = x.IsEnabled,
                priority = x.Priority
            };

            _db.EndpointPolicyAssignments.Remove(x);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 📝 Audit: Endpoint policy assignment deleted
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "EndpointPolicyAssignmentDeleted",
                entity: "EndpointPolicyAssignment",
                entityId: assignmentId.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: oldObj,
                newObj: null,
                extra: new { assignmentId },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Endpoint policy assignment deleted
            var actorName = User?.Identity?.Name ?? "An admin";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.EndpointPolicyAssignments.Deleted,
                subject: "Endpoint policy assignment deleted",
                body: $"You deleted endpoint policy assignment #{assignmentId}.",
                url: Url.Action(nameof(Index)),
                extraMeta_: new
                {
                    assignmentId = assignmentId
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Endpoint policy assignment deleted",
                        body = $"You deleted endpoint policy assignment #{assignmentId}."
                    },
                    routed = new
                    {
                        subject = "Endpoint policy assignment deleted",
                        body = $"{actorName} deleted endpoint policy assignment #{assignmentId}."
                    },
                    superadmin = new
                    {
                        subject = "Endpoint policy assignment deleted",
                        body = $"{actorName} deleted endpoint policy assignment #{assignmentId}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = "Assignment deleted.";
            return RedirectToAction(nameof(Index));

        }

        private async Task<List<SelectListItem>> PolicyOptionsAsync(string? selected = null)
        {
            var items = await _db.AuthorizationPolicies.AsNoTracking()
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Name, Text = p.Name })
                .ToListAsync();

            foreach (var i in items)
                i.Selected = string.Equals(i.Value, selected, StringComparison.OrdinalIgnoreCase);

            return items;
        }


        public async Task<IActionResult> BulkCreate()
        {
            var vm = new EndpointPolicyEditViewModel
            {
                Policies = await PolicyOptionsAsync()
            };
            ViewBag.Controllers = _catalog.GetControllers();     // (Area, Controller)
            ViewBag.Actions = _catalog.GetControllerActions();   // Area, Controller, Action
            ViewBag.Pages = _catalog.GetRazorPages();            // Area, PageRoute
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(EndpointPolicyEditViewModel vm, string[] controllerActions, string[] pages)
        {
            if (string.IsNullOrWhiteSpace(vm.PolicyName))
            {
                ModelState.AddModelError(nameof(vm.PolicyName), "Select a policy.");
            }
            var policy = await _db.AuthorizationPolicies.FirstOrDefaultAsync(p => p.Name == vm.PolicyName && p.IsEnabled);
            if (policy is null)
            {
                ModelState.AddModelError(nameof(vm.PolicyName), "Policy not found or disabled.");
            }

            if (!ModelState.IsValid)
            {
                vm.Policies = await PolicyOptionsAsync(vm.PolicyName);
                ViewBag.Controllers = _catalog.GetControllers();
                ViewBag.Actions = _catalog.GetControllerActions();
                ViewBag.Pages = _catalog.GetRazorPages();
                return View(vm);
            }

            int created = 0;

            foreach (var ca in controllerActions ?? Array.Empty<string>())
            {
                // format: area|controller|action (area may be empty)
                var parts = ca.Split('|');
                var area = parts[0]; var controller = parts[1]; var action = parts[2];

                bool exists = await _db.EndpointPolicyAssignments.AnyAsync(x =>
                    x.MatchType == MatchTypes.ControllerAction &&
                    (x.Area ?? "") == (area ?? "") &&
                    x.Controller == controller && x.Action == action &&
                    x.PolicyId == policy.Id);

                if (!exists)
                {
                    _db.EndpointPolicyAssignments.Add(new EndpointPolicyAssignment
                    {
                        MatchType = MatchTypes.ControllerAction,
                        Area = string.IsNullOrEmpty(area) ? null : area,
                        Controller = controller,
                        Action = action,
                        PolicyId = policy.Id,
                        Policy = policy,
                        PolicyName = policy.Name,
                        IsEnabled = vm.IsEnabled,
                        Priority = vm.Priority,
                        Notes = vm.Notes,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                    created++;
                }
            }

            foreach (var pr in pages ?? Array.Empty<string>())
            {
                // format: area|page
                var parts = pr.Split('|');
                var area = parts[0]; var page = parts[1];

                bool exists = await _db.EndpointPolicyAssignments.AnyAsync(x =>
                    x.MatchType == MatchTypes.RazorPage &&
                    (x.Area ?? "") == (area ?? "") &&
                    x.Page == page && x.PolicyId == policy.Id);

                if (!exists)
                {
                    _db.EndpointPolicyAssignments.Add(new EndpointPolicyAssignment
                    {
                        MatchType = MatchTypes.RazorPage,
                        Area = string.IsNullOrEmpty(area) ? null : area,
                        Page = page,
                        PolicyId = policy.Id,
                        Policy = policy,
                        PolicyName = policy.Name,
                        IsEnabled = vm.IsEnabled,
                        Priority = vm.Priority,
                        Notes = vm.Notes,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                    created++;
                }
            }

            if (created > 0)
                await _db.SaveChangesAsync();

            await _store.InvalidateAsync();

            // 📝 Audit: Endpoint policy bulk assignments created
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "EndpointPolicyAssignmentsBulkCreated",
                entity: "AuthorizationPolicy",
                entityId: policy.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: null,
                newObj: new
                {
                    policyId = policy.Id,
                    policyName = policy.Name,
                    createdCount = created,
                    isEnabled = vm.IsEnabled,
                    priority = vm.Priority,
                    controllerActionsSelected = controllerActions?.Length ?? 0,
                    pagesSelected = pages?.Length ?? 0
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Endpoint policy bulk assignments created
            var actorName = User?.Identity?.Name ?? "An admin";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.EndpointPolicyAssignments.BulkCreated,
                subject: "Endpoint policy assignments created",
                body: $"You bulk created {created} endpoint policy assignment(s) for policy '{policy.Name}'.",
                url: Url.Action(nameof(Index)),
                extraMeta_: new
                {
                    policyId = policy.Id,
                    policyName = policy.Name,
                    createdCount = created
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Endpoint policy assignments created",
                        body = $"You bulk created {created} endpoint policy assignment(s) for policy '{policy.Name}'."
                    },
                    routed = new
                    {
                        subject = "Endpoint policy assignments created",
                        body = $"{actorName} bulk created {created} endpoint policy assignment(s) for policy '{policy.Name}'."
                    },
                    superadmin = new
                    {
                        subject = "Endpoint policy assignments created",
                        body = $"{actorName} bulk created {created} endpoint policy assignment(s) for policy '{policy.Name}'."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = created > 0
                ? $"Created {created} assignment(s)."
                : "No new assignments were created (duplicates skipped).";

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> CreatePreset(
            [FromQuery] string matchType,
            [FromQuery(Name = "area")] string? area,
            [FromQuery(Name = "controller")] string? controllerName,
            [FromQuery(Name = "action")] string? actionName,
            [FromQuery(Name = "page")] string? page,
            [FromQuery(Name = "path")] string? path)
        {
            var vm = new EndpointPolicyEditViewModel
            {
                MatchType = string.IsNullOrWhiteSpace(matchType) ? MatchTypes.ControllerAction : matchType,
                Area = area,
                Controller = controllerName,
                Action = actionName,
                Page = page,
                Path = path,
                IsEnabled = true,
                Priority = 100,
                Policies = await PolicyOptionsAsync()
            };
            return View("Create", vm);
        }

        private async Task NotifyAdminAsync(
    string eventKey,
    string subject,
    string body,
    object? extraMeta_ = null,
    object? texts_ = null,
    string? url = null,
    int? targetUserId = null,
    IEnumerable<int>? targetUserIds = null,
    CancellationToken ct = default)
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
                            actorUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                            actorUserName = User?.Identity?.Name,

                            targetUserId,
                            targetUserIds = targetUserIds?.ToArray(),

                            texts = texts_,
                            extra = extraMeta_
                        }
                    })
                };

                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
            }
        }



    }
}
