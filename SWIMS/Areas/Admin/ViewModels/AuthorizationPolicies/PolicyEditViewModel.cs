using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SWIMS.Areas.Admin.ViewModels.AuthorizationPolicies
{
    public class PolicyEditViewModel
    {
        public int? Id { get; set; }

        [Required, MaxLength(128)]
        public string Name { get; set; } = default!; // Kept immutable on Edit for safety

        [MaxLength(512)]
        public string? Description { get; set; }

        public bool IsEnabled { get; set; } = true;

        public List<SelectListItem> AllRoles { get; set; } = new();
        public List<int> SelectedRoleIds { get; set; } = new();
    }
}
