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
            return Guid.NewGuid().ToString();
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
        public async Task<IActionResult> Create([Bind("Id,uuid,first_name,middle_name,last_name,dob,gender,martial_status,id_number,telephone_number,status,approved_amount")] SW_beneficiary sW_beneficiary, IFormCollection frm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_beneficiary);
                sW_beneficiary.uuid = GenerateNewUuidAsString();
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,uuid,first_name,middle_name,last_name,dob,gender,martial_status,id_number,telephone_number,status,approved_amount")] SW_beneficiary sW_beneficiary)
        {
            if (id != sW_beneficiary.Id)
            {
                return NotFound();
            }

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
