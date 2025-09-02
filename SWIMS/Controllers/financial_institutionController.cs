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
    public class financial_institutionController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public financial_institutionController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: financial_institution
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_financial_institutions.ToListAsync());
        }

        // GET: financial_institution/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_financial_institution = await _context.SW_financial_institutions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_financial_institution == null)
            {
                return NotFound();
            }

            return View(sW_financial_institution);
        }

        // GET: financial_institution/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: financial_institution/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name,email")] SW_financial_institution sW_financial_institution)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_financial_institution);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sW_financial_institution);
        }

        // GET: financial_institution/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_financial_institution = await _context.SW_financial_institutions.FindAsync(id);
            if (sW_financial_institution == null)
            {
                return NotFound();
            }
            return View(sW_financial_institution);
        }

        // POST: financial_institution/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,email")] SW_financial_institution sW_financial_institution)
        {
            if (id != sW_financial_institution.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_financial_institution);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_financial_institutionExists(sW_financial_institution.Id))
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
            return View(sW_financial_institution);
        }

        // GET: financial_institution/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_financial_institution = await _context.SW_financial_institutions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_financial_institution == null)
            {
                return NotFound();
            }

            return View(sW_financial_institution);
        }

        // POST: financial_institution/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_financial_institution = await _context.SW_financial_institutions.FindAsync(id);
            if (sW_financial_institution != null)
            {
                _context.SW_financial_institutions.Remove(sW_financial_institution);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_financial_institutionExists(int id)
        {
            return _context.SW_financial_institutions.Any(e => e.Id == id);
        }
    }
}
