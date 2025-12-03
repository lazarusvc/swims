using System.Collections.Generic;
using SWIMS.Services.SystemSettings;

namespace SWIMS.Areas.Admin.ViewModels.SystemSettings
{
    public class SystemSettingsIndexViewModel
    {
        public string? ActiveEnvironment { get; set; }
        public IList<SystemSettingsSectionSummary> Sections { get; set; } = new List<SystemSettingsSectionSummary>();
    }
}
