using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    public class formController : Controller
    {
        private readonly SwimsDb_moreContext _context;

        public formController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        public string GenerateNewUuidAsString()
        {
            // Generates a new GUID and converts it to a string representation
            return Guid.NewGuid().ToString();
        }

        public IActionResult Program(string? uuid)
        {
            ViewBag.form = _context.SW_forms.Where(m => m.uuid.Equals(uuid)).Select(m => m.form).FirstOrDefault();
            ViewBag.formName = _context.SW_forms.Where(m => m.uuid.Equals(uuid)).Select(m => m.name).FirstOrDefault();
            return View();
        }

        // GET: form
        public async Task<IActionResult> Index()
        {
            var swimsDb_moreContext = _context.SW_forms.Include(s => s.SW_identity);
            return View(await swimsDb_moreContext.ToListAsync());
        }

        // GET: form/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_form = await _context.SW_forms
                .Include(s => s.SW_identity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_form == null)
            {
                return NotFound();
            }

            return View(sW_form);
        }

        // GET: form/Create
        public IActionResult Create()
        {
            ViewBag.datetime = System.DateTime.UtcNow;
            ViewBag.UUID = GenerateNewUuidAsString();
            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name");
            return View();
        }

        // POST: form/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,uuid,name,desc,form,isApproval_01,isApproval_02,isApproval_03,dateModified,SW_identityId")] SW_form sW_form)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sW_form);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Complete), new { id = sW_form.Id});
            }
            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name", sW_form.SW_identityId);
            return View(sW_form);
        }

        public IActionResult Complete(int? id)
        {
            ViewBag.id = id;
            ViewBag.frm = _context.SW_forms.Where(x => x.Id == id).Select(x => x.form).FirstOrDefault();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(IFormCollection frm)
        {
            int fID = Convert.ToInt32(frm["formId"]);

            // fetch form JSON from DB
            var swForm = await _context.SW_forms.FindAsync(fID);
            if (swForm == null) return NotFound("Form not found");

            // deserialize into a collection
            var formDefinition = JsonSerializer.Deserialize<List<form_FieldAttributes>>(swForm.form);

            if (formDefinition == null || !formDefinition.Any())
                return BadRequest("Form definition empty or invalid JSON");

            // SW_formTableName
            // map json data {label, name} to DB context name
            var fieldEntities__names = formDefinition.Select(def => new SW_formTableName
            {
                name = def.label,
                field = def.name,
                SW_formsId = fID
            }).ToList();

            // SW_formTableData_Type
            // map json data {type, name} to DB context name
            var fieldEntities__types = formDefinition.Select(def => new SW_formTableData_Type
            {
                type = def.type,
                field = def.name,
                SW_formsId = fID
            }).ToList();

            // save to DB table(s)
            await _context.SW_formTableNames.AddRangeAsync(fieldEntities__names);
            await _context.SW_formTableData_Types.AddRangeAsync(fieldEntities__types);
            await _context.SaveChangesAsync();

            return View();
        }

        // GET: form/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_form = await _context.SW_forms.FindAsync(id);
            if (sW_form == null)
            {
                return NotFound();
            }
            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name", sW_form.SW_identityId);
            return View(sW_form);
        }

        // POST: form/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,uuid,name,desc,form,isApproval_01,isApproval_02,isApproval_03,dateModified,SW_identityId")] SW_form sW_form)
        {
            if (id != sW_form.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sW_form);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_formExists(sW_form.Id))
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
            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name", sW_form.SW_identityId);
            return View(sW_form);
        }

        // GET: form/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_form = await _context.SW_forms
                .Include(s => s.SW_identity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_form == null)
            {
                return NotFound();
            }

            return View(sW_form);
        }

        // POST: form/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_form = await _context.SW_forms.FindAsync(id);
            if (sW_form != null)
            {
                _context.SW_forms.Remove(sW_form);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_formExists(int id)
        {
            return _context.SW_forms.Any(e => e.Id == id);
        }
    }
}
