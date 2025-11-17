using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Areas.Admin.ViewModels.AuthorizationPolicies;
using SWIMS.Data;
using SWIMS.Models.Security;
using SWIMS.Services.Auth;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthorizationPoliciesController : Controller
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IPolicyStore _store;

        public AuthorizationPoliciesController(SwimsIdentityDbContext db, IPolicyStore store)
        {
            _db = db;
            _store = store;
        }

        // GET: /Admin/AuthorizationPolicies
        public async Task<IActionResult> Index()
        {
            var rows = await _db.AuthorizationPolicies
                .AsNoTracking()
                .Include(p => p.Roles)
                .OrderBy(p => p.Name)
                .Select(p => new PolicyListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    IsEnabled = p.IsEnabled,
                    IsSystem = p.IsSystem,
                    RoleNames = p.Roles.Select(r => r.RoleName).OrderBy(n => n).ToList(),
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return View(rows);
        }

        // GET: /Admin/AuthorizationPolicies/Create
        public async Task<IActionResult> Create()
        {
            var vm = new PolicyEditViewModel
            {
                AllRoles = await BuildRoleSelectListAsync()
            };
            return View(vm);
        }

        // POST: /Admin/AuthorizationPolicies/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PolicyEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AllRoles = await BuildRoleSelectListAsync(vm.SelectedRoleIds);
                return View(vm);
            }

            var exists = await _db.AuthorizationPolicies.AnyAsync(p => p.Name == vm.Name);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Name), "A policy with this name already exists.");
                vm.AllRoles = await BuildRoleSelectListAsync(vm.SelectedRoleIds);
                return View(vm);
            }

            var policy = new AuthorizationPolicyEntity
            {
                Name = vm.Name.Trim(),
                Description = vm.Description,
                IsEnabled = vm.IsEnabled,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await ReplaceRolesAsync(policy, vm.SelectedRoleIds);

            _db.AuthorizationPolicies.Add(policy);
            await _db.SaveChangesAsync();

            await _store.InvalidateAsync(policy.Name);
            TempData["Ok"] = $"Policy '{policy.Name}' created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/AuthorizationPolicies/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var policy = await _db.AuthorizationPolicies
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (policy is null) return NotFound();

            // Prevent editing of system policies
            if (policy.IsSystem)
            {
                TempData["Ok"] = "This is a system policy and cannot be edited.";
                return RedirectToAction(nameof(Index));
            }


            var vm = new PolicyEditViewModel
            {
                Id = policy.Id,
                Name = policy.Name, // immutable from UI
                Description = policy.Description,
                IsEnabled = policy.IsEnabled,
                SelectedRoleIds = policy.Roles.Select(r => r.RoleId).ToList(),
                AllRoles = await BuildRoleSelectListAsync(policy.Roles.Select(r => r.RoleId))
            };

            return View(vm);
        }

        // POST: /Admin/AuthorizationPolicies/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PolicyEditViewModel vm)
        {
            var policy = await _db.AuthorizationPolicies
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (policy is null) return NotFound();

            // Prevent editing of system policies
            if (policy.IsSystem)
            {
                TempData["Ok"] = "This is a system policy and cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            // keep name immutable
            vm.Name = policy.Name;

            if (!ModelState.IsValid)
            {
                vm.AllRoles = await BuildRoleSelectListAsync(vm.SelectedRoleIds);
                return View(vm);
            }

            policy.Description = vm.Description;
            policy.IsEnabled = vm.IsEnabled;
            policy.UpdatedAt = DateTimeOffset.UtcNow;

            await ReplaceRolesAsync(policy, vm.SelectedRoleIds);

            await _db.SaveChangesAsync();
            await _store.InvalidateAsync(policy.Name);

            TempData["Ok"] = $"Policy '{policy.Name}' updated.";
            return RedirectToAction(nameof(Index));
        }

        private static bool IsSystemPolicy(string name)
            => string.Equals(name, "AdminOnly", StringComparison.OrdinalIgnoreCase);

        // POST: /Admin/AuthorizationPolicies/Toggle
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var policy = await _db.AuthorizationPolicies.FirstOrDefaultAsync(p => p.Id == id);
            if (policy is null) return NotFound();

            if (policy.IsSystem)
            {
                TempData["Ok"] = "This is a system policy and cannot be disabled.";
                return RedirectToAction(nameof(Index));
            }

            policy.IsEnabled = !policy.IsEnabled;
            policy.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync(policy.Name);

            TempData["Ok"] = $"Policy '{policy.Name}' {(policy.IsEnabled ? "enabled" : "disabled")}.";
            return RedirectToAction(nameof(Index));
        }



        // POST: /Admin/AuthorizationPolicies/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var policy = await _db.AuthorizationPolicies.FirstOrDefaultAsync(p => p.Id == id);
            if (policy is null) return NotFound();

            if (policy.IsSystem)
            {
                TempData["Ok"] = "This is a system policy and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _db.AuthorizationPolicies.Remove(policy);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync(policy.Name);

            TempData["Ok"] = $"Policy '{policy.Name}' deleted.";
            return RedirectToAction(nameof(Index));
        }


        // Helpers
        private async Task<List<SelectListItem>> BuildRoleSelectListAsync(IEnumerable<int>? selectedIds = null)
        {
            var selected = selectedIds?.ToHashSet() ?? new HashSet<int>();
            return await _db.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name!,
                    Selected = selected.Contains(r.Id)
                })
                .ToListAsync();
        }

        private async Task ReplaceRolesAsync(AuthorizationPolicyEntity policy, IList<int> selectedRoleIds)
        {
            policy.Roles.Clear();

            if (selectedRoleIds?.Count > 0)
            {
                var roles = await _db.Roles
                    .Where(r => selectedRoleIds.Contains(r.Id))
                    .ToListAsync();

                foreach (var role in roles)
                {
                    policy.Roles.Add(new AuthorizationPolicyRole
                    {
                        RoleId = role.Id,
                        Role = role,
                        RoleName = role.Name!
                    });
                }
            }
        }
    }
}
