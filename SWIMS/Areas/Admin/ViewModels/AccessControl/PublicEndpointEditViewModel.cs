using SWIMS.Models.Security;
using System.ComponentModel.DataAnnotations;

namespace SWIMS.Areas.Admin.ViewModels.AccessControl
{
    public class PublicEndpointEditViewModel
    {
        public int? Id { get; set; }

        [Required] public string MatchType { get; set; } = MatchTypes.ControllerAction;

        // Target fields (use depending on MatchType)
        public string? Area { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Page { get; set; }
        public string? Path { get; set; }
        public string? Regex { get; set; }

        [MaxLength(512)] public string? Notes { get; set; }

        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 100;
    }
}
