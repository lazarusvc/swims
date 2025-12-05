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

        [MaxLength(128)]
        [Display(Name = "Programme tag")]
        public string? ProgramTag { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Dropdown options
        public List<SelectListItem> Beneficiaries { get; set; } = new();
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

        public string UserId { get; set; } = default!;

        public string RoleOnCase { get; set; } = default!;

        public DateTime AssignedAt { get; set; }

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
}
