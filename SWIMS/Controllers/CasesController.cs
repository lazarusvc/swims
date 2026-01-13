using HandlebarsDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Data.Cases;
using SWIMS.Data.Lookups;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using SWIMS.Services.Cases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Globalization;




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
        private readonly ICaseLifecycleService _caseLifecycle;

        public CasesController(
            SwimsCasesDbContext cases,
            SwimsDb_moreContext core,
            SwimsIdentityDbContext identity,
            UserManager<SwUser> userManager,
            SwimsLookupDbContext lookup,
            ICaseLifecycleService caseLifecycle)
        {
            _cases = cases;
            _core = core;
            _identity = identity;
            _userManager = userManager;
            _lookup = lookup;
            _caseLifecycle = caseLifecycle;
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

        // GET: /Cases/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var caseEntity = await _cases.SW_cases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
            {
                return NotFound();
            }

            var vm = new CaseCreateViewModel
            {
                Id = caseEntity.Id,
                CaseNumber = caseEntity.case_number,
                SW_beneficiaryId = caseEntity.SW_beneficiaryId,
                Title = caseEntity.title,
                Status = caseEntity.status ?? "Pending",
                ProgramTag = caseEntity.program_tag,
                ProgramTagId = caseEntity.ProgramTagId,
                Notes = caseEntity.notes
            };

            vm.Beneficiaries = await BuildBeneficiarySelectListAsync();
            vm.ProgramOptions = await BuildProgramTagSelectListAsync(caseEntity.ProgramTagId);

            return View(vm);
        }

        // POST: /Cases/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CaseCreateViewModel vm)
        {
            if (id != vm.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                vm.Beneficiaries = await BuildBeneficiarySelectListAsync();
                vm.ProgramOptions = await BuildProgramTagSelectListAsync(vm.ProgramTagId);
                return View(vm);
            }

            var caseEntity = await _cases.SW_cases
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
            {
                return NotFound();
            }

            // Make sure beneficiary is valid and get a fresh copy for naming
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

            // Start from existing programme values so we don't accidentally wipe them
            string resolvedProgramTagString = caseEntity.program_tag;
            int? resolvedProgramTagId = caseEntity.ProgramTagId;

            if (vm.ProgramTagId.HasValue && vm.ProgramTagId.Value > 0)
            {
                var tag = await _lookup.SW_programTags
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == vm.ProgramTagId.Value && t.is_active);

                if (tag != null)
                {
                    resolvedProgramTagId = tag.Id;
                    // keep using the stable code as backing string
                    resolvedProgramTagString = tag.code;
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.ProgramTag))
            {
                resolvedProgramTagString = vm.ProgramTag.Trim();
                resolvedProgramTagId = null;
            }

            // Apply updates
            caseEntity.case_number = vm.CaseNumber;
            caseEntity.SW_beneficiaryId = vm.SW_beneficiaryId;
            caseEntity.title = caseTitle;
            caseEntity.status = string.IsNullOrWhiteSpace(vm.Status)
                ? "Pending"
                : vm.Status.Trim();
            caseEntity.ProgramTagId = resolvedProgramTagId;
            caseEntity.program_tag = resolvedProgramTagString;
            caseEntity.notes = string.IsNullOrWhiteSpace(vm.Notes)
                ? null
                : vm.Notes.Trim();

            await _cases.SaveChangesAsync();

            TempData["Ok"] = "Case updated successfully.";
            return RedirectToAction(nameof(Details), new { id = caseEntity.Id });
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

            // -------------------------------
            // Enrich linked case forms with:
            // FormName, SiteIdentityName, FormTypeName, ProgramTags
            // -------------------------------
            var formDataIds = forms.Select(x => x.FormTableDataId).Distinct().ToList();

            var formIdByFormDataId = await _core.SW_formTableData
                .AsNoTracking()
                .Where(d => formDataIds.Contains(d.Id))
                .Select(d => new { d.Id, d.SW_formsId })
                .ToDictionaryAsync(x => x.Id, x => x.SW_formsId);

            var formIds = formIdByFormDataId.Values.Distinct().ToList();

            var formMetaById = await _core.SW_forms
                .AsNoTracking()
                .Include(f => f.SW_identity)
                .Where(f => formIds.Contains(f.Id))
                .Select(f => new
                {
                    f.Id,
                    FormName = f.name,
                    SiteIdentityName = f.SW_identity != null ? f.SW_identity.name : null
                })
                .ToDictionaryAsync(x => x.Id);

            var formTypeByFormId = await _lookup.SW_formFormTypes
                .AsNoTracking()
                .Where(x => formIds.Contains(x.SW_formsId))
                .Join(_lookup.SW_formTypes.AsNoTracking(),
                    link => link.SW_formTypeId,
                    t => t.Id,
                    (link, t) => new { link.SW_formsId, TypeName = t.name })
                .ToDictionaryAsync(x => x.SW_formsId, x => x.TypeName);

            var programTagsByFormId = await _lookup.SW_formProgramTags
                .AsNoTracking()
                .Where(x => formIds.Contains(x.SW_formsId))
                .Join(_lookup.SW_programTags.AsNoTracking(),
                    link => link.SW_programTagId,
                    tag => tag.Id,
                    (link, tag) => new { link.SW_formsId, TagName = tag.name })
                .GroupBy(x => x.SW_formsId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(x => x.TagName).Distinct().OrderBy(n => n).ToList()
                );

            foreach (var row in forms)
            {
                if (!formIdByFormDataId.TryGetValue(row.FormTableDataId, out var formId))
                    continue;

                row.FormId = formId;

                if (formMetaById.TryGetValue(formId, out var meta))
                {
                    row.FormName = meta.FormName;
                    row.SiteIdentityName = meta.SiteIdentityName;
                }

                if (formTypeByFormId.TryGetValue(formId, out var typeName))
                    row.FormTypeName = typeName;

                if (programTagsByFormId.TryGetValue(formId, out var tags))
                    row.ProgramTagNames = tags;
            }



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
                BenefitStartAt = caseEntity.benefit_start_at,
                BenefitEndAt = caseEntity.benefit_end_at,
                BenefitPeriodMonths = caseEntity.benefit_period_months,
                BenefitPeriodSource = caseEntity.benefit_period_source,

                BenefitStartAtOverride = caseEntity.benefit_start_at_override,
                BenefitEndAtOverride = caseEntity.benefit_end_at_override,
                BenefitPeriodMonthsOverride = caseEntity.benefit_period_months_override,

                StatusOverride = caseEntity.status_override,
                StatusOverrideReason = caseEntity.status_override_reason,
                StatusOverrideUntil = caseEntity.status_override_until,
                StatusOverrideAt = caseEntity.status_override_at,
                StatusOverrideBy = caseEntity.status_override_by,

                IsStatusOverrideActive =
                    !string.IsNullOrWhiteSpace(caseEntity.status_override)
                    && (caseEntity.status_override_until == null || caseEntity.status_override_until > DateTime.UtcNow),


                BeneficiaryName = beneficiary != null
                    ? (!string.IsNullOrWhiteSpace(beneficiary.name)
                        ? beneficiary.name
                        : $"{beneficiary.first_name} {beneficiary.last_name}".Trim())
                    : "(no beneficiary)",
                BeneficiaryUuid = beneficiary?.uuid ?? string.Empty,
                BeneficiaryId = caseEntity.SW_beneficiaryId,
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
        public async Task<IActionResult> SetStatus(int id, string status, string? reason = null)
        {
            // Keep explicit for now
            var allowedStatuses = new[] { "Pending", "Active", "Inactive", "Closed", "Suspended" };

            if (string.IsNullOrWhiteSpace(status) || !allowedStatuses.Contains(status))
            {
                TempData["Ok"] = "Invalid status value.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var caseEntity = await _cases.SW_cases.FirstOrDefaultAsync(c => c.Id == id);
            if (caseEntity == null)
                return NotFound();

            // Manual override: set both the visible status + override fields
            var now = DateTime.UtcNow;

            caseEntity.status = status;

            caseEntity.status_override = status;
            caseEntity.status_override_reason = string.IsNullOrWhiteSpace(reason) ? "Manual override" : reason.Trim();
            caseEntity.status_override_at = now;
            caseEntity.status_override_by = User?.Identity?.Name ?? "(unknown)";
            caseEntity.status_override_until = null; // plan-ahead: later we’ll support timed suspensions

            // Keep closed_at consistent with the *manual* status
            if (status == "Closed")
            {
                if (caseEntity.closed_at == null)
                    caseEntity.closed_at = now;
            }
            else
            {
                caseEntity.closed_at = null;
            }

            await _cases.SaveChangesAsync();

            TempData["Ok"] = $"Case status manually overridden to {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }


        // POST: /Cases/ClearStatusOverride
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearStatusOverride(int id)
        {
            var caseEntity = await _cases.SW_cases.FirstOrDefaultAsync(c => c.Id == id);
            if (caseEntity == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(caseEntity.status_override))
            {
                TempData["Ok"] = "No manual status override is active for this case.";
                return RedirectToAction(nameof(Details), new { id });
            }

            caseEntity.status_override = null;
            caseEntity.status_override_reason = null;
            caseEntity.status_override_until = null;
            caseEntity.status_override_at = null;
            caseEntity.status_override_by = null;

            await _cases.SaveChangesAsync();

            // Recompute back to automatic behavior (best-effort)
            try
            {
                var userId = _userManager.GetUserId(User) ?? User?.Identity?.Name;
                var result = await _caseLifecycle.RefreshFromPrimaryApplicationAsync(id, userId);

                // Changed = whether anything needed to update; not a "success/fail" flag
                TempData["Ok"] = $"Manual override cleared. {result.Message}";
            }
            catch
            {
                TempData["Ok"] = "Manual override cleared. Automatic recompute failed; use Refresh to recompute.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }




        // GET: /Cases/LinkForm/5
        [HttpGet]
        public async Task<IActionResult> LinkForm(int id)
        {
            var caseEntity = await _cases.SW_cases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
                return NotFound();

            var beneficiaryUuid = await _core.SW_beneficiaries
                .AsNoTracking()
                .Where(b => b.Id == caseEntity.SW_beneficiaryId)
                .Select(b => b.uuid)
                .FirstOrDefaultAsync();

            ViewBag.FormScopeNote = !string.IsNullOrWhiteSpace(beneficiaryUuid)
                ? "Showing submissions for the selected beneficiary only."
                : "No beneficiary selected on this case — showing all submissions.";

            // We are hiding/parking auto-linking for now.
            ViewBag.AutoLinkingEnabled = AutoLinkingEnabled;

            var vm = new CaseLinkFormViewModel
            {
                SW_caseId = caseEntity.Id,
                CaseNumber = caseEntity.case_number,
                CaseTitle = caseEntity.title,
                IsPrimaryApplication = false,
                IncludeLinkedForms = false
            };

            vm.AvailableForms = await BuildAvailableFormsSelectListAsync(beneficiaryUuid, caseEntity.SW_beneficiaryId);

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkForm(int id, CaseLinkFormViewModel vm)
        {
            if (id != vm.SW_caseId)
                return BadRequest();

            var caseEntity = await _cases.SW_cases
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
                return NotFound();

            // Force-disable auto linking no matter what comes from the UI.
            vm.IncludeLinkedForms = false;

            if (!ModelState.IsValid || vm.SelectedFormTableDatumId == null)
            {
                var beneficiaryUuid0 = await _core.SW_beneficiaries
                    .AsNoTracking()
                    .Where(b => b.Id == caseEntity.SW_beneficiaryId)
                    .Select(b => b.uuid)
                    .FirstOrDefaultAsync();

                ViewBag.FormScopeNote = !string.IsNullOrWhiteSpace(beneficiaryUuid0)
                    ? "Showing submissions for the selected beneficiary only."
                    : "No beneficiary selected on this case — showing all submissions.";

                ViewBag.AutoLinkingEnabled = AutoLinkingEnabled;

                vm.CaseNumber = caseEntity.case_number;
                vm.CaseTitle = caseEntity.title;
                vm.AvailableForms = await BuildAvailableFormsSelectListAsync(beneficiaryUuid0, caseEntity.SW_beneficiaryId);
                return View(vm);
            }

            var formData = await _core.SW_formTableData
                .Include(d => d.SW_forms)
                .FirstOrDefaultAsync(d => d.Id == vm.SelectedFormTableDatumId.Value);

            if (formData == null)
            {
                ModelState.AddModelError(nameof(vm.SelectedFormTableDatumId),
                    "The selected form submission no longer exists.");

                var beneficiaryUuid1 = await _core.SW_beneficiaries
                    .AsNoTracking()
                    .Where(b => b.Id == caseEntity.SW_beneficiaryId)
                    .Select(b => b.uuid)
                    .FirstOrDefaultAsync();

                ViewBag.FormScopeNote = !string.IsNullOrWhiteSpace(beneficiaryUuid1)
                    ? "Showing submissions for the selected beneficiary only."
                    : "No beneficiary selected on this case — showing all submissions.";

                ViewBag.AutoLinkingEnabled = AutoLinkingEnabled;

                vm.CaseNumber = caseEntity.case_number;
                vm.CaseTitle = caseEntity.title;
                vm.AvailableForms = await BuildAvailableFormsSelectListAsync(beneficiaryUuid1, caseEntity.SW_beneficiaryId);
                return View(vm);
            }

            // Prevent duplicate link (same submission linked twice to same case).
            var alreadyLinked = await _cases.SW_caseForms
                .AnyAsync(x => x.SW_caseId == caseEntity.Id && x.SW_formTableDatumId == formData.Id);

            if (alreadyLinked)
            {
                ModelState.AddModelError(nameof(vm.SelectedFormTableDatumId),
                    "That form submission is already linked to this case.");

                var beneficiaryUuid2 = await _core.SW_beneficiaries
                    .AsNoTracking()
                    .Where(b => b.Id == caseEntity.SW_beneficiaryId)
                    .Select(b => b.uuid)
                    .FirstOrDefaultAsync();

                ViewBag.FormScopeNote = !string.IsNullOrWhiteSpace(beneficiaryUuid2)
                    ? "Showing submissions for the selected beneficiary only."
                    : "No beneficiary selected on this case — showing all submissions.";

                ViewBag.AutoLinkingEnabled = AutoLinkingEnabled;

                vm.CaseNumber = caseEntity.case_number;
                vm.CaseTitle = caseEntity.title;
                vm.AvailableForms = await BuildAvailableFormsSelectListAsync(beneficiaryUuid2, caseEntity.SW_beneficiaryId);
                return View(vm);
            }

            // Determine the form type name (role) using lookup relationship (SW_formFormTypes -> SW_formTypes)
            var formTypeName = await _lookup.SW_formFormTypes
                .AsNoTracking()
                .Where(x => x.SW_formsId == formData.SW_formsId)
                .Join(_lookup.SW_formTypes.AsNoTracking(),
                    link => link.SW_formTypeId,
                    t => t.Id,
                    (link, t) => t.name)
                .FirstOrDefaultAsync();

            // Store it as the "role" label on the case link
            formTypeName = string.IsNullOrWhiteSpace(formTypeName) ? null : formTypeName.Trim();


            await using var tx = await _cases.Database.BeginTransactionAsync();

            try
            {
                // Primary uniqueness enforcement:
                // If user is linking a new Primary Application, clear any existing primaries FIRST and save,
                // so the DB unique filtered index never sees 2 primaries in the same case.
                if (vm.IsPrimaryApplication)
                {
                    await ClearOtherPrimaryApplicationsAsync(caseEntity.Id);
                    await _cases.SaveChangesAsync();
                }

                var link = new SW_caseForm
                {
                    SW_caseId = caseEntity.Id,
                    SW_formTableDatumId = formData.Id,
                    form_role = formTypeName,
                    is_primary_application = vm.IsPrimaryApplication,
                    linked_at = DateTime.UtcNow,
                    linked_by = _userManager.GetUserId(User)
                };

                _cases.SW_caseForms.Add(link);
                await _cases.SaveChangesAsync();

                // Auto-linking is parked for now.
                if (AutoLinkingEnabled && vm.IncludeLinkedForms)
                {
                    await AttachLinkedFormsAsync(caseEntity.Id, formData.Id, formTypeName);
                    await _cases.SaveChangesAsync();
                }

                if (vm.IsPrimaryApplication)
                {
                    string? userId = _userManager.GetUserId(User);
                    var result = await _caseLifecycle.RefreshFromPrimaryApplicationAsync(caseEntity.Id, userId);

                    TempData["Ok"] = $"Primary application linked. {result.Message}";
                }
                else
                {
                    TempData["Ok"] = "Form submission linked to case.";
                }

                await tx.CommitAsync();
                return RedirectToAction(nameof(Details), new { id = caseEntity.Id });
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshFromPrimaryApplication(int id)
        {
            var hasPrimary = await _cases.SW_caseForms
                .AsNoTracking()
                .AnyAsync(x => x.SW_caseId == id && x.is_primary_application);

            if (!hasPrimary)
            {
                TempData["Err"] = "No Primary Application is linked to this case yet. Link a Primary Application first, then recalculate.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var userId = _userManager.GetUserId(User);
            var result = await _caseLifecycle.RefreshFromPrimaryApplicationAsync(id, userId);
            TempData["Ok"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetachForm(int id)
        {
            var link = await _cases.SW_caseForms
                .FirstOrDefaultAsync(cf => cf.Id == id);

            if (link == null)
            {
                return NotFound();
            }

            var caseId = link.SW_caseId;

            _cases.SW_caseForms.Remove(link);
            await _cases.SaveChangesAsync();

            TempData["Ok"] = "Form detached from case. The form submission itself is unchanged.";
            return RedirectToAction(nameof(Details), new { id = caseId });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignWorker(int id)
        {
            var assignment = await _cases.SW_caseAssignments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            var caseId = assignment.SW_caseId;

            if (!assignment.is_active)
            {
                TempData["Ok"] = "This worker is already removed from the case.";
                return RedirectToAction(nameof(Details), new { id = caseId });
            }

            assignment.is_active = false;
            assignment.unassigned_at = DateTime.Now; // matches your other timestamps

            await _cases.SaveChangesAsync();

            TempData["Ok"] = "Worker removed from case.";
            return RedirectToAction(nameof(Details), new { id = caseId });
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



        // ✅ REPLACE ENTIRE METHOD
        private static readonly PropertyInfo[] _formDataProps =
            typeof(SW_formTableDatum)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name.StartsWith("FormData", StringComparison.OrdinalIgnoreCase))
                .ToArray();

        private static string NormalizeRaw(string raw)
        {
            raw = raw.Trim();

            // Handle cases where some older UI stored "id,uuid,name"
            if (raw.Contains(','))
            {
                var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length >= 2) return parts[1]; // uuid
                if (parts.Length == 1) return parts[0];
            }

            return raw;
        }

        private bool RowMatchesBeneficiary(SW_formTableDatum row, string? beneficiaryUuid, int? beneficiaryId)
        {
            var benUuid = string.IsNullOrWhiteSpace(beneficiaryUuid) ? null : beneficiaryUuid.Trim();
            var benId = beneficiaryId.HasValue ? beneficiaryId.Value.ToString() : null;

            foreach (var prop in _formDataProps)
            {
                var rawObj = prop.GetValue(row);
                if (rawObj == null) continue;

                var raw = rawObj.ToString();
                if (string.IsNullOrWhiteSpace(raw)) continue;

                var normalized = NormalizeRaw(raw);

                if (benUuid != null &&
                    string.Equals(normalized, benUuid, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (benId != null &&
                    string.Equals(normalized, benId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private async Task<List<SelectListItem>> BuildAvailableFormsSelectListAsync(string? beneficiaryUuid, int? beneficiaryId)
        {
            var strictScope = !string.IsNullOrWhiteSpace(beneficiaryUuid) || (beneficiaryId.HasValue && beneficiaryId.Value > 0);

            // Keep bounded (adjust if needed)
            var candidates = await _core.SW_formTableData
                .AsNoTracking()
                .Include(d => d.SW_forms)
                .OrderByDescending(d => d.Id)
                .Take(750)
                .ToListAsync();

            if (strictScope)
            {
                candidates = candidates
                    .Where(d => RowMatchesBeneficiary(d, beneficiaryUuid, beneficiaryId))
                    .ToList();
            }

            // --- NEW: extract beneficiary tokens from each row so we can display the name ---
            var tokenBySubmissionId = new Dictionary<int, (int? id, string? uuid, string? inlineName)>();
            var idKeys = new HashSet<int>();
            var uuidKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in candidates)
            {
                var token = ExtractBeneficiaryToken(row);
                tokenBySubmissionId[row.Id] = token;

                if (token.id.HasValue) idKeys.Add(token.id.Value);
                if (!string.IsNullOrWhiteSpace(token.uuid)) uuidKeys.Add(token.uuid);
            }

            // Resolve beneficiary names from DB (only for tokens we found)
            string MakeName(SW_beneficiary b) =>
                !string.IsNullOrWhiteSpace(b.name)
                    ? b.name
                    : $"{b.first_name} {b.last_name}".Trim();

            var benById = idKeys.Count == 0
                ? new Dictionary<int, string>()
                : await _core.SW_beneficiaries
                    .AsNoTracking()
                    .Where(b => idKeys.Contains(b.Id))
                    .ToDictionaryAsync(b => b.Id, b => MakeName(b));

            var benByUuid = uuidKeys.Count == 0
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : await _core.SW_beneficiaries
                    .AsNoTracking()
                    .Where(b => b.uuid != null && uuidKeys.Contains(b.uuid))
                    .ToDictionaryAsync(b => b.uuid!, b => MakeName(b), StringComparer.OrdinalIgnoreCase);

            var items = new List<SelectListItem>();

            foreach (var row in candidates)
            {
                var formName = row.SW_forms?.name ?? $"Form {row.SW_formsId}";

                // --- NEW: build beneficiary label ---
                tokenBySubmissionId.TryGetValue(row.Id, out var token);

                string? benName = null;

                // Prefer inline name if it exists (id,uuid,name format)
                if (!string.IsNullOrWhiteSpace(token.inlineName))
                    benName = token.inlineName;
                else if (token.id.HasValue && benById.TryGetValue(token.id.Value, out var byIdName))
                    benName = byIdName;
                else if (!string.IsNullOrWhiteSpace(token.uuid) && benByUuid.TryGetValue(token.uuid, out var byUuidName))
                    benName = byUuidName;

                var benLabel = benName
                    ?? token.uuid
                    ?? (token.id?.ToString())
                    ?? "Unknown";

                items.Add(new SelectListItem
                {
                    Value = row.Id.ToString(),
                    Text = $"{formName} • Submission #{row.Id} • Beneficiary: {benLabel}"
                });
            }

            if (items.Count == 0)
            {
                items.Add(new SelectListItem
                {
                    Value = "",
                    Text = strictScope ? "(No submissions found for this beneficiary)" : "(No submissions found)",
                    Disabled = true
                });
            }

            return items;
        }


        // ✅ ADD THIS METHOD (it can attach nothing if linking data isn't ready — that's fine)
        private async Task AttachLinkedFormsAsync(int caseId, int rootSubmissionId, string? rootRole)
        {
            var root = await _core.SW_formTableData
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == rootSubmissionId);

            if (root == null) return;

            // Convention:
            // - children store a group key in isLinkingForm
            // - or children store root.Id.ToString() in isLinkingForm
            var groupKey = !string.IsNullOrWhiteSpace(root.isLinkingForm)
                ? root.isLinkingForm.Trim()
                : rootSubmissionId.ToString();

            var ids = new HashSet<int> { rootSubmissionId };

            // Add all submissions that point at this group key
            var linkedIds = await _core.SW_formTableData
                .AsNoTracking()
                .Where(d => d.isLinkingForm == groupKey)
                .Select(d => d.Id)
                .ToListAsync();

            foreach (var x in linkedIds) ids.Add(x);

            // If groupKey is numeric, include that as a possible group-root submission
            if (int.TryParse(groupKey, out var groupRootId))
                ids.Add(groupRootId);

            if (ids.Count <= 1) return;

            // Existing links for this case
            var existing = await _cases.SW_caseForms
                .AsNoTracking()
                .Where(cf => cf.SW_caseId == caseId)
                .Select(cf => cf.SW_formTableDatumId)
                .ToHashSetAsync();

            // Root is being added in-memory in the action (not saved yet), treat as linked
            existing.Add(rootSubmissionId);

            var linkedRole = string.IsNullOrWhiteSpace(rootRole)
                ? "Linked"
                : $"Linked: {rootRole.Trim()}";

            var linkedBy = _userManager.GetUserId(User);

            foreach (var submissionId in ids)
            {
                if (existing.Contains(submissionId)) continue;

                _cases.SW_caseForms.Add(new SW_caseForm
                {
                    SW_caseId = caseId,
                    SW_formTableDatumId = submissionId,
                    form_role = linkedRole,
                    is_primary_application = false,
                    linked_at = DateTime.UtcNow,
                    linked_by = linkedBy
                });
            }
        }

        private static bool LooksLikeGuid(string s) => Guid.TryParse(s, out _);

        private static (int? id, string? uuid, string? inlineName) ExtractBeneficiaryToken(SW_formTableDatum row)
        {
            foreach (var prop in _formDataProps)
            {
                var rawObj = prop.GetValue(row);
                if (rawObj == null) continue;

                var raw = rawObj.ToString();
                if (string.IsNullOrWhiteSpace(raw)) continue;

                raw = raw.Trim();

                // If stored as "id,uuid,name" (or similar), parse that first
                if (raw.Contains(','))
                {
                    var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    int? id = null;
                    string? uuid = null;
                    string? name = null;

                    if (parts.Length >= 1 && int.TryParse(parts[0], out var parsedId))
                        id = parsedId;

                    if (parts.Length >= 2 && LooksLikeGuid(parts[1]))
                        uuid = parts[1];

                    if (parts.Length >= 3)
                        name = parts[2];

                    if (id.HasValue || uuid != null || name != null)
                        return (id, uuid, name);
                }

                var normalized = NormalizeRaw(raw);

                if (LooksLikeGuid(normalized))
                    return (null, normalized, null);

                if (int.TryParse(normalized, out var parsed))
                    return (parsed, null, null);
            }

            return (null, null, null);
        }

        // GET: /Cases/BenefitPeriod/5
        [HttpGet]
        public async Task<IActionResult> BenefitPeriod(int id)
        {
            var caseEntity = await _cases.SW_cases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
                return NotFound();

            var vm = new CaseBenefitPeriodEditViewModel
            {
                Id = id,
                CaseNumber = GetString(caseEntity, "case_number") ?? id.ToString(CultureInfo.InvariantCulture),
                Title = GetString(caseEntity, "title") ?? string.Empty,
                Status = GetString(caseEntity, "status") ?? "Pending",

                BenefitStartAt = GetDateTime(caseEntity, "benefit_start_at"),
                BenefitEndAt = GetDateTime(caseEntity, "benefit_end_at"),
                BenefitPeriodMonths = GetInt(caseEntity, "benefit_period_months"),
                BenefitPeriodSource = GetString(caseEntity, "benefit_period_source"),

                BenefitStartAtOverride = GetDateTime(caseEntity, "benefit_start_at_override"),
                BenefitEndAtOverride = GetDateTime(caseEntity, "benefit_end_at_override"),
                BenefitPeriodMonthsOverride = GetInt(caseEntity, "benefit_period_months_override"),
            };

            return View(vm);
        }

        // POST: /Cases/BenefitPeriod/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BenefitPeriod(int id, CaseBenefitPeriodEditViewModel vm)
        {
            if (id != vm.Id)
                return BadRequest();

            var caseEntity = await _cases.SW_cases
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caseEntity == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Re-hydrate effective values for redisplay
                vm.CaseNumber = GetString(caseEntity, "case_number") ?? vm.CaseNumber;
                vm.Title = GetString(caseEntity, "title") ?? vm.Title;
                vm.Status = GetString(caseEntity, "status") ?? vm.Status;

                vm.BenefitStartAt = GetDateTime(caseEntity, "benefit_start_at");
                vm.BenefitEndAt = GetDateTime(caseEntity, "benefit_end_at");
                vm.BenefitPeriodMonths = GetInt(caseEntity, "benefit_period_months");
                vm.BenefitPeriodSource = GetString(caseEntity, "benefit_period_source");

                return View(vm);
            }

            if (vm.ClearOverrides)
            {
                SetValue(caseEntity, "benefit_start_at_override", null);
                SetValue(caseEntity, "benefit_end_at_override", null);
                SetValue(caseEntity, "benefit_period_months_override", null);
            }
            else
            {
                SetValue(caseEntity, "benefit_start_at_override", vm.BenefitStartAtOverride);
                SetValue(caseEntity, "benefit_end_at_override", vm.BenefitEndAtOverride);
                SetValue(caseEntity, "benefit_period_months_override", vm.BenefitPeriodMonthsOverride);
            }

            await _cases.SaveChangesAsync();

            // Re-run lifecycle refresh so effective fields + status align with overrides
            var lifecycle = HttpContext.RequestServices.GetService(typeof(ICaseLifecycleService)) as ICaseLifecycleService;
            if (lifecycle != null)
            {
                await lifecycle.RefreshFromPrimaryApplicationAsync(id, triggeredByUserId: User?.Identity?.Name);
            }

            TempData["Ok"] = "Benefit period overrides saved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private static PropertyInfo? GetProp(object o, string name)
            => o.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        private static string? GetString(object o, string name)
        {
            var p = GetProp(o, name);
            return p?.GetValue(o) as string;
        }

        private static int? GetInt(object o, string name)
        {
            var p = GetProp(o, name);
            var v = p?.GetValue(o);

            if (v is null) return null;

            // Nullable<int> boxes as int when not null
            if (v is int i) return i;

            if (v is long l && l >= int.MinValue && l <= int.MaxValue) return (int)l;
            if (v is short s) return s;
            if (v is byte b) return b;

            if (v is decimal dec &&
                dec >= int.MinValue && dec <= int.MaxValue &&
                decimal.Truncate(dec) == dec)
            {
                return (int)dec;
            }

            if (v is string str && int.TryParse(str.Trim(), out var parsed))
                return parsed;

            return null;
        }


        private static DateTime? GetDateTime(object o, string name)
        {
            var p = GetProp(o, name);
            var v = p?.GetValue(o);
            if (v is null) return null;

            if (v is DateTime dt) return dt;
            if (v is DateTimeOffset dto) return dto.UtcDateTime;

            if (v is string s && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
                return parsed;

            return null;
        }

        private static void SetValue(object target, string propertyName, object? value)
        {
            var p = GetProp(target, propertyName);
            if (p == null || !p.CanWrite) return;

            if (value == null)
            {
                p.SetValue(target, null);
                return;
            }

            var destType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

            if (destType.IsInstanceOfType(value))
            {
                p.SetValue(target, value);
                return;
            }

            // Convert.ChangeType handles ints/strings; DateTime handled by direct assignment above
            var converted = Convert.ChangeType(value, destType, CultureInfo.InvariantCulture);
            p.SetValue(target, converted);
        }


        // Temporarily disable Austin's "linked forms auto attach" feature (UI + behavior).
        private const bool AutoLinkingEnabled = false;

        private async Task ClearOtherPrimaryApplicationsAsync(int caseId, CancellationToken ct = default)
        {
            var currentPrimaries = await _cases.SW_caseForms
                .Where(x => x.SW_caseId == caseId && x.is_primary_application)
                .ToListAsync(ct);

            if (currentPrimaries.Count == 0) return;

            foreach (var row in currentPrimaries)
                row.is_primary_application = false;
        }


    }
}
