using System;
using System.Collections.Generic;

namespace SWIMS.Models.ViewModels
{
    public class ApprovalsDashboardViewModel
    {
        /// <summary>
        /// Approval levels (1–5) that this user is allowed to act on,
        /// based on Approvals_L1..L5 policies.
        /// </summary>
        public IReadOnlyList<int> UserLevels { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Forms that have approvals configured and (potentially) items for this user.
        /// </summary>
        public IReadOnlyList<ApprovalDashboardFormViewModel> Forms { get; set; }
            = Array.Empty<ApprovalDashboardFormViewModel>();
    }

    public class ApprovalDashboardFormViewModel
    {
        public int FormId { get; set; }
        public string FormUuid { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;

        /// <summary>
        /// Number of approval levels configured on this form (0–5).
        /// </summary>
        public int ApprovalLevelsConfigured { get; set; }

        /// <summary>
        /// Total number of entries pending for THIS user’s approval level(s)
        /// on this form (we never show other levels’ counts).
        /// </summary>
        public int PendingCount { get; set; }
    }
}
