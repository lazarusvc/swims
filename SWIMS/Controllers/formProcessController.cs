using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    public class formProcessController : Controller
    {
        private readonly SwimsDb_moreContext _context;
        private readonly SwimsStoredProcsDbContext _context_sp;

        public formProcessController(SwimsDb_moreContext context, SwimsStoredProcsDbContext sp)
        {
            _context = context;
            _context_sp = sp;
        }

        // GET: formProcess
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_formProcesses.ToListAsync());
        }

        // GET: formProcess/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }

            return View(sW_formProcess);
        }

        // GET: formProcess/Create
        public IActionResult Create()
        {
            ViewBag.processes = _context_sp.StoredProcesses
            .Select(c => new SelectListItem() { Text = c.Name, Value = "../StoredProcesses/Run/" + Convert.ToString(c.Id) })
            .ToList();

            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name");
            return View();
        }

        // POST: formProcess/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,url,name,SW_formsId")] SW_formProcess sW_formProcess)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_formProcess);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sW_formProcess);
        }

        // GET: formProcess/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses.FindAsync(id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }
            return View(sW_formProcess);
        }

        // POST: formProcess/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,url,name,SW_formsId")] SW_formProcess sW_formProcess)
        {
            if (id != sW_formProcess.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_formProcess);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_formProcessExists(sW_formProcess.Id))
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
            return View(sW_formProcess);
        }

        // GET: formProcess/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }

            return View(sW_formProcess);
        }

        // POST: formProcess/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_formProcess = await _context.SW_formProcesses.FindAsync(id);
            if (sW_formProcess != null)
            {
                _context.SW_formProcesses.Remove(sW_formProcess);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_formProcessExists(int id)
        {
            return _context.SW_formProcesses.Any(e => e.Id == id);
        }
    }
}
