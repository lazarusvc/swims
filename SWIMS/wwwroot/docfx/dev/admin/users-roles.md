# Users & Roles

## User Management

**Route**: managed via `identityController` and `usersController`  
**Permission**: `Admin.Users`

### User List

Shows all `SwUser` accounts with columns: Username, Email, Active/Locked, Roles, LDAP flag.

### Creating Users

1. Navigate to **Admin → Users → Create**.
2. Enter username, email, and temporary password.
3. Assign roles.
4. Save — user receives a confirmation email if the email service is configured.

For LDAP users, provision is automatic on first login. Manual creation via the UI creates a **local Identity user** (not LDAP-linked).

### Editing Users

From the user detail view:

| Action | Effect |
|--------|--------|
| Edit profile | Update display name, email |
| Reset password | Admin-initiated password reset (sends reset email) |
| Lock / Unlock account | `LockoutEnabled` / `LockoutEnd` |
| Assign / remove roles | Updates `AspNetUserRoles` |
| Reset 2FA | Clears authenticator key, disables 2FA |
| Delete user | Soft-delete or hard-delete (configurable) |

### `UserWithRolesViewModel`

The users list and edit views use `UserWithRolesViewModel`:

```csharp
public class UserWithRolesViewModel
{
    public SwUser User { get; set; }
    public IList<string> Roles { get; set; }
    public bool IsLdapUser { get; set; }
}
```

## Role Management

**Route**: `rolesController`  
**Permission**: `Admin.Roles`

### Role List

Shows all `SwRole` records with their assigned permission counts.

### Creating Roles

1. Navigate to **Admin → Roles → Create**.
2. Enter role name (must be unique, no spaces recommended).
3. Save.

### Assigning Permissions to a Role

1. Navigate to **Admin → Roles → Edit → {role}**.
2. The edit view shows a checklist of all `Permissions.*` constants.
3. Check/uncheck permissions.
4. Save — updates `AspNetRoleClaims` (type = `"permission"`).

Permission changes take effect on each user's **next login** (when the claims are re-loaded into the cookie).

### Default Roles (Seeded)

| Role | Key Permissions |
|------|----------------|
| Admin | All `Admin.*`, all other permissions |
| ProgramManager | `Programs.View`, `Forms.Manage`, `Forms.Builder`, `RefData.Manage`, `Reports.Admin` |
| CaseWorker | `Intake.*`, `Clients.*`, `Cases.*`, `Forms.Submit`, `Forms.Access`, `Approvals.View`, `Approvals.Level1` |
| Approver | `Approvals.View`, `Approvals.Level1` through `Level5` (assign appropriate levels per user) |
| ReportsUser | `Reports.View` |
| Viewer | `Programs.View`, `Clients.View`, `Cases.View`, `Reports.View` |

> [!NOTE]
> Default role names and permission assignments are seeded by `SeedData.cs`. They can be modified via the Admin UI after initial setup. Seeding is idempotent — running it again will add missing defaults but not overwrite customised roles.
