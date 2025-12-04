using System;

namespace SWIMS.Models
{
    public class SW_caseForm
    {
        public int Id { get; set; }

        public int SW_caseId { get; set; }

        public int SW_formTableDatumId { get; set; }

        /// <summary>
        /// Semantic role of this form in the case: "Application", "Intake", "Assessment", etc.
        /// </summary>
        public string? form_role { get; set; }

        /// <summary>
        /// True if this is the primary application form for the case.
        /// </summary>
        public bool is_primary_application { get; set; }

        public DateTime? linked_at { get; set; }

        public string? linked_by { get; set; }

        public SW_case SW_case { get; set; } = null!;

        /// <summary>
        /// Convenience navigation to the underlying form record.
        /// Not managed by the cases DbContext.
        /// </summary>
        public SW_formTableDatum? SW_formTableDatum { get; set; }
    }
}
