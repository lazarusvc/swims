namespace SWIMS.Services.Notifications;

public static class SwimsEventKeys
{
    public static class Cases
    {
        public const string Created = "Swims.Events.Cases.Created";
        public const string Updated = "Swims.Events.Cases.Updated"; // route optional (not in your DB DATA.csv yet)

        public const string Assigned = "Swims.Events.Cases.Assigned";
        public const string Unassigned = "Swims.Events.Cases.Unassigned";

        public const string StatusChanged = "Swims.Events.Cases.StatusChanged";

        public const string FormLinked = "Swims.Events.Cases.FormLinked";
        public const string FormDetached = "Swims.Events.Cases.FormDetached";

        public const string RefreshedFromPrimaryApplication = "Swims.Events.Cases.RefreshedFromPrimaryApplication";

        public const string BenefitPeriodOverridesSaved = "Swims.Events.Cases.BenefitPeriodOverridesSaved";
    }

    public static class Forms
    {
        public const string DefinitionCreated = "Swims.Events.Forms.DefinitionCreated";
        public const string DefinitionUpdated = "Swims.Events.Forms.DefinitionUpdated";
        public const string DefinitionDeleted = "Swims.Events.Forms.DefinitionDeleted";
        public const string DefinitionPublished = "Swims.Events.Forms.DefinitionPublished";
        public const string DefinitionCompleted = "Swims.Events.Forms.DefinitionCompleted";

        public const string EntryCreated = "Swims.Events.Forms.EntryCreated";
        public const string EntryUpdated = "Swims.Events.Forms.EntryUpdated";
        public const string EntryDeleted = "Swims.Events.Forms.EntryDeleted";
    }

    public static class Approvals
    {
        public const string PendingL1 = "Swims.Events.Approvals.Pending.L1";
        public const string PendingL2 = "Swims.Events.Approvals.Pending.L2";
        public const string PendingL3 = "Swims.Events.Approvals.Pending.L3";
        public const string PendingL4 = "Swims.Events.Approvals.Pending.L4";
        public const string PendingL5 = "Swims.Events.Approvals.Pending.L5";

        public const string FinalApproved = "Swims.Events.Approvals.FinalApproved";

        public static string PendingForLevel(int level) => level switch
        {
            1 => PendingL1,
            2 => PendingL2,
            3 => PendingL3,
            4 => PendingL4,
            5 => PendingL5,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Approval level must be 1..5.")
        };
    }

    // Admin/Security
    public static class Security
    {
        public static class AuthorizationPolicies
        {
            public const string Created = "Swims.Events.Security.AuthorizationPolicies.Created";
            public const string Updated = "Swims.Events.Security.AuthorizationPolicies.Updated";
            public const string Toggled = "Swims.Events.Security.AuthorizationPolicies.Toggled";
            public const string Deleted = "Swims.Events.Security.AuthorizationPolicies.Deleted";
        }

        // Matches your existing controller naming: "Endpoint policy assignment ..."
        public static class EndpointPolicyAssignments
        {
            public const string Created = "Swims.Events.Security.EndpointPolicyAssignments.Created";
            public const string Updated = "Swims.Events.Security.EndpointPolicyAssignments.Updated";
            public const string Toggled = "Swims.Events.Security.EndpointPolicyAssignments.Toggled";
            public const string Deleted = "Swims.Events.Security.EndpointPolicyAssignments.Deleted";
            public const string BulkCreated = "Swims.Events.Security.EndpointPolicyAssignments.BulkCreated";
        }

        public static class PublicEndpoints
        {
            public const string Created = "Swims.Events.Security.PublicEndpoints.Created";
            public const string Updated = "Swims.Events.Security.PublicEndpoints.Updated";
            public const string Toggled = "Swims.Events.Security.PublicEndpoints.Toggled";
            public const string Deleted = "Swims.Events.Security.PublicEndpoints.Deleted";
        }
    }

    // SSRS / reporting
    public static class Reports
    {
        public const string ExportFailed = "Swims.Events.Reports.ExportFailed";
        public const string ExportSucceeded = "Swims.Events.Reports.ExportSucceeded";
        public const string Run = "Swims.Events.Reports.Run";

        public static class Definitions
        {
            public const string Created = "Swims.Events.Reports.Definitions.Created";
            public const string Updated = "Swims.Events.Reports.Definitions.Updated";
            public const string Deleted = "Swims.Events.Reports.Definitions.Deleted";
        }

        public static class Parameters
        {
            public const string Created = "Swims.Events.Reports.Parameters.Created";
            public const string Updated = "Swims.Events.Reports.Parameters.Updated";
            public const string Deleted = "Swims.Events.Reports.Parameters.Deleted";
            public const string QuickAdded = "Swims.Events.Reports.Parameters.QuickAdded";
        }
    }

    // Form Reports (formReportController)
    public static class FormReports
    {
        public const string Created = "Swims.Events.Forms.Reports.Created";
        public const string Updated = "Swims.Events.Forms.Reports.Updated";
        public const string Deleted = "Swims.Events.Forms.Reports.Deleted";
    }

    // Identity (users/roles admin controllers)
    public static class Identity
    {
        public static class Roles
        {
            public const string Created = "Swims.Events.Identity.Roles.Created";
            public const string Updated = "Swims.Events.Identity.Roles.Updated";
            public const string Deleted = "Swims.Events.Identity.Roles.Deleted";
            public const string MembershipUpdated = "Swims.Events.Identity.Roles.MembershipUpdated";
        }

        public static class Users
        {
            public const string Created = "Swims.Events.Identity.Users.Created";
            public const string Updated = "Swims.Events.Identity.Users.Updated";
            public const string Deleted = "Swims.Events.Identity.Users.Deleted";
            public const string RolesUpdated = "Swims.Events.Identity.Users.RolesUpdated";
        }
    }

    // Stored procedures
    public static class StoredProcesses
    {
        public const string Created = "Swims.Events.StoredProcesses.Created";
        public const string Updated = "Swims.Events.StoredProcesses.Updated";
        public const string Deleted = "Swims.Events.StoredProcesses.Deleted";

        // Matches your existing notify blocks: executed + export (status lives in metadata)
        public const string Executed = "Swims.Events.StoredProcesses.Executed";
        public const string Exported = "Swims.Events.StoredProcesses.Exported";

        public static class Parameters
        {
            public const string Created = "Swims.Events.StoredProcesses.Parameters.Created";
            public const string Updated = "Swims.Events.StoredProcesses.Parameters.Updated";
            public const string Deleted = "Swims.Events.StoredProcesses.Parameters.Deleted";
        }
    }
}
