using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Areas.Admin.ViewModels.Notifications;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using SWIMS.Security;
using System.Reflection;

namespace SWIMS.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = Permissions.Admin_NotificationsRouting)]
public class NotificationRoutingController : Controller
{
    private readonly SwimsIdentityDbContext _db;

    public NotificationRoutingController(SwimsIdentityDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var routes = await _db.NotificationRoutes
            .Include(r => r.Roles)
            .Include(r => r.Permissions)
            .Include(r => r.Users)
            .AsNoTracking()
            .OrderBy(r => r.EventKey)
            .ToListAsync(ct);

        return View(routes);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = await BuildEditViewModelAsync(null, ct);
        return View("Edit", vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var vm = await BuildEditViewModelAsync(id, ct);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(NotificationRouteEditViewModel model, CancellationToken ct)
    {
        // Validate Type from NotificationTypes constants (prevents manual tampering).
        var allowedTypes = BuildNotificationTypeOptions();
        var requestedType = (model.Type ?? NotificationTypes.System).Trim();

        if (!allowedTypes.Contains(requestedType, StringComparer.Ordinal))
        {
            ModelState.AddModelError(nameof(model.Type), "Invalid route type.");
        }

        if (!ModelState.IsValid)
        {
            await RehydrateListsAsync(model, ct);
            return View("Edit", model);
        }

        var now = DateTimeOffset.UtcNow;

        NotificationRoute route;

        if (model.Id.HasValue)
        {
            route = await _db.NotificationRoutes
                .Include(r => r.Roles)
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == model.Id.Value, ct);

            if (route == null) return NotFound();
        }
        else
        {
            route = new NotificationRoute();
            _db.NotificationRoutes.Add(route);
        }

        route.EventKey = (model.EventKey ?? "").Trim();
        route.Type = requestedType;
        route.IsEnabled = model.IsEnabled;
        route.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        route.UpdatedAtUtc = now;

        // Replace lists (MMVP)
        route.Roles.Clear();
        route.Permissions.Clear();
        route.Users.Clear();

        // Roles
        if (model.SelectedRoleIds?.Count > 0)
        {
            var roles = await _db.Roles
                .Where(r => model.SelectedRoleIds.Contains(r.Id))
                .Select(r => new { r.Id, r.Name })
                .ToListAsync(ct);

            foreach (var r in roles)
            {
                route.Roles.Add(new NotificationRouteRole
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? ""
                });
            }
        }

        // Permissions
        if (model.SelectedPermissionKeys?.Count > 0)
        {
            var allPerms = BuildPermissionOptions();
            var permMap = allPerms.ToDictionary(x => x.Key, x => x.Name);

            foreach (var key in model.SelectedPermissionKeys
                         .Where(k => !string.IsNullOrWhiteSpace(k))
                         .Select(k => k.Trim())
                         .Distinct(StringComparer.Ordinal))
            {
                route.Permissions.Add(new NotificationRoutePermission
                {
                    PermissionKey = key,
                    PermissionNameSnapshot = permMap.TryGetValue(key, out var n) ? n : null
                });
            }
        }

        // Users
        if (model.SelectedUserIds?.Count > 0)
        {
            var users = await _db.Users
                .Where(u => model.SelectedUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync(ct);

            foreach (var u in users)
            {
                route.Users.Add(new NotificationRouteUser
                {
                    UserId = u.Id,
                    UserNameSnapshot = u.UserName,
                    EmailSnapshot = u.Email
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        TempData["Success"] = "Notification route saved.";
        return RedirectToAction(nameof(Index));
    }

    // Ajax picker for user search
    [HttpGet]
    public async Task<IActionResult> UserSearch(string q, CancellationToken ct)
    {
        q = (q ?? "").Trim();
        if (q.Length < 2) return Json(Array.Empty<object>());

        var items = await _db.Users
            .AsNoTracking()
            .Where(u =>
                (u.UserName != null && u.UserName.Contains(q)) ||
                (u.Email != null && u.Email.Contains(q)))
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                id = u.Id,
                label = (u.UserName ?? $"user:{u.Id}") + (u.Email != null ? $" — {u.Email}" : "")
            })
            .Take(10)
            .ToListAsync(ct);

        return Json(items);
    }

    private async Task<NotificationRouteEditViewModel?> BuildEditViewModelAsync(int? id, CancellationToken ct)
    {
        NotificationRoute? route = null;

        if (id.HasValue)
        {
            route = await _db.NotificationRoutes
                .Include(r => r.Roles)
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id.Value, ct);

            if (route == null) return null;
        }

        var allRoles = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleOptionViewModel { Id = r.Id, Name = r.Name ?? "" })
            .ToListAsync(ct);

        var allPermissions = BuildPermissionOptions();
        var allTypes = BuildNotificationTypeOptions();

        var selectedType = (route?.Type ?? NotificationTypes.System).Trim();
        if (!allTypes.Contains(selectedType, StringComparer.Ordinal))
            selectedType = NotificationTypes.System;

        var selectedUserIds = route?.Users.Select(u => u.UserId).Distinct().ToList() ?? new List<int>();
        var selectedUsers = new List<UserOptionViewModel>();

        if (selectedUserIds.Count > 0)
        {
            selectedUsers = await _db.Users
                .AsNoTracking()
                .Where(u => selectedUserIds.Contains(u.Id))
                .Select(u => new UserOptionViewModel
                {
                    Id = u.Id,
                    Label = (u.UserName ?? $"user:{u.Id}") + (u.Email != null ? $" — {u.Email}" : "")
                })
                .ToListAsync(ct);
        }

        return new NotificationRouteEditViewModel
        {
            Id = route?.Id,
            EventKey = route?.EventKey ?? "",
            Type = selectedType,
            AllTypes = allTypes,

            IsEnabled = route?.IsEnabled ?? true,
            Description = route?.Description,

            AllRoles = allRoles,
            SelectedRoleIds = route?.Roles.Select(r => r.RoleId).Distinct().ToList() ?? new List<int>(),

            AllPermissions = allPermissions,
            SelectedPermissionKeys = route?.Permissions.Select(p => p.PermissionKey).Distinct().ToList() ?? new List<string>(),

            SelectedUserIds = selectedUserIds,
            SelectedUsers = selectedUsers
        };
    }

    private async Task RehydrateListsAsync(NotificationRouteEditViewModel vm, CancellationToken ct)
    {
        vm.AllRoles = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleOptionViewModel { Id = r.Id, Name = r.Name ?? "" })
            .ToListAsync(ct);

        vm.AllPermissions = BuildPermissionOptions();
        vm.AllTypes = BuildNotificationTypeOptions();

        if (vm.SelectedUserIds?.Count > 0)
        {
            vm.SelectedUsers = await _db.Users
                .AsNoTracking()
                .Where(u => vm.SelectedUserIds.Contains(u.Id))
                .Select(u => new UserOptionViewModel
                {
                    Id = u.Id,
                    Label = (u.UserName ?? $"user:{u.Id}") + (u.Email != null ? $" — {u.Email}" : "")
                })
                .ToListAsync(ct);
        }
        else
        {
            vm.SelectedUsers = new List<UserOptionViewModel>();
        }

        // Normalize Type to a valid option (prevents blanking out on failed validation)
        var t = (vm.Type ?? NotificationTypes.System).Trim();
        if (!vm.AllTypes.Contains(t, StringComparer.Ordinal))
            vm.Type = NotificationTypes.System;
        else
            vm.Type = t;
    }

