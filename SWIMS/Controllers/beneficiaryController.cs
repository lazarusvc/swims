using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    public class beneficiaryController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public beneficiaryController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        public string GenerateNewUuidAsString()
        {
            // Generates a new GUID and converts it to a string representation
            return Guid.NewGuid().ToString()[..5];
        }

        // GET: beneficiary
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_beneficiaries.ToListAsync());
        }

        // GET: beneficiary/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_beneficiary = await _context.SW_beneficiaries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_beneficiary == null)
            {
                return NotFound();
            }

            return View(sW_beneficiary);
        }

        // GET: beneficiary/Create
        public IActionResult Create()
        {
            ViewBag.UUID = GenerateNewUuidAsString();
            return View();
        }

        // POST: beneficiary/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,uuid,first_name,middle_name,last_name,dob,gender,martial_status,id_number,telephone_number,status,approved_amount,name")] SW_beneficiary sW_beneficiary)
        {
            if (ModelState.IsValid)
            {
                _context.Add(new SW_beneficiary { 
                    uuid = sW_beneficiary.uuid,
                    first_name = sW_beneficiary.first_name,
                    middle_name = sW_beneficiary.middle_name,
                    last_name = sW_beneficiary.last_name,
                    dob = sW_beneficiary.dob,
                    gender = sW_beneficiary.gender,
                    martial_status = sW_beneficiary.martial_status,
                    id_number = sW_beneficiary.id_number,
                    telephone_number = sW_beneficiary.telephone_number,
                    status = sW_beneficiary.status,
                    approved_amount = sW_beneficiary.approved_amount,
                    name = sW_beneficiary.first_name + " " + sW_beneficiary.last_name
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sW_beneficiary);
        }

        // GET: beneficiary/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_beneficiary = await _context.SW_beneficiaries.FindAsync(id);
            if (sW_beneficiary == null)
            {
                return NotFound();
            }
            return View(sW_beneficiary);
        }

        // POST: beneficiary/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,uuid,first_name,middle_name,last_name,dob,gender,martial_status,id_number,telephone_number,status,approved_amount,name")] SW_beneficiary sW_beneficiary)
        {
            if (id != sW_beneficiary.Id)
            {
                return NotFound();
            }

            sW_beneficiary.name = sW_beneficiary.first_name + " " + sW_beneficiary.last_name;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_beneficiary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_beneficiaryExists(sW_beneficiary.Id))
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
            return View(sW_beneficiary);
        }

        // GET: beneficiary/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_beneficiary = await _context.SW_beneficiaries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_beneficiary == null)
            {
                return NotFound();
            }

            return View(sW_beneficiary);
        }

        // POST: beneficiary/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_beneficiary = await _context.SW_beneficiaries.FindAsync(id);
            if (sW_beneficiary != null)
            {
                _context.SW_beneficiaries.Remove(sW_beneficiary);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_beneficiaryExists(int id)
        {
            return _context.SW_beneficiaries.Any(e => e.Id == id);
        }
    }
}
