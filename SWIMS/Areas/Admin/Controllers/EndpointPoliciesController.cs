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

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EndpointPoliciesController : Controller
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IEndpointPolicyAssignmentStore _store;
        private readonly IEndpointCatalog _catalog;
        private readonly IElsaWorkflowClient _elsa;

        public EndpointPoliciesController(
            SwimsIdentityDbContext db,
            IEndpointPolicyAssignmentStore store,
            IEndpointCatalog catalog,
            IElsaWorkflowClient elsa)
        {
            _db = db;
            _store = store;
            _catalog = catalog;
            _elsa = elsa;
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

            // 🔔 Notify: Endpoint policy assignment created
            await NotifyAdminAsync(
                subject: "Endpoint policy assignment created",
                body: $"Endpoint assignment for policy '{policy.Name}' was created.",
                metadata: new
                {
                    action = "EndpointPolicyAssignmentCreated",
                    assignmentId = row.Id,
                    policyId = policy.Id,
                    policyName = policy.Name,
                    matchType = row.MatchType,
                    area = row.Area,
                    controller = row.Controller,
                    actionName = row.Action,
                    page = row.Page,
                    path = row.Path
                });

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

            x.MatchType = vm.MatchType; x.Area = vm.Area; x.Controller = vm.Controller; x.Action = vm.Action;
            x.Page = vm.Page; x.Path = vm.Path; x.Regex = vm.Regex;
            x.PolicyId = policy.Id; x.Policy = policy; x.PolicyName = policy.Name;
            x.Notes = vm.Notes; x.IsEnabled = vm.IsEnabled; x.Priority = vm.Priority;
            x.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 🔔 Notify: Endpoint policy assignment updated
            await NotifyAdminAsync(
                subject: "Endpoint policy assignment updated",
                body: $"Endpoint assignment for policy '{policy.Name}' was updated.",
                metadata: new
                {
                    action = "EndpointPolicyAssignmentUpdated",
                    assignmentId = x.Id,
                    policyId = policy.Id,
                    policyName = policy.Name,
                    matchType = x.MatchType,
                    area = x.Area,
                    controller = x.Controller,
                    actionName = x.Action,
                    page = x.Page,
                    path = x.Path
                });

            TempData["Ok"] = "Endpoint policy assignment updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var x = await _db.EndpointPolicyAssignments.FindAsync(id);
            if (x is null) return NotFound();

            x.IsEnabled = !x.IsEnabled;
            x.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 🔔 Notify: Endpoint policy assignment toggled
            await NotifyAdminAsync(
                subject: "Endpoint policy assignment toggled",
                body: $"Endpoint assignment ID {x.Id} was {(x.IsEnabled ? "enabled" : "disabled")}.",
                metadata: new
                {
                    action = "EndpointPolicyAssignmentToggled",
                    assignmentId = x.Id,
                    isEnabled = x.IsEnabled
                });

            TempData["Ok"] = $"Assignment {(x.IsEnabled ? "enabled" : "disabled")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.EndpointPolicyAssignments.FindAsync(id);
            if (x is null) return NotFound();

            var assignmentId = x.Id;

            _db.EndpointPolicyAssignments.Remove(x);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 🔔 Notify: Endpoint policy assignment deleted
            await NotifyAdminAsync(
                subject: "Endpoint policy assignment deleted",
                body: $"Endpoint assignment ID {assignmentId} was deleted.",
                metadata: new
                {
                    action = "EndpointPolicyAssignmentDeleted",
                    assignmentId = assignmentId
                });

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

            // 🔔 Notify: Endpoint policy bulk assignments created
            if (created > 0)
            {
                await NotifyAdminAsync(
                    subject: "Endpoint policy assignments created",
                    body: $"Bulk created {created} endpoint policy assignment(s) for policy '{policy.Name}'.",
                    metadata: new
                    {
                        action = "EndpointPolicyAssignmentsBulkCreated",
                        createdCount = created,
                        policyId = policy.Id,
                        policyName = policy.Name
                    });
            }

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

        private async Task NotifyAdminAsync(string subject, string body, object? metadata = null)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? recipient = !string.IsNullOrWhiteSpace(userIdClaim)
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
                // 🔔 Notify: Admin authorization config event
                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload);
            }
            catch
            {
            }
        }


    }
}
