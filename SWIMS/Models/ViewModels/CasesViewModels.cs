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
        public int Id { get; set; }

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

        public int? FormId { get; set; }
        public string? FormName { get; set; }
        public string? SiteIdentityName { get; set; }
        public string? FormTypeName { get; set; }
        public List<string> ProgramTagNames { get; set; } = new();

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

        public int BeneficiaryId { get; set; }

        public string? BeneficiaryPhone { get; set; }

        public string? BeneficiaryIdNumber { get; set; }

        // Linked forms and assignments (for now read-only summaries)
        public IReadOnlyList<CaseFormSummaryViewModel> Forms { get; set; }
            = Array.Empty<CaseFormSummaryViewModel>();

        public IReadOnlyList<CaseAssignmentSummaryViewModel> Assignments { get; set; }
            = Array.Empty<CaseAssignmentSummaryViewModel>();

        // Benefit period (effective)
        public DateTime? BenefitStartAt { get; set; }
        public DateTime? BenefitEndAt { get; set; }
        public int? BenefitPeriodMonths { get; set; }
        public string? BenefitPeriodSource { get; set; }

        // Benefit period overrides (optional display)
        public DateTime? BenefitStartAtOverride { get; set; }
        public DateTime? BenefitEndAtOverride { get; set; }
        public int? BenefitPeriodMonthsOverride { get; set; }

        public bool IsStatusOverrideActive { get; set; }
        public string? StatusOverride { get; set; }
        public string? StatusOverrideReason { get; set; }
        public DateTime? StatusOverrideUntil { get; set; }
        public DateTime? StatusOverrideAt { get; set; }
        public string? StatusOverrideBy { get; set; }


    }

    public sealed class CaseLinkFormViewModel
    {
        [Required]
        public int SW_caseId { get; set; }

        public string CaseNumber { get; set; } = string.Empty;
        public string CaseTitle { get; set; } = string.Empty;

        [Display(Name = "Form submission")]
        [Required(ErrorMessage = "Please pick a form submission to link.")]
        public int? SelectedFormTableDatumId { get; set; }

        [Display(Name = "Primary application?")]
        public bool IsPrimaryApplication { get; set; }

        [Display(Name = "Also attach linked child forms (where available)")]
        public bool IncludeLinkedForms { get; set; } = true;

        public List<SelectListItem> AvailableForms { get; set; } = new();
    }

}
