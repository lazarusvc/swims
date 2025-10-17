using Microsoft.AspNetCore.Mvc.Rendering;
using SWIMS.Models.Security;
using System.ComponentModel.DataAnnotations;

namespace SWIMS.Areas.Admin.ViewModels.AccessControl
{
    public class EndpointPolicyEditViewModel
    {
        public int? Id { get; set; }

        [Required] public string MatchType { get; set; } = MatchTypes.ControllerAction;

        public string? Area { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Page { get; set; }
        public string? Path { get; set; }
        public string? Regex { get; set; }

        [Required, MaxLength(128)] public string PolicyName { get; set; } = default!;
        public int PolicyId { get; set; }

        [MaxLength(512)] public string? Notes { get; set; }

        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 100;

        // selects
        public List<SelectListItem> Policies { get; set; } = new();
    }
}
