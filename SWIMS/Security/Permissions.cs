namespace SWIMS.Security;
public static class Permissions
{
    // Admin
    public const string Admin_Users = "Admin.Users";
    public const string Admin_Roles = "Admin.Roles";
    public const string Admin_Settings = "Admin.Settings";
    public const string Admin_Policies = "Admin.Policies";
    public const string Admin_Endpoints = "Admin.Endpoints";
    public const string Admin_PublicEndpoints = "Admin.PublicEndpoints";
    public const string Admin_RouteInspector = "Admin.RouteInspector";
    public const string Admin_Hangfire = "Admin.Hangfire";
    public const string Admin_ApiDashboard = "Admin.ApiDashboard";
    public const string Admin_SessionLog = "Admin.SessionLogs";
    public const string Admin_AuditLogs = "Admin.AuditLogs";

    // Forms
    public const string Programs_View = "Programs.View";   // programs dashboard
    public const string Forms_Manage = "Forms.Manage";    // forms admin CRUD (not the builder)
    public const string Forms_Builder = "Forms.Builder";
    public const string Forms_Submit = "Forms.Submit";

    // Intake
    public const string Intake_View = "Intake.View";
    public const string Intake_Create = "Intake.Create";
    public const string Intake_Edit = "Intake.Edit";
    public const string Intake_Assign = "Intake.Assign";

    // Clients
    public const string Clients_View = "Clients.View";
    public const string Clients_Create = "Clients.Create";
    public const string Clients_Edit = "Clients.Edit";
    public const string Clients_Archive = "Clients.Archive";

    // Applications
    public const string Applications_View = "Applications.View";
    public const string Applications_Edit = "Applications.Edit";
    public const string Applications_Assign = "Applications.Assign";

    // Assessment & Recommendations
    public const string Assessment_View = "Assessment.View";

    // Approvals
    public const string Approvals_L1 = "Approvals.Level1"; // Social Worker
    public const string Approvals_L2 = "Approvals.Level2"; // Coordinator
    public const string Approvals_L3 = "Approvals.Level3"; // Director
    public const string Approvals_L4 = "Approvals.Level4"; // Permanent Secretary
    public const string Approvals_L5 = "Approvals.Level5"; // Minister
    // Extended approvals to mirror SW→Coord→Director→PS→Minister chain

    // Payments & Reconciliation
    public const string Payments_View = "Payments.View";
    public const string Payments_Validate = "Payments.Validate";       // SEOA
    public const string Payments_ExportLists = "Payments.ExportLists";    // Welfare 6/7 / SmartStream prep
    public const string Payments_Reconcile = "Payments.Reconcile";

    // Reference Data (Beneficiaries, Cities, Orgs, Financial Institutions)
    public const string RefData_Manage = "RefData.Manage";

    // Stored Procedures (Ops)
    public const string SP_Run = "SP.Run";
    public const string SP_Manage = "SP.Manage";
    public const string SP_Params = "SP.Params";

    // Reporting
    public const string Reports_View = "Reports.View";
    public const string Reports_Admin = "Reports.Admin";

    // API
    public const string Api_Access = "Api.Access";

}
