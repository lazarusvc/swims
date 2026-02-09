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
}
