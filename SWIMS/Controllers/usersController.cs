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
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;
using SWIMS.Services.Notifications;
using SWIMS.Services.Diagnostics.Auditing;


namespace SWIMS.Controllers
{
    public class usersController : Controller
    {
        private readonly UserManager<SwUser> _userManager;
        private readonly RoleManager<SwRole> _roleManager;
        private readonly IElsaWorkflowQueue _elsaQueue;
        private readonly IAuditLogger _audit;

        public usersController(
            UserManager<SwUser> userManager,
            RoleManager<SwRole> roleManager,
            IElsaWorkflowQueue elsaQueue,
            IAuditLogger audit)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _elsaQueue = elsaQueue;
            _audit = audit;
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

            // 🔔 Notify: Admin created user account
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Identity.Users.Created,
                subject: "User account created",
                body: $"User '{(swUser.Email ?? swUser.UserName ?? $"ID {swUser.Id}")}' was created.",
                userId: swUser.Id,
                userEmail: swUser.Email,
                url: Url.Action(nameof(Details), new { id = swUser.Id }),
                texts: new
                {
                    actor = new
                    {
                        subject = "User account created",
                        body = $"You created user '{(swUser.Email ?? swUser.UserName ?? $"ID {swUser.Id}")}'."
                    },
                    routed = new
                    {
                        subject = "User account created",
                        body = $"{actorName} created user '{(swUser.Email ?? swUser.UserName ?? $"ID {swUser.Id}")}'."
                    },
                    superadmin = new
                    {
                        subject = "User account created",
                        body = $"{actorName} created user '{(swUser.Email ?? swUser.UserName ?? $"ID {swUser.Id}")}'."
                    }
                },
                extraMeta: new { role },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


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

            // 🔔 Notify: Admin updated user account
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Identity.Users.Updated,
                subject: "User account updated",
                body: $"User '{(user.Email ?? user.UserName ?? $"ID {user.Id}")}' was updated.",
                userId: user.Id,
                userEmail: user.Email,
                url: Url.Action(nameof(Details), new { id = user.Id }),
                texts: new
                {
                    actor = new
                    {
                        subject = "User account updated",
                        body = $"You updated user '{(user.Email ?? user.UserName ?? $"ID {user.Id}")}'."
                    },
                    routed = new
                    {
                        subject = "User account updated",
                        body = $"{actorName} updated user '{(user.Email ?? user.UserName ?? $"ID {user.Id}")}'."
                    },
                    superadmin = new
                    {
                        subject = "User account updated",
                        body = $"{actorName} updated user '{(user.Email ?? user.UserName ?? $"ID {user.Id}")}'."
                    }
                },
                extraMeta: new { role },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


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
            {
                var userId = user.Id;
                var userEmail = user.Email;
                var userName = user.UserName;

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    // 🔔 Notify: Admin deleted user account
                    var actorName = User?.Identity?.Name ?? "Someone";

                    await NotifyAdminAsync(
                        eventKey: SwimsEventKeys.Identity.Users.Deleted,
                        subject: "User account deleted",
                        body: $"User '{(userEmail ?? userName ?? $"ID {userId}")}' was deleted.",
                        userId: userId,
                        userEmail: userEmail,
                        url: Url.Action(nameof(Index)),
                        texts: new
                        {
                            actor = new
                            {
                                subject = "User account deleted",
                                body = $"You deleted user '{(userEmail ?? userName ?? $"ID {userId}")}'."
                            },
                            routed = new
                            {
                                subject = "User account deleted",
                                body = $"{actorName} deleted user '{(userEmail ?? userName ?? $"ID {userId}")}'."
                            },
                            superadmin = new
                            {
                                subject = "User account deleted",
                                body = $"{actorName} deleted user '{(userEmail ?? userName ?? $"ID {userId}")}'."
                            }
                        },
                        ct: HttpContext.RequestAborted);

                    // 🔔 Notify: END

                }
            }

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

            // 📝 Audit: User roles updated (P0 launch MUST)
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "UserRolesUpdated",
                entity: "User",
                entityId: user.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: new { roles = currentRoleNames },
                newObj: new { roles = desiredRoleNames },
                extra: new
                {
                    targetUserId = user.Id,
                    addedRoles = toAdd,
                    removedRoles = toRemove
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END


            // 🔔 Notify: Admin updated user roles
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Identity.Users.RolesUpdated,
                subject: "User roles updated",
                body: $"Roles for user '{(user.Email ?? user.UserName ?? $"ID {user.Id}")}' were updated.",
                userId: user.Id,
                userEmail: user.Email,
                url: Url.Action(nameof(ManageRoles), new { id = user.Id }),
                texts: new
                {
                    actor = new
                    {
                        subject = "User roles updated",
                        body = $"You updated roles for user '{(user.Email ?? user.UserName ?? $"ID {user.Id}")}'."
                    },
                    routed = new
                    {
                        subject = "User roles updated",
                        body = $"{actorName} updated roles for user '{(user.Email ?? user.UserName ?? $"ID {user.Id}")}'."
                    },
                    superadmin = new
                    {
                        subject = "User roles updated",
                        body = $"{actorName} updated roles for user '{(user.Email ?? user.UserName ?? $"ID {user.Id}")}'."
                    }
                },
                extraMeta: new { addedRoles = toAdd, removedRoles = toRemove },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


            return RedirectToAction(nameof(Index));
        }


        // --------------------------------------------------------------------
        // Generic admin notification helper for user management actions.
        // --------------------------------------------------------------------
        private async Task NotifyAdminAsync(
    string eventKey,
    string subject,
    string body,
    int? userId = null,
    string? userEmail = null,
    string? url = null,
    object? texts = null,
    object? extraMeta = null,
    CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipient = userIdClaim ?? User.Identity?.Name;

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
                        userId,
                        userEmail,
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
                // Swallow failures so admin UX is never blocked by Elsa issues.
            }
        }



    }
}