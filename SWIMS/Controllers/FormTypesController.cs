using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data.Lookups;
using SWIMS.Models;
using SWIMS.Security;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    [Authorize(Policy = Permissions.RefData_Manage)]
    public class FormTypesController : Controller
    {
        private readonly SwimsLookupDbContext _lookup;

        public FormTypesController(SwimsLookupDbContext lookup)
        {
            _lookup = lookup;
        }

        // GET: /FormTypes
        public async Task<IActionResult> Index()
        {
            var types = await _lookup.SW_formTypes
                .AsNoTracking()
                .OrderBy(t => t.sort_order)
                .ThenBy(t => t.name)
                .ToListAsync();

            return View(types);
        }

        // GET: /FormTypes/Create
        public IActionResult Create()
        {
            var model = new SW_formType
            {
                is_active = true,
                sort_order = 1
            };

            return View(model);
        }

        // POST: /FormTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("code,name,is_active,sort_order")] SW_formType model)
        {
            if (ModelState.IsValid)
            {
                model.code = model.code?.Trim().ToUpperInvariant();
                model.name = model.name?.Trim();

                var exists = await _lookup.SW_formTypes
                    .AnyAsync(t => t.code == model.code);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.code), "Code must be unique.");
                }
                else
                {
                    _lookup.SW_formTypes.Add(model);
                    await _lookup.SaveChangesAsync();

                    TempData["Ok"] = "Form type created.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        // GET: /FormTypes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var type = await _lookup.SW_formTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type == null)
                return NotFound();

            return View(type);
        }

        // POST: /FormTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,code,name,is_active,sort_order")] SW_formType model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _lookup.SW_formTypes
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (existing == null)
                    return NotFound();

                existing.code = model.code?.Trim().ToUpperInvariant();
                existing.name = model.name?.Trim();
                existing.is_active = model.is_active;
                existing.sort_order = model.sort_order;

                await _lookup.SaveChangesAsync();

                TempData["Ok"] = "Form type updated.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /FormTypes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var type = await _lookup.SW_formTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type == null)
                return NotFound();

            return View(type);
        }

        // GET: /FormTypes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _lookup.SW_formTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type == null)
                return NotFound();

            return View(type);
        }

        // POST: /FormTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var type = await _lookup.SW_formTypes
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type == null)
                return NotFound();

            _lookup.SW_formTypes.Remove(type);
            await _lookup.SaveChangesAsync();

            TempData["Ok"] = "Form type deleted.";
            return RedirectToAction(nameof(Index));
        }



    }
}
