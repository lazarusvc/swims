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
    public class organizationController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public organizationController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: organization
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_organizations.ToListAsync());
        }

        // GET: organization/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_organization = await _context.SW_organizations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_organization == null)
            {
                return NotFound();
            }

            return View(sW_organization);
        }

        // GET: organization/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: organization/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,vendor_id,name,type,active")] SW_organization sW_organization)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_organization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sW_organization);
        }

        // GET: organization/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_organization = await _context.SW_organizations.FindAsync(id);
            if (sW_organization == null)
            {
                return NotFound();
            }
            return View(sW_organization);
        }

        // POST: organization/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,vendor_id,name,type,active")] SW_organization sW_organization)
        {
            if (id != sW_organization.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_organization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_organizationExists(sW_organization.Id))
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
            return View(sW_organization);
        }

        // GET: organization/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_organization = await _context.SW_organizations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_organization == null)
            {
                return NotFound();
            }

            return View(sW_organization);
        }

        // POST: organization/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_organization = await _context.SW_organizations.FindAsync(id);
            if (sW_organization != null)
            {
                _context.SW_organizations.Remove(sW_organization);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_organizationExists(int id)
        {
            return _context.SW_organizations.Any(e => e.Id == id);
        }
    }
}
