using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;

namespace SWIMS.Controllers
{
    public class formTableNameController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public formTableNameController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: formTableName
        public async Task<IActionResult> Index()
        {
            var swimsDb_moreContext = _context.SW_formTableNames.Include(s => s.SW_forms);
            return View(await swimsDb_moreContext.ToListAsync());
        }

        // GET: formTableName/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableName = await _context.SW_formTableNames
                .Include(s => s.SW_forms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formTableName == null)
            {
                return NotFound();
            }

            return View(sW_formTableName);
        }

        // GET: formTableName/Create
        public IActionResult Create()
        {
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name");
            return View();
        }

        // POST: formTableName/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name,field,SW_formsId")] SW_formTableName sW_formTableName)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_formTableName);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableName.SW_formsId);
            return View(sW_formTableName);
        }

        // GET: formTableName/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableName = await _context.SW_formTableNames.FindAsync(id);
            if (sW_formTableName == null)
            {
                return NotFound();
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableName.SW_formsId);
            return View(sW_formTableName);
        }

        // POST: formTableName/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,field,SW_formsId")] SW_formTableName sW_formTableName)
        {
            if (id != sW_formTableName.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_formTableName);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_formTableNameExists(sW_formTableName.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableName.SW_formsId);
            return View(sW_formTableName);
        }

        // GET: formTableName/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableName = await _context.SW_formTableNames
                .Include(s => s.SW_forms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formTableName == null)
            {
                return NotFound();
            }

            return View(sW_formTableName);
        }

        // POST: formTableName/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_formTableName = await _context.SW_formTableNames.FindAsync(id);
            if (sW_formTableName != null)
            {
                _context.SW_formTableNames.Remove(sW_formTableName);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_formTableNameExists(int id)
        {
            return _context.SW_formTableNames.Any(e => e.Id == id);
        }
    }
}
