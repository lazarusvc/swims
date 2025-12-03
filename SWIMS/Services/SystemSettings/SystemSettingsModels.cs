using System.Collections.Generic;

namespace SWIMS.Services.SystemSettings
{
    public class SystemSettingsOverview
    {
        public string? ActiveEnvironment { get; set; }
        public IList<SystemSettingsSectionSummary> Sections { get; set; } = new List<SystemSettingsSectionSummary>();
    }

    public class SystemSettingsSectionSummary
    {
        public string Key { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? Description { get; set; }

        /// <summary>
        /// True if this section appears in the main appsettings JSON file
        /// (for the current environment).
        /// </summary>
        public bool IsFromAppSettings { get; set; }

        /// <summary>
        /// True if the section currently has any value in the merged configuration
        /// (from any provider: JSON, env vars, secrets, etc.).
        /// </summary>
        public bool IsConfigured { get; set; }
    }

    public class SystemSettingsSection
    {
        public string Key { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? Description { get; set; }

        /// <summary>
        /// Environment name used to load/save (e.g. "Development", "Production").
        /// Null / empty means "current environment".
        /// </summary>
        public string? EnvironmentName { get; set; }

        public IList<SystemSettingsItem> Items { get; set; } = new List<SystemSettingsItem>();
    }

    public class SystemSettingsItem
    {
        /// <summary>
        /// Full configuration key path, e.g. "Emailing:Smtp:Host".
        /// </summary>
        public string KeyPath { get; set; } = default!;

        public string DisplayName { get; set; } = default!;
        public string? Description { get; set; }

        public string? Value { get; set; }

        /// <summary>
        /// "string", "bool", "int", etc. Used for basic UI decisions.
        /// </summary>
        public string DataType { get; set; } = "string";

        /// <summary>
        /// If true, UI should treat this as sensitive (mask value etc.).
        /// </summary>
        public bool IsSecret { get; set; }

        public bool IsRequired { get; set; }
    }
}
