# Administration — Overview

*The Administration section is for users with Admin-level permissions. Standard users will not see this section.*

The Admin area provides tools to configure and monitor the SWIMS system. Each sub-section requires its own specific permission.

## Admin Sections

| Section | Permission | Purpose |
|---------|-----------|---------|
| [Users](users.md) | `Admin.Users` | Create, edit, and manage user accounts |
| [Roles](roles.md) | `Admin.Roles` | Create roles and assign permission sets |
| [System Settings](system-settings.md) | `Admin.Settings` | Application branding, support contacts, feature flags |
| [Notification Routing](notification-routing.md) | `Admin.NotificationsRouting` | Control which channels fire for each notification type |
| [Authorization & Policies](authorization.md) | `Admin.Policies`, `Admin.Endpoints`, `Admin.PublicEndpoints` | Manage dynamic access control policies |
| [Audit & Session Logs](logs.md) | `Admin.AuditLogs`, `Admin.SessionLogs` | Review system activity and user sessions |
| [Hangfire Dashboard](hangfire.md) | `Admin.Hangfire` | Monitor background jobs |
| [API Dashboard](api-dashboard.md) | `Admin.ApiDashboard` | View live API endpoint inventory and health |

## Accessing the Admin Area

Admin sections are accessible from the sidebar under the **Admin** group, or by navigating directly to the relevant URL. Only sections your account has permission for will be visible.
