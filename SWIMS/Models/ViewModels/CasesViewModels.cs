using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SWIMS.Models.ViewModels
{
    public class CaseListItemViewModel
    {
        public int Id { get; set; }

        public string CaseNumber { get; set; } = default!;

        public string Title { get; set; } = default!;

        public string Status { get; set; } = default!;

        public string BeneficiaryName { get; set; } = default!;

        public string BeneficiaryUuid { get; set; } = default!;

        public string? ProgramTag { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }
    }

    public class CaseIndexViewModel
    {
        public IReadOnlyList<CaseListItemViewModel> Cases { get; set; }
            = Array.Empty<CaseListItemViewModel>();

        public string? SearchText { get; set; }

        public string? StatusFilter { get; set; }

        public int? ProgramFilter { get; set; }

        public List<SelectListItem> ProgramOptions { get; set; } = new();
    }

    public class CaseCreateViewModel
    {
        [Display(Name = "Case number")]
        public string CaseNumber { get; set; } = default!;

        [Required]
        [Display(Name = "Beneficiary")]
        public int SW_beneficiaryId { get; set; }

        [MaxLength(256)]
        [Display(Name = "Case title (auto-filled from beneficiary)")]
        public string? Title { get; set; }

        [Required]
        [MaxLength(32)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        // Legacy backing string; we’ll still populate this from the selected tag/code for now.
        [MaxLength(128)]
        [Display(Name = "Programme tag (legacy)")]
        public string? ProgramTag { get; set; }

        [Display(Name = "Programme")]
        public int? ProgramTagId { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Dropdown options
        public List<SelectListItem> Beneficiaries { get; set; } = new();

        // Dropdown options for programmes
        public List<SelectListItem> ProgramOptions { get; set; } = new();
    }

    public class CaseFormSummaryViewModel
    {
        public int Id { get; set; }

        public string Role { get; set; } = default!;

        public bool IsPrimary { get; set; }

        public DateTime? LinkedAt { get; set; }

        public string? LinkedBy { get; set; }

        public int FormTableDataId { get; set; }
    }

    public class CaseAssignmentSummaryViewModel
    {
        public int Id { get; set; }

        // Raw Identity key (as stored in SW_caseAssignment.user_id)
        public string UserId { get; set; } = string.Empty;

        // Friendly display name from SwUser (First + Last / FullName)
        public string UserDisplayName { get; set; } = string.Empty;

        // Comma-separated system roles (e.g. "SocialWorker, Supervisor")
        public string SystemRoles { get; set; } = string.Empty;

        // Case-specific role (e.g. "Primary Social Worker")
        public string RoleOnCase { get; set; } = string.Empty;

        public DateTime? AssignedAt { get; set; }
        public DateTime? UnassignedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CaseDetailsViewModel
    {
        public int Id { get; set; }

        public string CaseNumber { get; set; } = default!;

        public string Title { get; set; } = default!;

        public string Status { get; set; } = default!;

        public string? ProgramTag { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? ClosedAt { get; set; }

        // Beneficiary summary
        public string BeneficiaryName { get; set; } = default!;

        public string BeneficiaryUuid { get; set; } = default!;

        public string? BeneficiaryPhone { get; set; }

        public string? BeneficiaryIdNumber { get; set; }

        // Linked forms and assignments (for now read-only summaries)
        public IReadOnlyList<CaseFormSummaryViewModel> Forms { get; set; }
            = Array.Empty<CaseFormSummaryViewModel>();

        public IReadOnlyList<CaseAssignmentSummaryViewModel> Assignments { get; set; }
            = Array.Empty<CaseAssignmentSummaryViewModel>();
    }

    public class CaseLinkFormViewModel
    {
        [Required]
        public int SW_caseId { get; set; }

        public string? CaseNumber { get; set; } = default!;

        public string? CaseTitle { get; set; } = default!;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a form submission.")]
        [Display(Name = "Form submission")]
        public int SelectedFormTableDatumId { get; set; }


        [Display(Name = "Form role in this case")]
        public string? FormRole { get; set; }

        [Display(Name = "Mark as primary application")]
        public bool IsPrimaryApplication { get; set; }

        public List<SelectListItem> AvailableForms { get; set; } = new();
    }
}
