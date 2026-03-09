# LDAP Integration

SWIMS integrates with an Active Directory / LDAP server using `System.DirectoryServices.Protocols` (cross-platform LDAP library). The integration is encapsulated in `LdapAuthService` and registered as a **singleton**.

## Configuration

```json
"Ldap": {
  "Server": "ldap.domain.gov.dm",
  "Port": 389,
  "Domain": "GOV",
  "BaseDn": "DC=gov,DC=dm",
  "UseSSL": false,
  "BindDn": "CN=svc-swims,OU=ServiceAccounts,DC=gov,DC=dm",
  "BindPassword": "<use user-secrets>",
  "UserSearchBase": "OU=Users,DC=gov,DC=dm",
  "GroupSearchBase": "OU=Groups,DC=gov,DC=dm"
}
```

> [!WARNING]
> Never commit `BindPassword` to source control. Use `dotnet user-secrets` in Development and environment variables or Key Vault in Production.

## RootDSE Discovery

`LdapAuthService` supports **RootDSE auto-discovery** to find the domain's default naming context without hard-coding the base DN. When `BaseDn` is empty or `"auto"`, the service queries `rootDSE` on connect:

```
GET rootDSE → defaultNamingContext → use as BaseDn
```

This simplifies configuration in environments where the DC structure may change.

## Authentication Flow

1. The service opens an `LdapConnection` to the configured server.
2. A service account bind (`BindDn` / `BindPassword`) is performed to enable directory searches.
3. The user's account is located by searching for `sAMAccountName=<username>` or `userPrincipalName=<upn>` in `UserSearchBase`.
4. The user's own DN is extracted and a **second bind** is attempted using the user's credentials — this is the actual password verification.
5. On success, group memberships (`memberOf` attribute) are retrieved.
6. Groups are mapped to ASP.NET roles using a configurable name mapping. If no explicit mapping exists, the CN of the AD group is used directly as the role name.

## User Provisioning

On successful LDAP authentication, `CreateOrUpdateLdapUserAsync` is called:

- If no `SwUser` exists with `IsLdapUser=true` and the matching `LdapUsername`, a new local Identity record is created.
- The user's display name and email are synced from AD attributes (`displayName`, `mail`).
- AD group memberships are compared against current ASP.NET role assignments; roles are added or removed to keep them in sync.
- `SignInManager.SignInAsync` is called to issue the application cookie.

This means an LDAP user has both an AD identity and a local `SwUser` record. The `SwUser.Id` (integer) is used throughout SWIMS as the foreign key in notifications, messages, audit logs, etc.

## LDAP Unavailability

If the LDAP server is unreachable:

- The `LdapAuthService.AuthenticateAsync` call fails gracefully and returns `null`.
- The login form falls through to local Identity authentication.
- **LDAP users cannot log in during an outage** — they have no local password unless one was set manually.
- The system logs a warning to Serilog with the connection error details.

## Security Hardening

- Bind credentials are validated at startup (null check on `BindPassword`).
- User search is scoped to `UserSearchBase` — broad directory traversal is not permitted.
- The service is registered as a singleton; it does not hold long-lived connections. Each call opens and disposes an `LdapConnection`.
- SSL (`UseSSL: true`) should be enabled in production environments.
