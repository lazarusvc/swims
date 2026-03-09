# RBAC & Permissions

## Roles

Roles are managed in **Admin → Roles**. Each role is an `SwRole` (extends `IdentityRole<int>`) stored in `auth.AspNetRoles`.

Default roles (seeded by `SeedData`):

| Role | Intended Users |
|------|---------------|
| Admin | System administrators |
| ProgramManager | Programme design and form management |
| CaseWorker | Front-line case intake and management |
| Approver | Multi-level approvals (L1–L5 as needed) |
| Viewer | Read-only access |
| ReportsUser | Access to reporting module |

## Assigning Permissions to Roles

Each role has a set of **permission claims** of type `"permission"`. These are managed via **Admin → Roles → Edit**.

When a permission is added to a role, an `IdentityRoleClaim<int>` row is inserted with:
- `ClaimType = "permission"`
- `ClaimValue = "Admin.Users"` (or whichever permission constant)

Users inherit all permission claims from every role they belong to.

## Assigning Roles to Users

Roles are assigned in **Admin → Users → Edit**. A user can belong to multiple roles. Role changes take effect on the user's **next login** (or if the cookie is refreshed).

## Checking Permissions in Code

### In Controllers

```csharp
[Authorize(Policy = Permissions.Cases_Manage)]
public async Task<IActionResult> Create()
{
    // Only reached if user has "Cases.Manage" permission claim
}
```

### In Razor Views

```html
@if (User.HasClaim("permission", Permissions.Admin_Users))
{
    <a asp-controller="Users" asp-action="Index">Manage Users</a>
}
```

### In Services

```csharp
public class SomeService
{
    private readonly IAuthorizationService _authz;

    public async Task DoSomethingAsync(ClaimsPrincipal user)
    {
        var result = await _authz.AuthorizeAsync(user, Permissions.Cases_Manage);
        if (!result.Succeeded) throw new UnauthorizedAccessException();
    }
}
```

## Sidebar Navigation

The WowDash sidebar is fully **permission-gated**. Each nav item checks whether the current user has the relevant permission claim before rendering. Users only see the modules they have access to.

## Admin-Only Bypass

Two static policies remain as a safety net regardless of DB state:

```csharp
options.AddPolicy("AdminOnly",       p => p.RequireRole("Admin"));
options.AddPolicy("ProgramManager",  p => p.RequireRole("Admin", "ProgramManager"));
```

These can be used with `[Authorize(Policy = "AdminOnly")]` on ultra-sensitive actions that should never depend on DB policy state.
