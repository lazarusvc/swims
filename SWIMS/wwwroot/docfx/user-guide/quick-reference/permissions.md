# Permissions & Roles

Quick reference mapping permissions to the default SWIMS roles.

## Permission → Default Role Mapping

| Permission | Admin | ProgramManager | CaseWorker | Approver | ReportsUser | Viewer |
|-----------|:-----:|:--------------:|:----------:|:--------:|:-----------:|:------:|
| **Admin.Users** | ✅ | | | | | |
| **Admin.Roles** | ✅ | | | | | |
| **Admin.Settings** | ✅ | | | | | |
| **Admin.Policies** | ✅ | | | | | |
| **Admin.Endpoints** | ✅ | | | | | |
| **Admin.Hangfire** | ✅ | | | | | |
| **Admin.ApiDashboard** | ✅ | | | | | |
| **Admin.AuditLogs** | ✅ | | | | | |
| **Admin.SessionLogs** | ✅ | | | | | |
| **Admin.NotificationsRouting** | ✅ | | | | | |
| **Programs.View** | ✅ | ✅ | ✅ | ✅ | | ✅ |
| **Forms.Manage** | ✅ | ✅ | | | | |
| **Forms.Builder** | ✅ | ✅ | | | | |
| **Forms.Submit** | ✅ | ✅ | ✅ | | | |
| **Forms.Access** | ✅ | ✅ | ✅ | ✅ | | |
| **Intake.View** | ✅ | | ✅ | | | |
| **Intake.Create** | ✅ | | ✅ | | | |
| **Intake.Edit** | ✅ | | ✅ | | | |
| **Intake.Assign** | ✅ | | ✅ | | | |
| **Clients.View** | ✅ | | ✅ | | | ✅ |
| **Clients.Create** | ✅ | | ✅ | | | |
| **Clients.Edit** | ✅ | | ✅ | | | |
| **Clients.Archive** | ✅ | | | | | |
| **Cases.View** | ✅ | | ✅ | | | ✅ |
| **Cases.Manage** | ✅ | | ✅ | | | |
| **Approvals.View** | ✅ | | ✅ | ✅ | | |
| **Approvals.Level1** | ✅ | | ✅ | ✅ | | |
| **Approvals.Level2** | ✅ | | | ✅ | | |
| **Approvals.Level3** | ✅ | | | ✅ | | |
| **Approvals.Level4** | ✅ | | | ✅ | | |
| **Approvals.Level5** | ✅ | | | ✅ | | |
| **RefData.Manage** | ✅ | ✅ | | | | |
| **Reports.View** | ✅ | ✅ | | | ✅ | ✅ |
| **Reports.Admin** | ✅ | ✅ | | | | |
| **SP.Run** | ✅ | | | | | |
| **SP.Manage** | ✅ | | | | | |
| **Api.Access** | ✅ | | | | | |

> [!NOTE]
> This table reflects the **default seeded roles**. Your SWIMS administrator may have customised role permissions. Check **Admin → Roles** for the actual configuration in your environment.
