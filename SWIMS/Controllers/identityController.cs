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
<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
    public class cityController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public cityController(SwimsDb_moreContext context)
========
    public class identityController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public identityController(SwimsDb_moreContext context)
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        {
            _context = context;
        }

<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
        // GET: city
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_cities.ToListAsync());
        }

        // GET: city/Details/5
========
        // GET: identity
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_identities.ToListAsync());
        }

        // GET: identity/Details/5
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
            var sW_city = await _context.SW_cities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_city == null)
========
            var sW_identity = await _context.SW_identities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_identity == null)
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
            {
                return NotFound();
            }

<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
            return View(sW_city);
        }

        // GET: city/Create
========
            return View(sW_identity);
        }

        // GET: identity/Create
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        public IActionResult Create()
        {
            return View();
        }

<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
        // POST: city/Create
========
        // POST: identity/Create
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
        public async Task<IActionResult> Create([Bind("Id,name")] SW_city sW_city)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_city);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sW_city);
        }

        // GET: city/Edit/5
========
        public async Task<IActionResult> Create([Bind("Id,name,desc,logo,media_01,media_02,media_03,header,signature")] SW_identity sW_identity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_identity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sW_identity);
        }

        // GET: identity/Edit/5
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
            var sW_city = await _context.SW_cities.FindAsync(id);
            if (sW_city == null)
            {
                return NotFound();
            }
            return View(sW_city);
        }

        // POST: city/Edit/5
========
            var sW_identity = await _context.SW_identities.FindAsync(id);
            if (sW_identity == null)
            {
                return NotFound();
            }
            return View(sW_identity);
        }

        // POST: identity/Edit/5
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
        public async Task<IActionResult> Edit(int id, [Bind("Id,name")] SW_city sW_city)
        {
            if (id != sW_city.Id)
========
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,desc,logo,media_01,media_02,media_03,header,signature")] SW_identity sW_identity)
        {
            if (id != sW_identity.Id)
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
                    _context.Update(sW_city);
========
                    _context.Update(sW_identity);
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
                    if (!SW_cityExists(sW_city.Id))
========
                    if (!SW_identityExists(sW_identity.Id))
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
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
<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
            return View(sW_city);
        }

        // GET: city/Delete/5
========
            return View(sW_identity);
        }

        // GET: identity/Delete/5
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
            var sW_city = await _context.SW_cities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_city == null)
========
            var sW_identity = await _context.SW_identities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_identity == null)
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
            {
                return NotFound();
            }

<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
            return View(sW_city);
        }

        // POST: city/Delete/5
========
            return View(sW_identity);
        }

        // POST: identity/Delete/5
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
            var sW_city = await _context.SW_cities.FindAsync(id);
            if (sW_city != null)
            {
                _context.SW_cities.Remove(sW_city);
========
            var sW_identity = await _context.SW_identities.FindAsync(id);
            if (sW_identity != null)
            {
                _context.SW_identities.Remove(sW_identity);
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

<<<<<<<< HEAD:SWIMS/Controllers/cityController.cs
        private bool SW_cityExists(int id)
        {
            return _context.SW_cities.Any(e => e.Id == id);
========
        private bool SW_identityExists(int id)
        {
            return _context.SW_identities.Any(e => e.Id == id);
>>>>>>>> int/merge-dev-2025-10:SWIMS/Controllers/identityController.cs
        }
    }
}
