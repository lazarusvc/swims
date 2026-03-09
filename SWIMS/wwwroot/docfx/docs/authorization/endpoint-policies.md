# Endpoint Policies

**Endpoint Policies** let administrators map specific MVC controller/action routes to named authorization policies through the Admin UI, without modifying code.

## Components

| Component | Location | Purpose |
|-----------|---------|---------|
| `DbEndpointPolicyFilter` | `Services/Auth/DbEndpointPolicyFilter.cs` | MVC action filter — checks endpoint policy assignment per request |
| `IEndpointPolicyAssignmentStore` | `Services/Auth/EndpointPolicyAssignmentStore.cs` | Queries the DB for controller/action → policy assignments |
| `EndpointPoliciesController` | `Areas/Admin/Controllers/EndpointPoliciesController.cs` | Admin CRUD for assignments |
| `PublicEndpointsController` | `Areas/Admin/Controllers/PublicEndpointsController.cs` | Admin CRUD for public endpoint exemptions |

## How Endpoint Policy Assignment Works

`DbEndpointPolicyFilter` is registered as a **global MVC action filter**:

```csharp
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add<DbEndpointPolicyFilter>();
});
```

On every MVC action execution:

1. The filter reads the current `RouteData` to determine `controller` + `action` + `area`.
2. It queries `IEndpointPolicyAssignmentStore` for a matching assignment.
3. If an assignment exists, it calls `IAuthorizationService.AuthorizeAsync(user, policyName)`.
4. If authorization fails, the request is short-circuited with `403 Forbidden`.
5. If no assignment exists, the filter does nothing — standard `[Authorize]` attributes take over.

## Public Endpoints

The `PublicEndpoints` table (`ac` schema) stores routes that are allowed without authentication:

- Routes are matched by controller/action (and optionally area).
- The `PublicOrAuthenticatedHandler` reads `IPublicAccessStore` to determine if the current route is public.
- Public endpoints completely bypass the authentication requirement.

> [!NOTE]
> The SWIMS setup wizard (`/Setup/*`) routes are typically registered as public endpoints so unauthenticated first-run setup is possible.

## Admin UI

### Endpoint Policies

Navigate to **Admin → Authorization → Endpoint Policies**:

- **Bulk Create**: paste a list of `controller/action` pairs and assign a policy to all of them at once.
- **Edit**: change the policy assignment for an existing route.
- **Delete**: remove the assignment (the route falls back to attribute-based authorization).

### Public Endpoints

Navigate to **Admin → Authorization → Public Endpoints**:

- **Create**: register a route as public (no auth required).
- **Edit/Delete**: manage existing public endpoint registrations.

### Route Inspector

Navigate to **Admin → Authorization → Route Inspector** (`Admin.RouteInspector` permission required):

- Displays all registered MVC routes in the application.
- Shows whether each route has an endpoint policy assignment.
- Useful for auditing which routes have custom policies and which rely on attribute-based authorization.

## Match Types

Endpoint policy assignments support flexible matching:

| Match Type | Behaviour |
|-----------|----------|
| Exact | Controller + Action must match exactly |
| Controller-wide | All actions in a controller |
| Area-wide | All controllers/actions in an area |

This allows assigning a policy like `"Admin.Users"` to the entire `UsersController` with a single assignment.
