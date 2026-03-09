# Dynamic Policies

SWIMS supports **database-backed authorization policies** via `DbAuthorizationPolicyProvider`. This allows administrators to define and modify authorization policies at runtime without redeploying the application.

## How It Works

ASP.NET Core's `IAuthorizationPolicyProvider` is replaced with `DbAuthorizationPolicyProvider`:

```csharp
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DbAuthorizationPolicyProvider>();
```

When the framework calls `GetPolicyAsync(policyName)`:

1. The provider opens a scope and queries `IPolicyStore` (backed by `EfPolicyStore`).
2. If a matching `AuthPolicy` row exists in the DB and `IsEnabled = true`, it constructs an `AuthorizationPolicy` from the stored specification.
3. If no DB policy is found, it falls through to the default `DefaultAuthorizationPolicyProvider` (which handles hardcoded policies like `"AdminOnly"`).

## Policy Data Model

```csharp
// Models/Security/AuthPolicies.cs
public class AuthPolicy
{
    public int Id { get; set; }
    public string Name { get; set; }       // policy name, matches [Authorize(Policy="...")]
    public bool IsEnabled { get; set; }
    public string SpecJson { get; set; }   // JSON-serialized policy requirements
}
```

The `SpecJson` defines what claims/roles the policy requires. A typical spec:

```json
{
  "requireClaims": [
    { "type": "permission", "value": "Cases.Manage" }
  ]
}
```

## Admin UI

Navigate to **Admin → Authorization Policies** to:

- **List** all policies with enabled/disabled status.
- **Create** a new policy with a name and requirement specification.
- **Edit** an existing policy (change requirements or disable it).
- **Delete** a policy (removes from DB; the policy name becomes unresolvable).

> [!WARNING]
> Disabling or deleting a policy that is actively referenced by `[Authorize(Policy="...")]` attributes will cause those endpoints to fall back to the default policy provider. If no default policy exists for that name, access will be denied with `403 Forbidden`.

## Caching

Policy resolution is **not cached at the provider level** — each `GetPolicyAsync` call queries the database. This ensures real-time policy changes take effect immediately.

For high-traffic environments, consider adding an in-memory cache with a short TTL in `EfPolicyStore.GetAsync()`.

## Permission-Based Policies vs DB Policies

The `Permissions.*` constants (e.g., `"Cases.Manage"`) are also valid DB policy names. When the DB contains a policy row named `"Cases.Manage"`, that definition takes precedence over the built-in claim-based interpretation. This allows overriding the default behaviour — for example, requiring an additional role check on top of the claim.

If no DB row exists for a `Permissions.*` name, `DbAuthorizationPolicyProvider` automatically builds a **claim-based policy** requiring `{ type: "permission", value: policyName }`.
