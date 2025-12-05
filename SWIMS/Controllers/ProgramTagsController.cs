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
        public async Task<IActionResult> Create([Bind("code,name,is_active,sort_order")] SW_programTag model)
        {
            if (ModelState.IsValid)
            {
                model.code = model.code?.Trim().ToUpperInvariant();
                model.name = model.name?.Trim();

                var exists = await _lookup.SW_programTags
                    .AnyAsync(t => t.code == model.code);

                if (exists)
                {
                    ModelState.AddModelError(nameof(model.code), "Code must be unique.");
                }
                else
                {
                    _lookup.SW_programTags.Add(model);
                    await _lookup.SaveChangesAsync();

                    TempData["Ok"] = "Program tag created.";
                    return RedirectToAction(nameof(Index));
                }
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
            [Bind("Id,code,name,is_active,sort_order")] SW_programTag model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _lookup.SW_programTags
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (existing == null)
                    return NotFound();

                existing.code = model.code?.Trim().ToUpperInvariant();
                existing.name = model.name?.Trim();
                existing.is_active = model.is_active;
                existing.sort_order = model.sort_order;

                await _lookup.SaveChangesAsync();

                TempData["Ok"] = "Program tag updated.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}
