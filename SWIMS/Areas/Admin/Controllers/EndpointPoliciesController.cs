using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Areas.Admin.ViewModels.AccessControl;
using SWIMS.Data;
using SWIMS.Models.Security;
using SWIMS.Services.Auth;
using SWIMS.Services.Diagnostics;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EndpointPoliciesController : Controller
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IEndpointPolicyAssignmentStore _store;
        private readonly IEndpointCatalog _catalog;

        public EndpointPoliciesController(SwimsIdentityDbContext db, IEndpointPolicyAssignmentStore store, IEndpointCatalog catalog)
        { _db = db; _store = store; _catalog = catalog; }

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
            TempData["Ok"] = $"Assignment {(x.IsEnabled ? "enabled" : "disabled")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.EndpointPolicyAssignments.FindAsync(id);
            if (x is null) return NotFound();

            _db.EndpointPolicyAssignments.Remove(x);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();
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
    }
}
