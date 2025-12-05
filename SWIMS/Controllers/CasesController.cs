using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Data.Cases;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using SWIMS.Data.Lookups;


namespace SWIMS.Controllers
{
    [Authorize]
    public class CasesController : Controller
    {
        private readonly SwimsCasesDbContext _cases;
        private readonly SwimsDb_moreContext _core;
        private readonly SwimsIdentityDbContext _identity;
        private readonly UserManager<SwUser> _userManager;
        private readonly SwimsLookupDbContext _lookup;

        public CasesController(
            SwimsCasesDbContext cases,
            SwimsDb_moreContext core,
            SwimsIdentityDbContext identity,
            UserManager<SwUser> userManager,
            SwimsLookupDbContext lookup)
        {
            _cases = cases;
            _core = core;
            _identity = identity;
            _userManager = userManager;
            _lookup = lookup;
        }


        // GET: /Cases
        public async Task<IActionResult> Index(string? search, string? status, int? program)
        {
            var query = _cases.SW_cases.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();

                // For v1: search within case fields only (no cross-context joins).
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

            if (program.HasValue && program.Value > 0)
            {
                query = query.Where(c => c.ProgramTagId == program.Value);
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

            var programTagIds = caseEntities
                .Select(c => c.ProgramTagId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var programTags = await _lookup.SW_programTags
                .AsNoTracking()
                .Where(t => programTagIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);

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

                string? programDisplay = null;
                if (c.ProgramTagId.HasValue &&
                    programTags.TryGetValue(c.ProgramTagId.Value, out var tag))
                {
                    programDisplay = tag.name;
                }
                else
                {
                    programDisplay = c.program_tag;
                }

                listItems.Add(new CaseListItemViewModel
                {
                    Id = c.Id,
                    CaseNumber = c.case_number,
                    Title = c.title,
                    Status = c.status,
                    ProgramTag = programDisplay,
                    CreatedAt = c.created_at,
                    CreatedBy = c.created_by,
                    BeneficiaryName = name,
                    BeneficiaryUuid = uuid
                });
            }

            var programOptions = await BuildProgramTagSelectListAsync(program);

            var vm = new CaseIndexViewModel
            {
                Cases = listItems,
                SearchText = search,
                StatusFilter = status,
                ProgramFilter = program,
                ProgramOptions = programOptions
            };

            return View(vm);
        }


        // GET: /Cases/My
        public async Task<IActionResult> My(string? search, string? status)
        {
            // Resolve the current logged-in user so we can match assignments.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var userIdString = user.Id.ToString();

            // Start from cases where this user has an active assignment.
            var query = _cases.SW_cases
                .AsNoTracking()
                .Where(c => _cases.SW_caseAssignments
                    .Any(a => a.SW_caseId == c.Id &&
                              a.user_id == userIdString &&
                              a.is_active));

            // -------------------------------
            // Reuse the same filters as Index
            // -------------------------------
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();

                // For v1: search within case fields only (no cross-context joins).
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

            var programTagIds = caseEntities
                .Select(c => c.ProgramTagId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var programTags = await _lookup.SW_programTags
                .AsNoTracking()
                .Where(t => programTagIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);

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

                string? programDisplay = null;
                if (c.ProgramTagId.HasValue &&
                    programTags.TryGetValue(c.ProgramTagId.Value, out var tag))
                {
                    programDisplay = tag.name;
                }
                else
                {
                    programDisplay = c.program_tag;
                }

                listItems.Add(new CaseListItemViewModel
                {
                    Id = c.Id,
                    CaseNumber = c.case_number,
                    Title = c.title,
                    Status = c.status,
                    ProgramTag = programDisplay,
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


            // Use a dedicated view so the header / filter form can point back to My.
            return View("My", vm);
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
            vm.ProgramOptions = await BuildProgramTagSelectListAsync();

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
                vm.ProgramOptions = await BuildProgramTagSelectListAsync(vm.ProgramTagId);
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
                vm.ProgramOptions = await BuildProgramTagSelectListAsync(vm.ProgramTagId);
                return View(vm);
            }

            var caseTitle = !string.IsNullOrWhiteSpace(beneficiary.name)
                ? beneficiary.name
                : $"{beneficiary.first_name} {beneficiary.last_name}".Trim();

            // Resolve programme from lookup if selected
            string resolvedProgramTagString = "Unspecified";
            int? resolvedProgramTagId = null;

            if (vm.ProgramTagId.HasValue && vm.ProgramTagId.Value > 0)
            {
                var tag = await _lookup.SW_programTags
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == vm.ProgramTagId.Value && t.is_active);

                if (tag != null)
                {
                    resolvedProgramTagId = tag.Id;
                    // Use the stable code as the backing string
                    resolvedProgramTagString = tag.code;
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.ProgramTag))
            {
                // Fallback: if someone typed a value in the legacy string field
                resolvedProgramTagString = vm.ProgramTag.Trim();
            }

            var entity = new SW_case
            {
                case_number = vm.CaseNumber,
                SW_beneficiaryId = vm.SW_beneficiaryId,
                title = caseTitle,
                status = string.IsNullOrWhiteSpace(vm.Status)
                    ? "Pending"
                    : vm.Status.Trim(),
                ProgramTagId = resolvedProgramTagId,
                program_tag = resolvedProgramTagString,
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

            // Load raw assignments from the cases DB
            var assignmentEntities = await _cases.SW_caseAssignments
                .AsNoTracking()
                .Where(a => a.SW_caseId == id)
                .OrderByDescending(a => a.assigned_at)
                .ToListAsync();

            // Extract distinct Identity user ids (stored as string in user_id)
            var userKeyValues = assignmentEntities
                .Select(a => a.user_id)
                .Where(uid => !string.IsNullOrWhiteSpace(uid))
                .Select(uid => int.TryParse(uid, out var parsed) ? (int?)parsed : null)
                .Where(parsed => parsed.HasValue)
                .Select(parsed => parsed!.Value)
                .Distinct()
                .ToList();

            // Pull the corresponding SwUser records
            var users = await _userManager.Users
                .AsNoTracking()
                .Where(u => userKeyValues.Contains(u.Id))
                .ToListAsync();

            // Build a lookup of userId -> display name
            var userDisplayNames = users.ToDictionary(
                u => u.Id,
                u =>
                    !string.IsNullOrWhiteSpace(u.FullName)
                        ? u.FullName
                        : $"{u.FirstName} {u.LastName}".Trim()
            );

            // Build a lookup of userId -> list of system role names
            var rolesByUser = new Dictionary<int, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                rolesByUser[user.Id] = roles;
            }

            // Finally, project into the summary view-model
            var assignments = assignmentEntities
                .Select(a =>
                {
                    var displayName = a.user_id;
                    var systemRoles = string.Empty;

                    if (int.TryParse(a.user_id, out var userIdInt))
                    {
                        if (userDisplayNames.TryGetValue(userIdInt, out var name))
                        {
                            displayName = name;
                        }

                        if (rolesByUser.TryGetValue(userIdInt, out var roleList) && roleList.Count > 0)
                        {
                            systemRoles = string.Join(", ", roleList);
                        }
                    }

                    return new CaseAssignmentSummaryViewModel
                    {
                        Id = a.Id,
                        UserId = a.user_id,
                        UserDisplayName = displayName,
                        SystemRoles = systemRoles,
                        RoleOnCase = a.role_on_case ?? string.Empty,
                        AssignedAt = a.assigned_at,
                        UnassignedAt = a.unassigned_at,
                        IsActive = a.is_active
                    };
                })
                .ToList();


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

        // POST: /Cases/SetStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, string status)
        {
            // For v1 we keep the allowed statuses simple and explicit.
            var allowedStatuses = new[] { "Pending", "Active", "Closed" };

            if (string.IsNullOrWhiteSpace(status) || !allowedStatuses.Contains(status))
            {
                TempData["Ok"] = "Invalid status value.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var caseEntity = await _cases.SW_cases
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
            {
                return NotFound();
            }

            if (caseEntity.status == status)
            {
                TempData["Ok"] = $"Case is already marked as {status}.";
                return RedirectToAction(nameof(Details), new { id });
            }

            caseEntity.status = status;

            // Keep closed_at in sync with the flag in a very simple way for v1:
            if (status == "Closed")
            {
                // Only set if it's not already set, so we preserve original close date.
                if (caseEntity.closed_at == null)
                {
                    caseEntity.closed_at = DateTime.UtcNow;
                }
            }
            else
            {
                // Any non-closed status clears the closed_at timestamp.
                caseEntity.closed_at = null;
            }

            await _cases.SaveChangesAsync();

            TempData["Ok"] = $"Case status updated to {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }


        // GET: /Cases/LinkForm/5
        public async Task<IActionResult> LinkForm(int id)
        {
            var caseEntity = await _cases.SW_cases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
            {
                return NotFound();
            }

            var vm = new CaseLinkFormViewModel
            {
                SW_caseId = caseEntity.Id,
                CaseNumber = caseEntity.case_number,
                CaseTitle = caseEntity.title,
                AvailableForms = await BuildAvailableFormsSelectListAsync()
            };

            return View(vm);
        }

        // POST: /Cases/LinkForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkForm(CaseLinkFormViewModel vm)
        {
            var caseEntity = await _cases.SW_cases
                .FirstOrDefaultAsync(c => c.Id == vm.SW_caseId);

            if (caseEntity == null)
            {
                return NotFound();
            }

            // First: basic validation (did they pick something at all?)
            if (!ModelState.IsValid)
            {
                vm.AvailableForms = await BuildAvailableFormsSelectListAsync();
                vm.CaseNumber = caseEntity.case_number;
                vm.CaseTitle = caseEntity.title;
                return View(vm);
            }

            // Ensure the selected form entry actually exists
            var formEntry = await _core.SW_formTableData
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == vm.SelectedFormTableDatumId);

            if (formEntry == null)
            {
                ModelState.AddModelError(nameof(vm.SelectedFormTableDatumId),
                    "Selected form submission was not found.");
                vm.AvailableForms = await BuildAvailableFormsSelectListAsync();
                vm.CaseNumber = caseEntity.case_number;
                vm.CaseTitle = caseEntity.title;
                return View(vm);
            }

            // Avoid duplicate link for the same case + form submission
            var alreadyLinked = await _cases.SW_caseForms
                .AnyAsync(cf =>
                    cf.SW_caseId == vm.SW_caseId &&
                    cf.SW_formTableDatumId == vm.SelectedFormTableDatumId);

            if (!alreadyLinked)
            {
                // If this one is primary, clear any existing primary for this case
                if (vm.IsPrimaryApplication)
                {
                    var existingPrimaries = await _cases.SW_caseForms
                        .Where(cf => cf.SW_caseId == vm.SW_caseId && cf.is_primary_application)
                        .ToListAsync();

                    foreach (var cf in existingPrimaries)
                    {
                        cf.is_primary_application = false;
                    }
                }

                var link = new SW_caseForm
                {
                    SW_caseId = vm.SW_caseId,
                    SW_formTableDatumId = vm.SelectedFormTableDatumId,
                    form_role = string.IsNullOrWhiteSpace(vm.FormRole)
                        ? null
                        : vm.FormRole.Trim(),
                    is_primary_application = vm.IsPrimaryApplication,
                    linked_at = DateTime.UtcNow,
                    linked_by = User?.Identity?.Name
                };

                _cases.SW_caseForms.Add(link);
                await _cases.SaveChangesAsync();
            }

            TempData["Ok"] = "Form linked to case successfully.";
            return RedirectToAction(nameof(Details), new { id = vm.SW_caseId });
        }

        // GET: /Cases/FormPreview/5
        [HttpGet]
        public async Task<IActionResult> FormPreview(int id)
        {
            // id = SW_caseForm.Id
            var link = await _cases.SW_caseForms
                .AsNoTracking()
                .FirstOrDefaultAsync(cf => cf.Id == id);

            if (link == null)
            {
                return NotFound("Linked form record not found for this case.");
            }

            // Look up the underlying form submission
            var formEntry = await _core.SW_formTableData
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == link.SW_formTableDatumId);

            if (formEntry == null)
            {
                return NotFound("Underlying form submission could not be found.");
            }

            // Resolve the parent form to get its UUID
            var swForm = await _core.SW_forms
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == formEntry.SW_formsId);

            if (swForm == null || string.IsNullOrWhiteSpace(swForm.uuid))
            {
                return NotFound("Parent form definition for this submission could not be found.");
            }

            // Reuse the existing program-side Preview page
            return RedirectToAction(
                actionName: "Preview",
                controllerName: "form",
                routeValues: new { dataID = formEntry.Id, uuid = swForm.uuid }
            );
        }

        // GET: /Cases/FormEdit/5
        [HttpGet]
        public async Task<IActionResult> FormEdit(int id)
        {
            // id = SW_caseForm.Id
            var link = await _cases.SW_caseForms
                .AsNoTracking()
                .FirstOrDefaultAsync(cf => cf.Id == id);

            if (link == null)
            {
                return NotFound("Linked form record not found for this case.");
            }

            // Look up the underlying form submission
            var formEntry = await _core.SW_formTableData
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == link.SW_formTableDatumId);

            if (formEntry == null)
            {
                return NotFound("Underlying form submission could not be found.");
            }

            // Resolve the parent form to get its UUID
            var swForm = await _core.SW_forms
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == formEntry.SW_formsId);

            if (swForm == null || string.IsNullOrWhiteSpace(swForm.uuid))
            {
                return NotFound("Parent form definition for this submission could not be found.");
            }

            // Reuse the existing program-side Update page
            return RedirectToAction(
                actionName: "Update",
                controllerName: "form",
                routeValues: new { dataID = formEntry.Id, uuid = swForm.uuid }
            );
        }


