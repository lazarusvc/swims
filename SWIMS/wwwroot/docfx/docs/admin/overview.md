# Admin & Settings — Overview

The Admin area (`Areas/Admin`) provides system administration capabilities. Access to each section is gated by its own permission constant.

## Admin Sections

| Section | Route | Permission | Purpose |
|---------|-------|-----------|---------|
| Users | `/identityController` (main controllers) | `Admin.Users` | User accounts management |
| Roles | `/rolesController` | `Admin.Roles` | Role management and permission assignment |
| System Settings | `/Admin/SystemSettings` | `Admin.Settings` | Global application settings |
| Authorization Policies | `/Admin/AuthorizationPolicies` | `Admin.Policies` | DB-backed policy management |
| Endpoint Policies | `/Admin/EndpointPolicies` | `Admin.Endpoints` | Route → policy assignments |
| Public Endpoints | `/Admin/PublicEndpoints` | `Admin.PublicEndpoints` | Routes exempt from auth |
| Route Inspector | `/Admin/RouteInspector` | `Admin.RouteInspector` | View all registered routes |
| Notification Routing | `/Admin/NotificationRouting` | `Admin.NotificationsRouting` | Per-type notification channel config |
| Report Definitions | `/Admin/ReportsAdmin` | `Reports.Admin` | SSRS report registry |
| Report Params | `/Admin/ReportParams` | `Reports.Admin` | Report parameter definitions |
| API Dashboard | `/Admin/Api/Dashboard` | `Admin.ApiDashboard` | Live API endpoint inventory |
| Hangfire | `/hangfire` | `Admin.Hangfire` | Background job dashboard |

## Related Pages

- [Users & Roles](users-roles.md)
- [System Settings](system-settings.md)
