# Authorization — Overview

SWIMS uses a layered authorization system built on ASP.NET Core Authorization with several custom extensions:

1. **RBAC via Permissions** — static permission constants assigned to roles, checked with `[Authorize(Policy="...")]`
2. **Dynamic DB-Backed Policies** — policies defined in the database, loaded at runtime by `DbAuthorizationPolicyProvider`
3. **Endpoint Policy Assignments** — maps specific controller/action routes to named policies via `DbEndpointPolicyFilter`
4. **Public Endpoints** — routes that bypass authentication entirely, managed via the Admin UI
5. **Fallback Policy** — `PublicOrAuthenticatedRequirement` ensures every request is either authenticated or explicitly marked public

## Permission Constants

All permission strings are defined in `Security/Permissions.cs` as `public const string` values:

```csharp
public static class Permissions
{
    // Admin
    public const string Admin_Users          = "Admin.Users";
    public const string Admin_Roles          = "Admin.Roles";
    public const string Admin_Settings       = "Admin.Settings";
    public const string Admin_Policies       = "Admin.Policies";
    public const string Admin_Endpoints      = "Admin.Endpoints";
    public const string Admin_PublicEndpoints= "Admin.PublicEndpoints";
    public const string Admin_RouteInspector = "Admin.RouteInspector";
    public const string Admin_Hangfire       = "Admin.Hangfire";
    public const string Admin_ApiDashboard   = "Admin.ApiDashboard";
    public const string Admin_SessionLog     = "Admin.SessionLogs";
    public const string Admin_AuditLogs      = "Admin.AuditLogs";
    public const string Admin_NotificationsRouting = "Admin.NotificationsRouting";

    // Forms & Programmes
    public const string Programs_View  = "Programs.View";
    public const string Forms_Manage   = "Forms.Manage";
    public const string Forms_Builder  = "Forms.Builder";
    public const string Forms_Submit   = "Forms.Submit";
    public const string Forms_Access   = "Forms.Access";

    // Intake / Applications / Assessment
    public const string Intake_View   = "Intake.View";
    public const string Intake_Create = "Intake.Create";
    public const string Intake_Edit   = "Intake.Edit";
    public const string Intake_Assign = "Intake.Assign";
    public const string Applications_View   = "Applications.View";
    public const string Applications_Edit   = "Applications.Edit";
    public const string Applications_Assign = "Applications.Assign";
    public const string Assessment_View = "Assessment.View";

    // Clients (Beneficiaries)
    public const string Clients_View    = "Clients.View";
    public const string Clients_Create  = "Clients.Create";
    public const string Clients_Edit    = "Clients.Edit";
    public const string Clients_Archive = "Clients.Archive";

    // Cases
    public const string Cases_View   = "Cases.View";
    public const string Cases_Manage = "Cases.Manage";

    // Approvals
    public const string Approvals_View = "Approvals.View";
    public const string Approvals_L1   = "Approvals.Level1";  // Social Worker
    public const string Approvals_L2   = "Approvals.Level2";  // Coordinator
    public const string Approvals_L3   = "Approvals.Level3";  // Director
    public const string Approvals_L4   = "Approvals.Level4";  // Permanent Secretary
    public const string Approvals_L5   = "Approvals.Level5";  // Minister

    // Payments
    public const string Payments_View        = "Payments.View";
    public const string Payments_Validate    = "Payments.Validate";
    public const string Payments_ExportLists = "Payments.ExportLists";
    public const string Payments_Reconcile   = "Payments.Reconcile";

    // Reference Data
    public const string RefData_Manage = "RefData.Manage";

    // Stored Procedures
    public const string SP_Run    = "SP.Run";
    public const string SP_Manage = "SP.Manage";
    public const string SP_Params = "SP.Params";

    // Reporting
    public const string Reports_View  = "Reports.View";
    public const string Reports_Admin = "Reports.Admin";

    // API
    public const string Api_Access = "Api.Access";
}
```

## How Permissions Are Enforced

Permissions are used as ASP.NET Core **policy names** on controllers/actions:

```csharp
[Authorize(Policy = Permissions.Cases_Manage)]
public IActionResult Create() { ... }
```

The `DbAuthorizationPolicyProvider` intercepts `GetPolicyAsync(policyName)`. If the policy name is a recognized `Permissions.*` constant, it builds a policy that requires the user to have a **claim** of type `permission` with that value. Roles get these claims during identity construction.

## Role Architecture

Roles are stored in `auth.AspNetRoles`. Each role has a set of permission claims (`auth.AspNetRoleClaims`) of type `"permission"` with values matching the `Permissions.*` constants.

When a user signs in, their role claims (including permission claims from all their roles) are loaded into the identity. Authorization checks then simply look for the required claim.

## Fallback Policy

A global fallback policy is registered:

```csharp
options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .AddRequirements(new PublicOrAuthenticatedRequirement())
    .Build();
```

`PublicOrAuthenticatedHandler` evaluates this requirement by:
1. If the route is in the `PublicEndpoints` database table → **allow**.
2. If the user is authenticated → **allow**.
3. Otherwise → **deny** (redirect to login).

This means all routes are secured by default — you never need `[Authorize]` everywhere, and you don't accidentally expose new endpoints without intending to.

## Related Pages

- [RBAC & Permissions](rbac.md)
- [Dynamic Policies](dynamic-policies.md)
- [Endpoint Policies](endpoint-policies.md)
