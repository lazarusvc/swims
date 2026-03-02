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

    public static class Admin
    {
        public static class Reports
        {
            public const string DefinitionCreated = "Swims.Events.Admin.Reports.DefinitionCreated";
            public const string DefinitionUpdated = "Swims.Events.Admin.Reports.DefinitionUpdated";
            public const string DefinitionDeleted = "Swims.Events.Admin.Reports.DefinitionDeleted";

            public static class Parameters
            {
                public const string Created = "Swims.Events.Admin.Reports.Parameters.Created";
                public const string Updated = "Swims.Events.Admin.Reports.Parameters.Updated";
                public const string Deleted = "Swims.Events.Admin.Reports.Parameters.Deleted";
                public const string QuickAdded = "Swims.Events.Admin.Reports.Parameters.QuickAdded";
            }
        }
    }

    public static class Reports
    {
        public const string ExportFailed = "Swims.Events.Reports.ExportFailed";
        public const string ExportSucceeded = "Swims.Events.Reports.ExportSucceeded";
    }



    // -------------------------------
    // Identity / Admin (Users + Roles)
    // -------------------------------
    public static class Identity
    {
        public static class Users
        {
            public const string Created = "Swims.Events.Identity.Users.Created";
            public const string Updated = "Swims.Events.Identity.Users.Updated";
            public const string Deleted = "Swims.Events.Identity.Users.Deleted";
            public const string RolesUpdated = "Swims.Events.Identity.Users.RolesUpdated";
        }

        public static class Roles
        {
            public const string Created = "Swims.Events.Identity.Roles.Created";
            public const string Updated = "Swims.Events.Identity.Roles.Updated";
            public const string Deleted = "Swims.Events.Identity.Roles.Deleted";
            public const string MembershipUpdated = "Swims.Events.Identity.Roles.MembershipUpdated";
        }
    }

    // -------------------------------
    // Forms / Form Report Config
    // -------------------------------
    public static class FormReports
    {
        public const string Created = "Swims.Events.Forms.Reports.Created";
        public const string Updated = "Swims.Events.Forms.Reports.Updated";
        public const string Deleted = "Swims.Events.Forms.Reports.Deleted";
    }

    // -------------------------------
    // Forms / Form Process Config
    // -------------------------------
    public static class FormProcess
    {
        public const string ProcessCreated = "Swims.Events.Forms.ProcessCreated";
        public const string ProcessUpdated = "Swims.Events.Forms.ProcessUpdated";
        public const string ProcessDeleted = "Swims.Events.Forms.ProcessDeleted";

    }

    // -------------------------------
    // Stored Procedures
    // -------------------------------
    public static class StoredProcedures
    {
        public const string Created = "Swims.Events.StoredProcedures.Created";
        public const string Updated = "Swims.Events.StoredProcedures.Updated";
        public const string Deleted = "Swims.Events.StoredProcedures.Deleted";

        public const string Executed = "Swims.Events.StoredProcedures.Executed";
        public const string Exported = "Swims.Events.StoredProcedures.Exported";

        public static class Parameters
        {
            public const string Created = "Swims.Events.StoredProcedures.Parameters.Created";
            public const string Updated = "Swims.Events.StoredProcedures.Parameters.Updated";
            public const string Deleted = "Swims.Events.StoredProcedures.Parameters.Deleted";
        }
    }

    public static class System
    {
        public static class Elsa
        {
            public const string Unavailable = "Swims.Events.System.Elsa.Unavailable";
        }
    }
}