    private static List<PermissionOptionViewModel> BuildPermissionOptions()
    {
        // Reflect public const string fields on SWIMS.Security.Permissions
        var fields = typeof(Permissions).GetFields(BindingFlags.Public | BindingFlags.Static);

        var list = new List<PermissionOptionViewModel>();

        foreach (var f in fields)
        {
            if (f.FieldType != typeof(string)) continue;
            if (!f.IsLiteral || f.IsInitOnly) continue;

            var key = f.GetRawConstantValue() as string;
            if (string.IsNullOrWhiteSpace(key)) continue;

            list.Add(new PermissionOptionViewModel
            {
                Key = key,
                Name = f.Name
            });
        }

        return list
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> BuildNotificationTypeOptions()
    {
        // Reflect public const string fields on SWIMS.Models.Notifications.NotificationTypes
        var fields = typeof(NotificationTypes).GetFields(BindingFlags.Public | BindingFlags.Static);

        var list = new List<string>();

        foreach (var f in fields)
        {
            if (f.FieldType != typeof(string)) continue;
            if (!f.IsLiteral || f.IsInitOnly) continue;

            var v = f.GetRawConstantValue() as string;
            if (string.IsNullOrWhiteSpace(v)) continue;

            list.Add(v);
        }

        return list
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }
}