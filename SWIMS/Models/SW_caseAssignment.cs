using System;

namespace SWIMS.Models
{
    public class SW_caseAssignment
    {
        public int Id { get; set; }

        public int SW_caseId { get; set; }

        /// <summary>
        /// ASP.NET Identity user Id.
        /// </summary>
        public string user_id { get; set; } = null!;

        /// <summary>
        /// Role within the case: e.g., "PrimaryWorker", "Supervisor".
        /// </summary>
        public string? role_on_case { get; set; }

        public DateTime assigned_at { get; set; }

        public DateTime? unassigned_at { get; set; }

        public bool is_active { get; set; }

        public SW_case SW_case { get; set; } = null!;
    }
}