        // GET: /Cases/Assign/5
        [Authorize] // we can refine to a Case.Manage policy later
        public async Task<IActionResult> Assign(int id)
        {
            var @case = await _cases.SW_cases.FindAsync(id);
            if (@case == null)
            {
                return NotFound();
            }

            // For now: all users, ordered by name. Later we can filter to specific roles.
            var users = await _identity.SwUsers
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            var vm = new CaseAssignViewModel
            {
                CaseId = @case.Id,
                CaseNumber = @case.case_number,
                CaseTitle = @case.title,
                AvailableUsers = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(u.FullName)
                        ? (u.UserName ?? $"User {u.Id}")
                        : u.FullName
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // same as above
        public async Task<IActionResult> Assign(CaseAssignViewModel model)
        {
            var @case = await _cases.SW_cases.FindAsync(model.CaseId);
            if (@case == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Reload users for the dropdown
                var users = await _identity.SwUsers
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();

                model.CaseNumber = @case.case_number;
                model.CaseTitle = @case.title;
                model.AvailableUsers = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(u.FullName)
                        ? (u.UserName ?? $"User {u.Id}")
                        : u.FullName
                }).ToList();

                return View(model);
            }

            var assignment = new SW_caseAssignment
            {
                SW_caseId = model.CaseId,
                user_id = model.UserId,
                role_on_case = string.IsNullOrWhiteSpace(model.RoleOnCase)
                    ? "Social worker"
                    : model.RoleOnCase,
                assigned_at = DateTime.UtcNow,
                is_active = model.IsActive,
                // unassigned_at stays null for a new assignment
            };

            _cases.SW_caseAssignments.Add(assignment);
            await _cases.SaveChangesAsync();

            TempData["Ok"] = "Staff member assigned to case.";

            return RedirectToAction(nameof(Details), new { id = model.CaseId });
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

        private async Task<List<SelectListItem>> BuildProgramTagSelectListAsync(int? selectedId = null)
        {
            var tags = await _lookup.SW_programTags
                .AsNoTracking()
                .Where(t => t.is_active)
                .OrderBy(t => t.sort_order)
                .ThenBy(t => t.name)
                .ToListAsync();

            return tags
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.name, // e.g. "Public Assistance"
                    Selected = selectedId.HasValue && selectedId.Value == t.Id
                })
                .ToList();
        }


        private async Task<List<SelectListItem>> BuildAvailableFormsSelectListAsync()
        {
            // For v1: show the most recent 50 form submissions across all forms.
            var query = _core.SW_formTableData
                .AsNoTracking()
                .OrderByDescending(d => d.Id)
                .Take(50)
                .Join(
                    _core.SW_forms.AsNoTracking(),
                    d => d.SW_formsId,
                    f => f.Id,
                    (d, f) => new { Datum = d, Form = f });

            var forms = await query.ToListAsync();

            return forms
                .Select(x =>
                {
                    var label = !string.IsNullOrWhiteSpace(x.Form.name)
                        ? x.Form.name
                        : x.Form.uuid;

                    return new SelectListItem
                    {
                        Value = x.Datum.Id.ToString(),
                        Text = $"{x.Datum.Id} - {label}"
                    };
                })
                .ToList();
        }
    }
}
