# Managing Roles

*Requires `Admin.Roles` permission.*

## What Is a Role?

A role is a named collection of permissions. Users are assigned roles, and their access to SWIMS features is determined by the permissions those roles include.

For example, a "CaseWorker" role might include permissions to view and manage cases and beneficiaries, but not to manage users or configure the system.

## Role List

Navigate to **Admin → Roles** to see all roles. Each row shows the role name and the number of permissions assigned.

## Creating a Role

1. Click **New Role**.
2. Enter a **Role Name** — use something descriptive, e.g. `SeniorCaseWorker`, `RegionalCoordinator`.
3. Click **Save**.
4. You are taken to the role's edit screen to assign permissions.

## Assigning Permissions to a Role

1. Open a role from the list (click its name).
2. The edit screen shows a checklist of all available permissions, grouped by module.
3. Check or uncheck permissions as needed.
4. Click **Save**.

Permission changes take effect for all users assigned this role on their **next login**.

## Available Permissions (Summary)

| Group | Key Permissions |
|-------|----------------|
| **Admin** | Users, Roles, Settings, Policies, Endpoints, Hangfire, API Dashboard, Session Logs, Audit Logs, Notification Routing |
| **Programmes & Forms** | Programs.View, Forms.Manage, Forms.Builder, Forms.Submit, Forms.Access |
| **Intake** | Intake.View, Intake.Create, Intake.Edit, Intake.Assign |
| **Clients (Beneficiaries)** | Clients.View, Clients.Create, Clients.Edit, Clients.Archive |
| **Applications** | Applications.View, Applications.Edit, Applications.Assign |
| **Cases** | Cases.View, Cases.Manage |
| **Approvals** | Approvals.View, Approvals.Level1 through Level5 |
| **Payments** | Payments.View, Payments.Validate, Payments.ExportLists, Payments.Reconcile |
| **Reference Data** | RefData.Manage |
| **Stored Procedures** | SP.Run, SP.Manage, SP.Params |
| **Reporting** | Reports.View, Reports.Admin |
| **API** | Api.Access |

## Default Roles

SWIMS seeds a set of default roles on first setup:

| Role | Typical Permissions |
|------|-------------------|
| **Admin** | All permissions |
| **ProgramManager** | Programs.View, Forms.Manage, Forms.Builder, RefData.Manage, Reports.Admin |
| **CaseWorker** | Intake.*, Clients.*, Cases.*, Forms.Submit, Forms.Access, Approvals.View, Approvals.Level1 |
| **Approver** | Approvals.View, Approvals.Level1–5 (assign relevant levels only) |
| **ReportsUser** | Reports.View |
| **Viewer** | Programs.View, Clients.View, Cases.View, Reports.View |

These defaults can be modified freely after initial setup.

## Deleting a Role

Roles with users assigned to them cannot be deleted. First remove all users from the role, then delete it.
