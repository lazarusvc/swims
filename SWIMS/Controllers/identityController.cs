using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;

namespace SWIMS.Controllers
{
    // Standard CRUD controller for SW_identity
    public class identityController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public identityController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        // GET: identity
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_identities.ToListAsync());
        }

        // GET: identity/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_identity = await _context.SW_identities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_identity == null)
            {
                return NotFound();
            }

            return View(sW_identity);
        }

        // GET: identity/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: identity/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_identity = await _context.SW_identities.FindAsync(id);
            if (sW_identity == null)
            {
                return NotFound();
            }
            return View(sW_identity);
        }

        // POST: identity/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,name,desc,logo,media_01,media_02,media_03,header,signature")] SW_identity sW_identity)
        {
            if (id != sW_identity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_identity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_identityExists(sW_identity.Id))
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
            return View(sW_identity);
        }

        // GET: identity/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_identity = await _context.SW_identities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_identity == null)
            {
                return NotFound();
            }

            return View(sW_identity);
        }

        // POST: identity/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_identity = await _context.SW_identities.FindAsync(id);
            if (sW_identity != null)
            {
                _context.SW_identities.Remove(sW_identity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_identityExists(int id)
        {
            return _context.SW_identities.Any(e => e.Id == id);
        }
    }
}