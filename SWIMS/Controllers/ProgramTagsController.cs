using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data.Lookups;
using SWIMS.Models;
using SWIMS.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    [Authorize(Policy = Permissions.RefData_Manage)]
    public class ProgramTagsController : Controller
    {
        private readonly SwimsLookupDbContext _lookup;

        public ProgramTagsController(SwimsLookupDbContext lookup)
        {
            _lookup = lookup;
        }

        // GET: /ProgramTags
        public async Task<IActionResult> Index()
        {
            var tags = await _lookup.SW_programTags
                .AsNoTracking()
                .OrderBy(t => t.sort_order)
                .ThenBy(t => t.name)
                .ToListAsync();

            return View(tags);
        }

        // GET: /ProgramTags/Create
        public IActionResult Create()
        {
            var model = new SW_programTag
            {
                is_active = true,
                sort_order = 1
            };

            return View(model);
        }

        // POST: /ProgramTags/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("code,name,is_active,sort_order,default_benefit_months")] SW_programTag model)
        {
            NormalizeAndValidate(model, ModelState);

            if (ModelState.IsValid)
            {
                var exists = await _lookup.SW_programTags
                    .AnyAsync(t => t.code == model.code);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.code), "Code must be unique.");
                    return View(model);
                }

                _lookup.SW_programTags.Add(model);
                await _lookup.SaveChangesAsync();

                TempData["Ok"] = "Program tag created.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /ProgramTags/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _lookup.SW_programTags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
                return NotFound();

            return View(tag);
        }

        // POST: /ProgramTags/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,code,name,is_active,sort_order,default_benefit_months")] SW_programTag model)
        {
            if (id != model.Id)
                return NotFound();

            NormalizeAndValidate(model, ModelState);

            if (ModelState.IsValid)
            {
                var codeConflict = await _lookup.SW_programTags
                    .AnyAsync(t => t.Id != id && t.code == model.code);

                if (codeConflict)
                {
                    ModelState.AddModelError(nameof(model.code), "Code must be unique.");
                    return View(model);
                }

                var existing = await _lookup.SW_programTags
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (existing == null)
                    return NotFound();

                existing.code = model.code;
                existing.name = model.name;
                existing.is_active = model.is_active;
                existing.sort_order = model.sort_order;
                existing.default_benefit_months = model.default_benefit_months;

                await _lookup.SaveChangesAsync();

                TempData["Ok"] = "Program tag updated.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        private static void NormalizeAndValidate(SW_programTag model, ModelStateDictionary modelState)
        {
            // Normalize
            model.code = (model.code ?? string.Empty).Trim().ToUpperInvariant();
            model.name = (model.name ?? string.Empty).Trim();

            // Required fields
            if (string.IsNullOrWhiteSpace(model.code))
                modelState.AddModelError(nameof(model.code), "Code is required.");

            if (string.IsNullOrWhiteSpace(model.name))
                modelState.AddModelError(nameof(model.name), "Name is required.");

            // Sort order sanity (optional guard)
            if (model.sort_order < 0)
                modelState.AddModelError(nameof(model.sort_order), "Sort order must be 0 or greater.");

            // Default months: treat <= 0 as not set
            if (model.default_benefit_months.HasValue && model.default_benefit_months.Value <= 0)
                model.default_benefit_months = null;

            // If set, validate range
            if (model.default_benefit_months.HasValue)
            {
                var m = model.default_benefit_months.Value;
                if (m < 1 || m > 120)
                    modelState.AddModelError(nameof(model.default_benefit_months), "Default benefit months must be between 1 and 120.");
            }
        }

        // GET: /ProgramTags/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tag = await _lookup.SW_programTags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
                return NotFound();

            return View(tag);
        }

        // GET: /ProgramTags/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _lookup.SW_programTags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
                return NotFound();

            return View(tag);
        }

        // POST: /ProgramTags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _lookup.SW_programTags
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
                return NotFound();

            _lookup.SW_programTags.Remove(tag);
            await _lookup.SaveChangesAsync();

            TempData["Ok"] = "Program tag deleted.";
            return RedirectToAction(nameof(Index));
        }



    }
}
