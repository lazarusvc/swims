// -------------------------------------------------------------------
// File:    rolesController.cs
// Author:  N/A
// Created: N/A
// Purpose: Provides CRUD operations for roles within the SWIMS application.
//          Only users in the "Admin" role may access these actions.
// Dependencies:
//   - SwRole (ASP.NET Core Identity role entity)
//   - RoleManager<SwRole> (Identity service for role management)
//   - Microsoft.AspNetCore.Mvc, Microsoft.AspNetCore.Authorization, Microsoft.AspNetCore.Identity
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using SWIMS.Security;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    /// <summary>
    /// Controller for creating, reading, updating, and deleting <see cref="SwRole"/> entities.
    /// Secured so that only users assigned to the "Admin" role may invoke its endpoints.
    /// </summary>
    public class rolesController : Controller
    {
        private readonly RoleManager<SwRole> _roleManager;
        private readonly UserManager<SwUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the rolesController.
        /// </summary>
        /// <param name="roleManager">Identity RoleManager for <see cref="SwRole"/>.</param>
        /// <param name="userManager">Identity UserManager for <see cref="SwUser"/>.</param>
        public rolesController(RoleManager<SwRole> roleManager, UserManager<SwUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves and displays all roles defined in the system.
        /// </summary>
        /// <returns>
        /// A <see cref="ViewResult"/> containing a list of <see cref="SwRole"/> instances.
        /// </returns>
        // GET: roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        /// <summary>
        /// Shows detailed information for the role with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the role.</param>
        /// <returns>
        /// A <see cref="ViewResult"/> if the role exists; otherwise, a <see cref="NotFoundResult"/>.
        /// </returns>
        // GET: roles/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();
            return View(role);
        }

        /// <summary>
        /// Renders a form for creating a new role.
        /// </summary>
        /// <returns>A <see cref="ViewResult"/> with an empty <see cref="SwRole"/> model.</returns>
        // GET: roles/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Handles the submission of a new role to be created.
        /// </summary>
        /// <param name="role">
        /// A <see cref="SwRole"/> instance (its <c>Name</c> property must be set).
        /// </param>
        /// <returns>
        /// Redirects to <see cref="Index"/> on success; otherwise, re-displays the form with validation errors.
        /// </returns>
        // POST: roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] SwRole role)
        {
            if (!ModelState.IsValid) return View(role);

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(role);
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Renders a form for editing an existing role.
        /// </summary>
        /// <param name="id">The unique identifier of the role to edit.</param>
        /// <returns>
        /// A <see cref="ViewResult"/> with the <see cref="SwRole"/> to edit; otherwise, a <see cref="NotFoundResult"/>.
        /// </returns>
        // GET: roles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();
            return View(role);
        }

        /// <summary>
        /// Processes updates to an existing role's properties.
        /// </summary>
        /// <param name="id">
        /// The identifier of the role being edited.
        /// </param>
        /// <param name="model">
        /// The <see cref="SwRole"/> model containing the updated <c>Name</c> and the original <c>Id</c>.
        /// </param>
        /// <returns>
        /// Redirects to <see cref="Index"/> on success; otherwise, re-displays the edit form with errors.
        /// </returns>
        // POST: roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] SwRole model)
        {
            if (id != model.Id) return NotFound();

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays a confirmation view for deleting a role.
        /// </summary>
        /// <param name="id">The identifier of the role to delete.</param>
        /// <returns>
        /// A <see cref="ViewResult"/> if the role exists; otherwise, a <see cref="NotFoundResult"/>.
        /// </returns>
        // GET: roles/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();
            return View(role);
        }

        /// <summary>
        /// Deletes the specified role after confirmation.
        /// </summary>
        /// <param name="id">The identifier of the role to delete.</param>
        /// <returns>A redirect to the <see cref="Index"/> action.</returns>
        // POST: roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role != null)
                await _roleManager.DeleteAsync(role);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Determines whether a role with the given identifier exists.
        /// </summary>
        /// <param name="id">The role's identifier.</param>
        /// <returns>True if the role exists; otherwise, false.</returns>
        private bool SwRoleExists(int id)
        {
            // Use RoleManager.Roles to check existence by ID
            return _roleManager.Roles.Any(r => r.Id == id);
        }

        // GET: roles/ManageUsers/5
        public async Task<IActionResult> ManageUsers(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound();

            var users = await _userManager.Users.AsNoTracking().ToListAsync();

            var vm = new EditRoleUsersVM
            {
                RoleId = role.Id,
                RoleName = role.Name ?? $"Role {role.Id}",
                Users = new List<UserChoiceVM>()
            };

            foreach (var u in users)
            {
                var inRole = await _userManager.IsInRoleAsync(u, role.Name!);
                vm.Users.Add(new UserChoiceVM
                {
                    UserId = u.Id,
                    DisplayName = string.IsNullOrWhiteSpace(u.FirstName) && string.IsNullOrWhiteSpace(u.LastName)
                                  ? (u.Email ?? u.UserName ?? $"User {u.Id}")
                                  : $"{u.FirstName} {u.LastName}".Trim(),
                    Email = u.Email ?? "",
                    Selected = inRole
                });
            }

            vm.Users = vm.Users.OrderBy(x => x.DisplayName).ToList();
            return View(vm);
        }

        // POST: roles/ManageUsers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUsers(int id, EditRoleUsersVM model)
        {
            if (id != model.RoleId) return NotFound();

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null || string.IsNullOrWhiteSpace(role.Name)) return NotFound();

            // Build desired + current sets
            var desiredUserIds = model.Users.Where(x => x.Selected).Select(x => x.UserId).ToHashSet();
            var allUsers = await _userManager.Users.ToListAsync();

            foreach (var u in allUsers)
            {
                var currentlyInRole = await _userManager.IsInRoleAsync(u, role.Name);
                var shouldBeInRole = desiredUserIds.Contains(u.Id);

                if (!currentlyInRole && shouldBeInRole)
                {
                    var addRes = await _userManager.AddToRoleAsync(u, role.Name);
                    if (!addRes.Succeeded)
                        foreach (var e in addRes.Errors) ModelState.AddModelError("", e.Description);
                }
                else if (currentlyInRole && !shouldBeInRole)
                {
                    var remRes = await _userManager.RemoveFromRoleAsync(u, role.Name);
                    if (!remRes.Succeeded)
                        foreach (var e in remRes.Errors) ModelState.AddModelError("", e.Description);
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            return RedirectToAction(nameof(Index));
        }
    }
}
