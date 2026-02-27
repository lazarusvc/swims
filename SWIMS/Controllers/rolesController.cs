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
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;
using SWIMS.Services.Notifications;
using SWIMS.Services.Diagnostics.Auditing;


namespace SWIMS.Controllers
{
    /// <summary>
    /// Controller for creating, reading, updating, and deleting <see cref="SwRole"/> entities.
    /// Secured so that only users assigned to the "Admin" role may invoke its endpoints.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class rolesController : Controller
    {
        private readonly RoleManager<SwRole> _roleManager;
        private readonly UserManager<SwUser> _userManager;
        private readonly IElsaWorkflowQueue _elsaQueue;
        private readonly IAuditLogger _audit;

        /// <summary>
        /// Initializes a new instance of the rolesController.
        /// </summary>
        /// <param name="roleManager">Identity RoleManager for <see cref="SwRole"/>.</param>
        /// <param name="userManager">Identity UserManager for <see cref="SwUser"/>.</param>
        /// <param name="elsa"></param>
        /// /// <param name="audit"></param>
        public rolesController(
            RoleManager<SwRole> roleManager,
            UserManager<SwUser> userManager,
            IElsaWorkflowQueue elsaQueue,
            IAuditLogger audit)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _elsaQueue = elsaQueue;
            _audit = audit;
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

            // 🔔 Notify: Admin created role
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Identity.Roles.Created,
                subject: "Role created",
                body: $"Role '{role.Name}' was created.",
                roleId: role.Id,
                roleName: role.Name,
                url: Url.Action(nameof(Details), new { id = role.Id }),
                texts: new
                {
                    actor = new
                    {
                        subject = "Role created",
                        body = $"You created role '{role.Name}'."
                    },
                    routed = new
                    {
                        subject = "Role created",
                        body = $"{actorName} created role '{role.Name}'."
                    },
                    superadmin = new
                    {
                        subject = "Role created",
                        body = $"{actorName} created role '{role.Name}'."
                    }
                },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


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

            // 🔔 Notify: Admin updated role
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Identity.Roles.Updated,
                subject: "Role updated",
                body: $"Role '{role.Name}' was updated.",
                roleId: role.Id,
                roleName: role.Name,
                url: Url.Action(nameof(Details), new { id = role.Id }),
                texts: new
                {
                    actor = new
                    {
                        subject = "Role updated",
                        body = $"You updated role '{role.Name}'."
                    },
                    routed = new
                    {
                        subject = "Role updated",
                        body = $"{actorName} updated role '{role.Name}'."
                    },
                    superadmin = new
                    {
                        subject = "Role updated",
                        body = $"{actorName} updated role '{role.Name}'."
                    }
                },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


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
            {
                var roleId = role.Id;
                var roleName = role.Name;

                var result = await _roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    // 🔔 Notify: Admin deleted role
                    var actorName = User?.Identity?.Name ?? "Someone";

                    await NotifyAdminAsync(
                        eventKey: SwimsEventKeys.Identity.Roles.Deleted,
                        subject: "Role deleted",
                        body: $"Role '{roleName ?? $"ID {roleId}"}' was deleted.",
                        roleId: roleId,
                        roleName: roleName,
                        url: Url.Action(nameof(Index)),
                        texts: new
                        {
                            actor = new
                            {
                                subject = "Role deleted",
                                body = $"You deleted role '{roleName ?? $"ID {roleId}"}'."
                            },
                            routed = new
                            {
                                subject = "Role deleted",
                                body = $"{actorName} deleted role '{roleName ?? $"ID {roleId}"}'."
                            },
                            superadmin = new
                            {
                                subject = "Role deleted",
                                body = $"{actorName} deleted role '{roleName ?? $"ID {roleId}"}'."
                            }
                        },
                        ct: HttpContext.RequestAborted);

                    // 🔔 Notify: END

                }
            }

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

            var currentUserIds = new HashSet<int>();
            var addedUserIds = new List<int>();
            var removedUserIds = new List<int>();

            foreach (var u in allUsers)
            {
                var currentlyInRole = await _userManager.IsInRoleAsync(u, role.Name);
                var shouldBeInRole = desiredUserIds.Contains(u.Id);

                // 📝 Audit tracking: current membership snapshot (before changes)
                if (currentlyInRole)
                    currentUserIds.Add(u.Id);

                if (!currentlyInRole && shouldBeInRole)
                {
                    var addRes = await _userManager.AddToRoleAsync(u, role.Name);
                    if (!addRes.Succeeded)
                        foreach (var e in addRes.Errors) ModelState.AddModelError("", e.Description);
                    else
                        addedUserIds.Add(u.Id);
                }
                else if (currentlyInRole && !shouldBeInRole)
                {
                    var remRes = await _userManager.RemoveFromRoleAsync(u, role.Name);
                    if (!remRes.Succeeded)
                        foreach (var e in remRes.Errors) ModelState.AddModelError("", e.Description);
                    else
                        removedUserIds.Add(u.Id);
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            // 📝 Audit: Role membership updated
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "RoleMembershipUpdated",
                entity: "Role",
                entityId: role.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: new { memberCount = currentUserIds.Count },
                newObj: new { memberCount = desiredUserIds.Count },
                extra: new
                {
                    roleId = role.Id,
                    roleName = role.Name,
                    addedUserIds,
                    removedUserIds
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Admin updated role membership
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Identity.Roles.MembershipUpdated,
                subject: "Role membership updated",
                body: $"Membership for role '{role.Name}' was updated.",
                roleId: role.Id,
                roleName: role.Name,
                url: Url.Action(nameof(ManageUsers), new { id = role.Id }),
                texts: new
                {
                    actor = new
                    {
                        subject = "Role membership updated",
                        body = $"You updated membership for role '{role.Name}'."
                    },
                    routed = new
                    {
                        subject = "Role membership updated",
                        body = $"{actorName} updated membership for role '{role.Name}'."
                    },
                    superadmin = new
                    {
                        subject = "Role membership updated",
                        body = $"{actorName} updated membership for role '{role.Name}'."
                    }
                },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------------------------
        // Generic admin notification helper for role management actions.
        // --------------------------------------------------------------------
        private async Task NotifyAdminAsync(
    string eventKey,
    string subject,
    string body,
    int? roleId = null,
    string? roleName = null,
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
                        roleId,
                        roleName,
                        texts,
                        extra = extraMeta
                    }
                })
            };

            try
            {
                // 🔔 Notify: Admin user / role management event
                await _elsaQueue.EnqueueByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
                // Don't block role admin if Elsa is unavailable.
            }
        }



    }
}
