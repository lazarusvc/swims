using System;
using System.ComponentModel.DataAnnotations;

namespace SWIMS.Models.ViewModels
{
    public sealed class CaseBenefitPeriodEditViewModel
    {
        public int Id { get; set; }

        public string CaseNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        // ===== Effective (computed) values currently on the case =====
        [Display(Name = "Effective start")]
        public DateTime? BenefitStartAt { get; set; }

        [Display(Name = "Effective end")]
        public DateTime? BenefitEndAt { get; set; }

        [Display(Name = "Effective period (months)")]
        public int? BenefitPeriodMonths { get; set; }

        [Display(Name = "Period source")]
        public string? BenefitPeriodSource { get; set; }

        // ===== Override inputs =====
        [Display(Name = "Override start date")]
        [DataType(DataType.Date)]
        public DateTime? BenefitStartAtOverride { get; set; }

        [Display(Name = "Override end date")]
        [DataType(DataType.Date)]
        public DateTime? BenefitEndAtOverride { get; set; }

        [Display(Name = "Override period (months)")]
        [Range(1, 120, ErrorMessage = "Override months must be between 1 and 120.")]
        public int? BenefitPeriodMonthsOverride { get; set; }

        [Display(Name = "Clear all overrides")]
        public bool ClearOverrides { get; set; }
    }
}
