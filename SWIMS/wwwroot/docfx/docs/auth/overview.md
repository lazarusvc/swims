# Authentication — Overview

SWIMS supports two authentication modes through a **unified login form**: local ASP.NET Core Identity accounts and LDAP/Active Directory accounts. Both modes result in a standard Identity cookie — the rest of the application does not distinguish between them after sign-in.

## Login Flow

```
User submits username + password
        │
        ▼
  LdapAuthService.AuthenticateAsync()
    │           │
   Success     Fail
    │           │
    │           ▼
    │     UserManager.FindByEmailAsync / FindByNameAsync
    │     PasswordSignInAsync (BCrypt)
    │
    ▼
  CreateOrUpdateLdapUserAsync()
    └── Finds or creates SwUser.IsLdapUser=true
    └── Syncs AD group memberships → ASP.NET Roles
    └── SignInManager.SignInAsync()
```

### Why a unified form?

The `Login.cshtml` input field accepts both **short AD usernames** (e.g., `jdoe`) and **email addresses / UPNs**. LDAP is attempted first; if it succeeds, the local Identity pathway is bypassed. If LDAP fails or returns nothing, the standard Identity password check runs. This means local users are unaffected by LDAP being unavailable.

## Key Classes

| Class | Location | Role |
|-------|---------|------|
| `LdapAuthService` | `Services/LdapAuthService.cs` | Connects to AD via `System.DirectoryServices.Protocols`, performs bind, queries user attributes and group memberships |
| `ILdapAuthService` | `Services/ILdapAuthService.cs` | Interface (registered as singleton) |
| `SwUser` | `Models/SwUser.cs` | `IdentityUser<int>` with `IsLdapUser`, `LdapUsername`, `LdapDomain` |
| `SwRole` | `Models/SwRole.cs` | `IdentityRole<int>` |
| `CompatibleBcryptHasher` | `Services/CompatibleBcryptHasher.cs` | BCrypt password hasher replacing the default PBKDF2 |
| `BcryptPasswordHasher` | `Services/BcryptPasswordHasher.cs` | Pure BCrypt implementation |
| `IdentityEmailSender` | `Services/Email/IdentityEmailSender.cs` | Adapts `IEmailService` for ASP.NET Identity `IEmailSender` |

## Password Policy

Configured in `Program.cs` via `AddDefaultIdentity`:

| Requirement | Value |
|------------|-------|
| Minimum length | 6 characters |
| Require digit | Yes |
| Require uppercase | Yes |
| Require lowercase | Yes |
| Require non-alphanumeric | **No** |
| Unique email | Required |
| Confirmed account | Required for sign-in |

## Session Management

After sign-in, `SessionCookieEvents` hooks into the application cookie middleware:

- **On sign-in**: writes a row to `ops.session_logs` with IP address, user agent, and login timestamp.
- **On sign-out**: stamps `LogoutAt` on the active session row.

This provides an auditable session history visible in the Admin → Session Log view.

## LDAP Users vs Local Users

| Feature | Local Identity User | LDAP User |
|---------|-------------------|-----------|
| Change Password | ✅ Via Account/Manage | ❌ Blocked (must use AD tools) |
| Change Email | ✅ Via Account/Manage | ❌ Blocked |
| Enable 2FA (TOTP) | ✅ | ❌ Blocked |
| AD group → roles sync | N/A | ✅ On each login |
| `IsLdapUser` flag | `false` | `true` |

The Account management Razor Pages detect `IsLdapUser` and hide or block the irrelevant settings automatically.

## Related Pages

- [LDAP Integration](ldap.md)
- [Two-Factor Authentication](two-factor.md)
