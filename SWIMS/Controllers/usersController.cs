// -------------------------------------------------------------------
// File:    usersController.cs
// Author:  N/A
// Created: N/A
// Purpose: Provides CRUD operations for users and manages their role assignments within SWIMS.
//          Only users in the "Admin" role may execute these actions.
// Dependencies:
//   - SwUser (ASP.NET Core Identity user entity)
//   - SwRole (ASP.NET Core Identity role entity)
//   - UserManager<SwUser>, RoleManager<SwRole> (Identity services)
//   - Microsoft.AspNetCore.Mvc, Authorization, Identity, Mvc.Rendering
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace SWIMS.Controllers
{
    /// <summary>
    /// Controller for listing, creating, editing, and deleting <see cref="SwUser"/> entities,
    /// including assigning and managing roles. Secured to <c>Admin</c> users only.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class usersController : Controller
    {
        private readonly UserManager<SwUser> _userManager;
        private readonly RoleManager<SwRole> _roleManager;

        /// <summary>
        /// Constructs a <see cref="usersController"/> with the specified identity services.
        /// </summary>
        /// <param name="userManager">
        /// The <see cref="UserManager{SwUser}"/> for user account management.
        /// </param>
        /// <param name="roleManager">
        /// The <see cref="RoleManager{SwRole}"/> for role lookup and assignment.
        /// </param>
        public usersController(UserManager<SwUser> userManager,
                               RoleManager<SwRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Displays all users with their current roles.
        /// </summary>
        /// <returns>
        /// A <see cref="ViewResult"/> containing a list of <see cref="UserWithRolesViewModel"/>.
        /// </returns>
        // GET: users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var tasks = users.Select(async u => new UserWithRolesViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                Email = u.Email,
                Roles = await _userManager.GetRolesAsync(u)
            });

            var list = await Task.WhenAll(tasks);
            return View(list);
        }

        /// <summary>
        /// Displays details for a specific user.
        /// </summary>
        /// <param name="id">The identifier of the user to display.</param>
        /// <returns>
        /// A <see cref="ViewResult"/> with a <see cref="UserWithRolesViewModel"/> if found;
        /// otherwise, a <see cref="NotFoundResult"/>.
        /// </returns>
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

        /// <summary>
        /// Renders a form for creating a new user.
        /// </summary>
        /// <returns>
        /// A <see cref="ViewResult"/> with <c>ViewBag.Roles</c> populated for role selection.
        /// </returns>
        // GET: users/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name));
            return View();
        }

        /// <summary>
        /// Processes the creation of a new user and assigns an initial role if specified.
        /// </summary>
        /// <param name="swUser">
        /// A <see cref="SwUser"/> model with <c>FirstName</c>, <c>LastName</c>, <c>UserName</c>, and <c>Email</c> set.
        /// </param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="role">The optional role name to assign upon creation.</param>
        /// <returns>
        /// Redirects to <see cref="Index"/> on success; otherwise re-displays the create form with errors.
        /// </returns>
        // POST: users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([
            Bind("FirstName,LastName,UserName,Email")] SwUser swUser,
            string password,
            string role)
        {
            if (!ModelState.IsValid) return View(swUser);

            // Mark the account as confirmed so admin-created users can sign in immediately
            swUser.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(swUser, password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name), role);
                return View(swUser);
            }

            if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
                await _userManager.AddToRoleAsync(swUser, role);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Renders a form for editing an existing user and their role.
        /// </summary>
        /// <param name="id">The identifier of the user to edit.</param>
        /// <returns>
        /// A <see cref="ViewResult"/> with the <see cref="SwUser"/> and <c>ViewBag.Roles</c> set;
        /// otherwise <see cref="NotFoundResult"/>.
        /// </returns>
        // GET: users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            ViewBag.Roles = new SelectList(
                _roleManager.Roles.Select(r => r.Name),
                (await _userManager.GetRolesAsync(user)).FirstOrDefault());
            return View(user);
        }

        /// <summary>
        /// Processes updates to an existing user's properties and role assignment.
        /// </summary>
        /// <param name="id">The identifier of the user being edited.</param>
        /// <param name="swUser">
        /// A <see cref="SwUser"/> with updated properties (<c>FirstName</c>, <c>LastName</c>, <c>UserName</c>, <c>Email</c>).
        /// </param>
        /// <param name="role">The new role to assign (or empty to remove roles).</param>
        /// <returns>
        /// Redirects to <see cref="Index"/> on success; otherwise re-displays the edit form with errors.
        /// </returns>
        // POST: users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("FirstName,LastName,UserName,Email")] SwUser swUser,
            string role)
        {
            if (!ModelState.IsValid) return View(swUser);

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
                ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name), role);
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

        /// <summary>
        /// Displays a confirmation view for deleting a user.
        /// </summary>
        /// <param name="id">The identifier of the user to delete.</param>
        /// <returns>
        /// A <see cref="ViewResult"/> with the user's details if found;
        /// otherwise <see cref="NotFoundResult"/>.
        /// </returns>
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

        /// <summary>
        /// Deletes the specified user after confirmation.
        /// </summary>
        /// <param name="id">The identifier of the user to delete.</param>
        /// <returns>Redirects to <see cref="Index"/>.</returns>
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
    }
}
