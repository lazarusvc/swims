# Authorization & Policies

*Requires `Admin.Policies`, `Admin.Endpoints`, and/or `Admin.PublicEndpoints` permissions.*

This section covers the advanced authorization controls in SWIMS: dynamic policies, endpoint policy assignments, public endpoints, and the route inspector.

## Authorization Policies

Navigate to **Admin → Authorization → Policies** (`Admin.Policies`).

Authorization policies define the rules that must be met to access specific features. Most policies are automatically managed by the roles and permissions system. This screen allows you to create or override policies at a finer grain — for example, requiring a specific claim in addition to a role.

**Typical use:** Creating a custom policy for a special workflow or integration requirement. For standard user access control, use [Roles](roles.md) instead.

## Endpoint Policies

Navigate to **Admin → Authorization → Endpoint Policies** (`Admin.Endpoints`).

Endpoint policies map specific pages or actions to named authorization policies — without requiring code changes. This lets you restrict (or change the requirements for) any route in the application from the Admin UI.

### Bulk Create

Use **Bulk Create** to assign the same policy to multiple routes at once — paste a list of `controller/action` pairs and choose the policy to apply.

## Public Endpoints

Navigate to **Admin → Authorization → Public Endpoints** (`Admin.PublicEndpoints`).

Public Endpoints are routes that are accessible **without logging in**. By default, all SWIMS routes require authentication. Adding a route here makes it publicly accessible.

> [!WARNING]
> Be extremely careful when adding Public Endpoints. Making the wrong route public could expose sensitive data. Only add routes that are intentionally unauthenticated (e.g. a public status page or the setup wizard).

## Route Inspector

Navigate to **Admin → Authorization → Route Inspector** (`Admin.RouteInspector`).

The Route Inspector shows a complete list of all registered routes in the application with their HTTP methods and any assigned endpoint policies. Use this to audit which routes have custom policies and which rely on attribute-based defaults.
