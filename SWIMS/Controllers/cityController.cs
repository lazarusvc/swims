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
    public class cityController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public cityController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: city
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_cities.ToListAsync());
        }

        // GET: city/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_city = await _context.SW_cities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_city == null)
            {
                return NotFound();
            }

            return View(sW_city);
        }

        // GET: city/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: city/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,name")] SW_city sW_city, IFormCollection frm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_city);
                await _context.SaveChangesAsync();

                // Redirect logic for /form/Program quick access buttons
                // ---------------------------------------------------------
                string partialCheck = frm["partialCheck"].ToString();
                if (partialCheck != null)
                {
                    return RedirectToAction("Program", "form", new { uuid = partialCheck });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(sW_city);
        }

        // GET: city/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_city = await _context.SW_cities.FindAsync(id);
            if (sW_city == null)
            {
                return NotFound();
            }
            return View(sW_city);
        }

        // POST: city/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name")] SW_city sW_city)
        {
            if (id != sW_city.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_city);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_cityExists(sW_city.Id))
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
            return View(sW_city);
        }

        // GET: city/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_city = await _context.SW_cities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_city == null)
            {
                return NotFound();
            }

            return View(sW_city);
        }

        // POST: city/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_city = await _context.SW_cities.FindAsync(id);
            if (sW_city != null)
            {
                _context.SW_cities.Remove(sW_city);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_cityExists(int id)
        {
            return _context.SW_cities.Any(e => e.Id == id);
        }
    }
}
