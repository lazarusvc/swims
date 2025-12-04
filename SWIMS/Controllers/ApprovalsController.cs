using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using SWIMS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    [Authorize] // Any authenticated user, then we narrow down inside by Approvals_L1..L5
    public class ApprovalsController : Controller
    {
        private readonly SwimsDb_moreContext _db;
        private readonly IAuthorizationService _authorization;

        public ApprovalsController(
            SwimsDb_moreContext db,
            IAuthorizationService authorization)
        {
            _db = db;
            _authorization = authorization;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Figure out which approval levels this user actually has.
            var userLevels = await GetUserApprovalLevelsAsync();

            // 2. Get all forms that have approvals configured at all.
            var forms = await _db.SW_forms
                .Where(f => f.approvalAmt != null && f.approvalAmt > 0)
                .Select(f => new
                {
                    f.Id,
                    f.uuid,
                    f.name,
                    f.approvalAmt
                })
                .OrderBy(f => f.name)
                .ToListAsync();

            var formIds = forms.Select(f => f.Id).ToList();

            // Nothing to do if no forms or no levels; we still return a model
            // so the view can show a friendly message instead of 403.
            var pendingByLevel = await GetPendingCountsByLevelAsync(userLevels, formIds);

            var items = new List<ApprovalDashboardFormViewModel>();

            foreach (var form in forms)
            {
                var pending = 0;

                // Only count for levels this user has, and only up to configured approvalAmt.
                foreach (var level in userLevels)
                {
                    if (form.approvalAmt.HasValue && level > form.approvalAmt.Value)
                        continue;

                    if (pendingByLevel.TryGetValue(level, out var byForm) &&
                        byForm.TryGetValue(form.Id, out var cnt))
                    {
                        pending += cnt;
                    }
                }

                items.Add(new ApprovalDashboardFormViewModel
                {
                    FormId = form.Id,
                    FormUuid = form.uuid,
                    FormName = form.name,
                    ApprovalLevelsConfigured = form.approvalAmt ?? 0,
                    PendingCount = pending
                });
            }

            var model = new ApprovalsDashboardViewModel
            {
                UserLevels = userLevels,
                Forms = items
            };

            return View(model);
        }

        /// <summary>
        /// Figure out which approval levels (1–5) this user is allowed to act on
        /// based on Approvals_L1..L5 policies.
        /// </summary>
        private async Task<List<int>> GetUserApprovalLevelsAsync()
        {
            var levels = new List<int>();

            if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L1)).Succeeded)
                levels.Add(1);

            if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L2)).Succeeded)
                levels.Add(2);

            if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L3)).Succeeded)
                levels.Add(3);

            if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L4)).Succeeded)
                levels.Add(4);

            if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L5)).Succeeded)
                levels.Add(5);

            return levels;
        }

        /// <summary>
        /// For each approval level the user has, get a per-form count of pending entries
        /// (isApproval_x == 0) across all forms in <paramref name="formIds"/>.
        /// 
        /// Result shape: level -> (formId -> count)
        /// </summary>
        private async Task<Dictionary<int, Dictionary<int, int>>> GetPendingCountsByLevelAsync(
            IReadOnlyCollection<int> levels,
            IReadOnlyCollection<int> formIds)
        {
            var result = new Dictionary<int, Dictionary<int, int>>();

            if (levels.Count == 0 || formIds.Count == 0)
                return result;

            // Level 1
            if (levels.Contains(1))
            {
                var l1 = await _db.SW_formTableData
                    .Where(d => formIds.Contains(d.SW_formsId) && d.isApproval_01 == 0)
                    .GroupBy(d => d.SW_formsId)
                    .Select(g => new { FormId = g.Key, Count = g.Count() })
                    .ToListAsync();

                result[1] = l1.ToDictionary(x => x.FormId, x => x.Count);
            }

            // Level 2
            if (levels.Contains(2))
            {
                var l2 = await _db.SW_formTableData
                    .Where(d => formIds.Contains(d.SW_formsId) && d.isApproval_02 == 0)
                    .GroupBy(d => d.SW_formsId)
                    .Select(g => new { FormId = g.Key, Count = g.Count() })
                    .ToListAsync();

                result[2] = l2.ToDictionary(x => x.FormId, x => x.Count);
            }

            // Level 3
            if (levels.Contains(3))
            {
                var l3 = await _db.SW_formTableData
                    .Where(d => formIds.Contains(d.SW_formsId) && d.isApproval_03 == 0)
                    .GroupBy(d => d.SW_formsId)
                    .Select(g => new { FormId = g.Key, Count = g.Count() })
                    .ToListAsync();

                result[3] = l3.ToDictionary(x => x.FormId, x => x.Count);
            }

            // Level 4
            if (levels.Contains(4))
            {
                var l4 = await _db.SW_formTableData
                    .Where(d => formIds.Contains(d.SW_formsId) && d.isApproval_04 == 0)
                    .GroupBy(d => d.SW_formsId)
                    .Select(g => new { FormId = g.Key, Count = g.Count() })
                    .ToListAsync();

                result[4] = l4.ToDictionary(x => x.FormId, x => x.Count);
            }

            // Level 5
            if (levels.Contains(5))
            {
                var l5 = await _db.SW_formTableData
                    .Where(d => formIds.Contains(d.SW_formsId) && d.isApproval_05 == 0)
                    .GroupBy(d => d.SW_formsId)
                    .Select(g => new { FormId = g.Key, Count = g.Count() })
                    .ToListAsync();

                result[5] = l5.ToDictionary(x => x.FormId, x => x.Count);
            }

            return result;
        }
    }
}
