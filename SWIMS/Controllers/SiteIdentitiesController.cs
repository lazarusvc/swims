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
    public class SiteIdentityController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public SiteIdentityController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: identities
        public async Task<IActionResult> Index()
        {
            return View(await _context.SwSiteIdentities.ToListAsync());
        }

        // GET: identities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var swSiteIdentity = await _context.SwSiteIdentities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (swSiteIdentity == null)
            {
                return NotFound();
            }

            return View(swSiteIdentity);
        }

        // GET: identities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: identities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Desc,Logo,Media01,Media02,Media03,Header,Signature")] SwSiteIdentity swSiteIdentity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(swSiteIdentity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(swSiteIdentity);
        }

        // GET: identities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var swSiteIdentity = await _context.SwSiteIdentities.FindAsync(id);
            if (swSiteIdentity == null)
            {
                return NotFound();
            }
            return View(swSiteIdentity);
        }

        // POST: identities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Desc,Logo,Media01,Media02,Media03,Header,Signature")] SwSiteIdentity swSiteIdentity)
        {
            if (id != swSiteIdentity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(swSiteIdentity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SwIdentityExists(swSiteIdentity.Id))
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
            return View(swSiteIdentity);
        }

        // GET: identities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var swSiteIdentity = await _context.SwSiteIdentities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (swSiteIdentity == null)
            {
                return NotFound();
            }

            return View(swSiteIdentity);
        }

        // POST: identities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var swSiteIdentity = await _context.SwSiteIdentities.FindAsync(id);
            if (swSiteIdentity != null)
            {
                _context.SwSiteIdentities.Remove(swSiteIdentity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SwIdentityExists(int id)
        {
            return _context.SwSiteIdentities.Any(e => e.Id == id);
        }
    }
}
