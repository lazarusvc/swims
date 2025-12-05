using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SWIMS.Models.ViewModels
{
    /// <summary>
    /// View model used to assign a staff member to a case.
    /// </summary>
    public class CaseAssignViewModel
    {
        public int CaseId { get; set; }

        public string CaseNumber { get; set; } = string.Empty;
        public string CaseTitle { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Staff member")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Role on case")]
        [StringLength(64)]
        public string? RoleOnCase { get; set; }

        [Display(Name = "Active on case")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> AvailableUsers { get; set; } = new();
    }
}
