using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using SWIMS.Data.Cases;

namespace SWIMS.Controllers
{
    [Authorize]
    public class CasesController : Controller
    {
        private readonly SwimsCasesDbContext _cases;
        private readonly SwimsDb_moreContext _core;

        public CasesController(
            SwimsCasesDbContext cases,
            SwimsDb_moreContext core)
        {
            _cases = cases;
            _core = core;
        }

        // GET: /Cases
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = _cases.SW_cases.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();

                // For v1: limit search to case fields only (no cross-context joins).
                query = query.Where(c =>
                    c.case_number.Contains(s) ||
                    c.title.Contains(s) ||
                    c.status.Contains(s) ||
                    (c.program_tag != null && c.program_tag.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.status == status);
            }

            var caseEntities = await query
                .OrderByDescending(c => c.created_at)
                .ToListAsync();

            var beneficiaryIds = caseEntities
                .Select(c => c.SW_beneficiaryId)
                .Distinct()
                .ToList();

            var beneficiaries = await _core.SW_beneficiaries
                .AsNoTracking()
                .Where(b => beneficiaryIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id);

            var listItems = new List<CaseListItemViewModel>();

            foreach (var c in caseEntities)
            {
                beneficiaries.TryGetValue(c.SW_beneficiaryId, out var b);

                var name = b != null
                    ? (!string.IsNullOrWhiteSpace(b.name)
                        ? b.name
                        : $"{b.first_name} {b.last_name}".Trim())
                    : "(no beneficiary)";

                var uuid = b?.uuid ?? string.Empty;

                listItems.Add(new CaseListItemViewModel
                {
                    Id = c.Id,
                    CaseNumber = c.case_number,
                    Title = c.title,
                    Status = c.status,
                    ProgramTag = c.program_tag,
                    CreatedAt = c.created_at,
                    CreatedBy = c.created_by,
                    BeneficiaryName = name,
                    BeneficiaryUuid = uuid
                });
            }

            var vm = new CaseIndexViewModel
            {
                Cases = listItems,
                SearchText = search,
                StatusFilter = status
            };

            return View(vm);
        }

        // GET: /Cases/Create
        public async Task<IActionResult> Create()
        {
            var vm = new CaseCreateViewModel
            {
                CaseNumber = await GenerateNextCaseNumberAsync(),
                Status = "Pending"
            };

            vm.Beneficiaries = await BuildBeneficiarySelectListAsync();

            return View(vm);
        }

        // POST: /Cases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Beneficiaries = await BuildBeneficiarySelectListAsync();
                return View(vm);
            }

            var userId = User?.Identity?.Name ?? "system";

            // Pull the beneficiary so we can name the case after them
            var beneficiary = await _core.SW_beneficiaries
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == vm.SW_beneficiaryId);

            if (beneficiary == null)
            {
                ModelState.AddModelError(nameof(vm.SW_beneficiaryId), "Selected beneficiary not found.");
                vm.Beneficiaries = await BuildBeneficiarySelectListAsync();
                return View(vm);
            }

            var caseTitle = !string.IsNullOrWhiteSpace(beneficiary.name)
                ? beneficiary.name
                : $"{beneficiary.first_name} {beneficiary.last_name}".Trim();

            var entity = new SW_case
            {
                case_number = vm.CaseNumber,
                SW_beneficiaryId = vm.SW_beneficiaryId,
                title = caseTitle,
                status = string.IsNullOrWhiteSpace(vm.Status)
                    ? "Pending"
                    : vm.Status.Trim(),
                program_tag = string.IsNullOrWhiteSpace(vm.ProgramTag)
                    ? "Unspecified"
                    : vm.ProgramTag!.Trim(),
                created_at = DateTime.UtcNow,
                created_by = userId,
                closed_at = null,
                notes = string.IsNullOrWhiteSpace(vm.Notes)
                    ? null
                    : vm.Notes!.Trim()
            };

            _cases.SW_cases.Add(entity);
            await _cases.SaveChangesAsync();

            TempData["Ok"] = "Case created successfully.";
            return RedirectToAction(nameof(Details), new { id = entity.Id });
        }


        // GET: /Cases/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var caseEntity = await _cases.SW_cases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
            {
                return NotFound();
            }

            var beneficiary = await _core.SW_beneficiaries
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == caseEntity.SW_beneficiaryId);

            var forms = await _cases.SW_caseForms
                .AsNoTracking()
                .Where(f => f.SW_caseId == id)
                .OrderByDescending(f => f.linked_at)
                .Select(f => new CaseFormSummaryViewModel
                {
                    Id = f.Id,
                    Role = f.form_role ?? string.Empty,
                    IsPrimary = f.is_primary_application,
                    LinkedAt = f.linked_at,
                    LinkedBy = f.linked_by,
                    FormTableDataId = f.SW_formTableDatumId
                })
                .ToListAsync();

            var assignments = await _cases.SW_caseAssignments
                .AsNoTracking()
                .Where(a => a.SW_caseId == id)
                .OrderByDescending(a => a.assigned_at)
                .Select(a => new CaseAssignmentSummaryViewModel
                {
                    Id = a.Id,
                    UserId = a.user_id,
                    RoleOnCase = a.role_on_case ?? string.Empty,
                    AssignedAt = a.assigned_at,
                    UnassignedAt = a.unassigned_at,
                    IsActive = a.is_active
                })
                .ToListAsync();

            var vm = new CaseDetailsViewModel
            {
                Id = caseEntity.Id,
                CaseNumber = caseEntity.case_number,
                Title = caseEntity.title,
                Status = caseEntity.status,
                ProgramTag = caseEntity.program_tag,
                Notes = caseEntity.notes,
                CreatedAt = caseEntity.created_at,
                CreatedBy = caseEntity.created_by,
                ClosedAt = caseEntity.closed_at,
                BeneficiaryName = beneficiary != null
                    ? (!string.IsNullOrWhiteSpace(beneficiary.name)
                        ? beneficiary.name
                        : $"{beneficiary.first_name} {beneficiary.last_name}".Trim())
                    : "(no beneficiary)",
                BeneficiaryUuid = beneficiary?.uuid ?? string.Empty,
                BeneficiaryPhone = beneficiary?.telephone_number,
                BeneficiaryIdNumber = beneficiary?.id_number,
                Forms = forms,
                Assignments = assignments
            };

            return View(vm);
        }

        // ----------------- helpers -----------------

        private async Task<string> GenerateNextCaseNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"CASE-{year}-";

            var lastForYear = await _cases.SW_cases
                .AsNoTracking()
                .Where(c => c.case_number.StartsWith(prefix))
                .OrderByDescending(c => c.case_number)
                .FirstOrDefaultAsync();

            var nextSequence = 1;

            if (lastForYear != null)
            {
                var suffix = lastForYear.case_number.Substring(prefix.Length);
                if (int.TryParse(suffix, out var lastSeq))
                {
                    nextSequence = lastSeq + 1;
                }
            }

            return $"{prefix}{nextSequence:D5}";
        }

        private async Task<List<SelectListItem>> BuildBeneficiarySelectListAsync()
        {
            var beneficiaries = await _core.SW_beneficiaries
                .AsNoTracking()
                .OrderBy(b => b.name)
                .ThenBy(b => b.last_name)
                .ToListAsync();

            return beneficiaries
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = !string.IsNullOrWhiteSpace(b.name)
                        ? b.name
                        : $"{b.first_name} {b.last_name}".Trim()
                })
                .ToList();
        }
    }
}
