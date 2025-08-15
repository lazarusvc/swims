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
    public class identitiesController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public identitiesController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: identities

        public async Task<IActionResult> Index()
        {
            return View(await _context.SwIdentities.ToListAsync());
        }

        // GET: identities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var swIdentity = await _context.SwIdentities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (swIdentity == null)
            {
                return NotFound();
            }

            return View(swIdentity);
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
        public async Task<IActionResult> Create([Bind("Id,Name,Desc,Logo,Media01,Media02,Media03,Header,Signature")] SwIdentity swIdentity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(swIdentity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(swIdentity);
        }

        // GET: identities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var swIdentity = await _context.SwIdentities.FindAsync(id);
            if (swIdentity == null)
            {
                return NotFound();
            }
            return View(swIdentity);
        }

        // POST: identities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Desc,Logo,Media01,Media02,Media03,Header,Signature")] SwIdentity swIdentity)
        {
            if (id != swIdentity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(swIdentity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SwIdentityExists(swIdentity.Id))
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
            return View(swIdentity);
        }

        // GET: identities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var swIdentity = await _context.SwIdentities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (swIdentity == null)
            {
                return NotFound();
            }

            return View(swIdentity);
        }

        // POST: identities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var swIdentity = await _context.SwIdentities.FindAsync(id);
            if (swIdentity != null)
            {
                _context.SwIdentities.Remove(swIdentity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SwIdentityExists(int id)
        {
            return _context.SwIdentities.Any(e => e.Id == id);
        }
    }
}
