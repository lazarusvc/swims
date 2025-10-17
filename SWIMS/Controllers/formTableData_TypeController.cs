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
    public class formTableData_TypeController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public formTableData_TypeController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: formTableData_Type
        public async Task<IActionResult> Index()
        {
            var swimsDb_moreContext = _context.SW_formTableData_Types.Include(s => s.SW_forms);
            return View(await swimsDb_moreContext.ToListAsync());
        }

        // GET: formTableData_Type/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableData_Type = await _context.SW_formTableData_Types
                .Include(s => s.SW_forms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formTableData_Type == null)
            {
                return NotFound();
            }

            return View(sW_formTableData_Type);
        }

        // GET: formTableData_Type/Create
        public IActionResult Create()
        {
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name");
            return View();
        }

        // POST: formTableData_Type/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,type,field,SW_formsId")] SW_formTableData_Type sW_formTableData_Type)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_formTableData_Type);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableData_Type.SW_formsId);
            return View(sW_formTableData_Type);
        }

        // GET: formTableData_Type/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableData_Type = await _context.SW_formTableData_Types.FindAsync(id);
            if (sW_formTableData_Type == null)
            {
                return NotFound();
            }
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableData_Type.SW_formsId);
            return View(sW_formTableData_Type);
        }

        // POST: formTableData_Type/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,type,field,SW_formsId")] SW_formTableData_Type sW_formTableData_Type)
        {
            if (id != sW_formTableData_Type.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_formTableData_Type);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_formTableData_TypeExists(sW_formTableData_Type.Id))
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
            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name", sW_formTableData_Type.SW_formsId);
            return View(sW_formTableData_Type);
        }

        // GET: formTableData_Type/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formTableData_Type = await _context.SW_formTableData_Types
                .Include(s => s.SW_forms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formTableData_Type == null)
            {
                return NotFound();
            }

            return View(sW_formTableData_Type);
        }

        // POST: formTableData_Type/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_formTableData_Type = await _context.SW_formTableData_Types.FindAsync(id);
            if (sW_formTableData_Type != null)
            {
                _context.SW_formTableData_Types.Remove(sW_formTableData_Type);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_formTableData_TypeExists(int id)
        {
            return _context.SW_formTableData_Types.Any(e => e.Id == id);
        }
    }
}
