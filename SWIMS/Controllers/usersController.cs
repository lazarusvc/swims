// -------------------------------------------------------------------
// File:    usersController.cs (fixed)
// Purpose: Avoid nested active readers by materializing queries before
//          calling async APIs that open additional readers on the same
//          DbContext connection.
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // <-- added for ToListAsync/AsNoTracking
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using SWIMS.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    public class usersController : Controller
    {
        private readonly UserManager<SwUser> _userManager;
        private readonly RoleManager<SwRole> _roleManager;

        public usersController(UserManager<SwUser> userManager,
                               RoleManager<SwRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: users
        public async Task<IActionResult> Index()
        {
            // IMPORTANT: materialize first to avoid nested active data readers
            var users = await _userManager.Users
                                          .AsNoTracking()
                                          .ToListAsync();

            var list = new List<UserWithRolesViewModel>(users.Count);
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserWithRolesViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    Email = u.Email,
                    Roles = roles
                });
            }
            return View(list);
        }

        // GET: users/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var u = await _userManager.FindByIdAsync(id.ToString());
            if (u == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(u);
            var vm = new UserWithRolesViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                Email = u.Email,
                Roles = roles
            };
            return View(vm);
        }

        // GET: users/Create
        public async Task<IActionResult> Create()
        {
            // Materialize role names before rendering (no open reader left around)
            var roleNames = await _roleManager.Roles
                                              .Select(r => r.Name)
                                              .ToListAsync();
            ViewBag.Roles = new SelectList(roleNames);
            return View();
        }

        // POST: users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FirstName,LastName,UserName,Email")] SwUser swUser,
            string password,
            string role)
        {
            if (!ModelState.IsValid)
            {
                var roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                ViewBag.Roles = new SelectList(roleNames, role);
                return View(swUser);
            }

            swUser.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(swUser, password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);

                var roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                ViewBag.Roles = new SelectList(roleNames, role);
                return View(swUser);
            }

            if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
                await _userManager.AddToRoleAsync(swUser, role);

            return RedirectToAction(nameof(Index));
        }

        // GET: users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var roleNames = await _roleManager.Roles
                                              .Select(r => r.Name)
                                              .ToListAsync();
            var selected = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            ViewBag.Roles = new SelectList(roleNames, selected);

            return View(user);
        }

        // POST: users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("FirstName,LastName,UserName,Email")] SwUser swUser,
            string role)
        {
            if (!ModelState.IsValid)
            {
                var roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                ViewBag.Roles = new SelectList(roleNames, role);
                return View(swUser);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.FirstName = swUser.FirstName;
            user.LastName = swUser.LastName;
            user.UserName = swUser.UserName;
            user.Email = swUser.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors)
                    ModelState.AddModelError("", err.Description);

                var roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                ViewBag.Roles = new SelectList(roleNames, role);
                return View(swUser);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.FirstOrDefault() != role)
            {
                if (currentRoles.Any())
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
                    await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _userManager.FindByIdAsync(id.ToString());
            if (u == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(u);
            var vm = new UserWithRolesViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                Email = u.Email,
                Roles = roles
            };
            return View(vm);
        }

        // POST: users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
                await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }

        // GET: users/ManageRoles/5
        public async Task<IActionResult> ManageRoles(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles
                                             .Select(r => new { r.Id, r.Name })
                                             .ToListAsync();
            var userRoleNames = await _userManager.GetRolesAsync(user);

            var vm = new EditUserRolesVM
            {
                UserId = user.Id,
                DisplayName = user.Email ?? user.UserName ?? $"User {user.Id}",
                Roles = allRoles.Select(r => new RoleChoiceVM
                {
                    RoleId = r.Id,
                    RoleName = r.Name!,
                    Selected = userRoleNames.Contains(r.Name!)
                }).OrderBy(x => x.RoleName).ToList()
            };

            return View(vm);
        }

        // POST: users/ManageRoles/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(int id, EditUserRolesVM model)
        {
            if (id != model.UserId) return NotFound();

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var currentRoleNames = await _userManager.GetRolesAsync(user);

            // Desired roles from checkboxes
            var desiredRoleNames = new List<string>();
            foreach (var rc in model.Roles)
            {
                if (rc.Selected)
                {
                    var role = await _roleManager.FindByIdAsync(rc.RoleId.ToString());
                    if (role != null && !string.IsNullOrWhiteSpace(role.Name))
                        desiredRoleNames.Add(role.Name);
                }
            }

            var toAdd = desiredRoleNames.Except(currentRoleNames).ToList();
            var toRemove = currentRoleNames.Except(desiredRoleNames).ToList();

            if (toAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, toAdd);
                if (!addResult.Succeeded)
                    foreach (var e in addResult.Errors) ModelState.AddModelError("", e.Description);
            }

            if (toRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removeResult.Succeeded)
                    foreach (var e in removeResult.Errors) ModelState.AddModelError("", e.Description);
            }

            if (!ModelState.IsValid)
            {
                // Rehydrate display name on error
                model.DisplayName = user.Email ?? user.UserName ?? $"User {user.Id}";
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}