using System;
using System.Collections.Generic;

namespace SWIMS.Models
{
    public class SW_case
    {
        public int Id { get; set; }

        public string case_number { get; set; } = null!;   // e.g. CASE-2025-00001

        public int SW_beneficiaryId { get; set; }

        public string title { get; set; } = null!;         // Short human label for the case

        public string status { get; set; } = null!;        // Pending, Active, Closed, etc.

        public string? program_tag { get; set; }           // e.g. “PublicAssistance”, “Kits”, etc.

        /// <summary>
        /// FK to ref.SW_programTag.Id (optional for legacy cases).
        /// </summary>
        public int? ProgramTagId { get; set; }

        public DateTime created_at { get; set; }

        public string? created_by { get; set; }            // Identity user id or name

        public DateTime? closed_at { get; set; }

        public string? notes { get; set; }

        // Navigation to beneficiary – used in app logic, but ignored by the Cases DbContext
        public SW_beneficiary? SW_beneficiary { get; set; }

        public ICollection<SW_caseForm> SW_caseForms { get; set; } = new List<SW_caseForm>();

        public ICollection<SW_caseAssignment> SW_caseAssignments { get; set; } = new List<SW_caseAssignment>();
    }
}
