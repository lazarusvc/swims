using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using SWIMS.Data;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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

        [HttpGet]
        public async Task<IActionResult> Program(string? uuid)
        {
            // 0. Varibales 
            var f_Linq = _context.SW_forms.Where(m => m.uuid.Equals(uuid));
            int formId = Convert.ToInt32(f_Linq.Select(m => m.Id).FirstOrDefault());
            ViewBag.formId = formId;
            ViewBag.form = f_Linq.Select(m => m.form).FirstOrDefault();
            ViewBag.formName = f_Linq.Select(m => m.name).FirstOrDefault();
            ViewBag.formImage = f_Linq.Select(m => m.image).FirstOrDefault();
            ViewBag.formDesc = f_Linq.Select(m => m.desc).FirstOrDefault();
            ViewBag.header = f_Linq.Select(x => x.header).FirstOrDefault();

            ViewBag.processes = _context.SW_formProcesses
                .Where(c => c.SW_formsId == formId)
                .Select(c => new SelectListItem() { Text = c.url, Value = c.url })
                .ToList();

            ViewBag.entries = _context.SW_formTableData.Where(c => c.SW_formsId == formId).Count();
            ViewBag.entries_pending = _context.SW_formTableData.Where(
                c => c.SW_formsId == formId && 
                c.isApproval_01 == 0 ||
                c.isApproval_02 == 0 ||
                c.isApproval_03 == 0).Count();
            ViewBag.entries_approved = _context.SW_formTableData.Where(
                c => c.SW_formsId == formId &&
                c.isApproval_01 == 1 ||
                c.isApproval_02 == 1 ||
                c.isApproval_03 == 1).Count();


            // 1. Fetch form with JSON
            var swForm = await _context.SW_forms.FindAsync(formId);
            if (swForm == null) return NotFound("Form not found");

            // 2. Deserialize JSON definition
            var formDefinition = JsonSerializer.Deserialize<List<form_FieldAttributes>>(swForm.form);
            if (formDefinition == null || !formDefinition.Any())
                return BadRequest("Invalid or empty form definition");

            // 3. Fetch mappings from SW_formTableName
            var tableNameMappings = _context.SW_formTableNames
                .Where(t => t.SW_formsId == formId)
                .ToList();

            // 4. Join JSON fields with SW_formTableName mappings
            var columnMappings = formDefinition
                .Join(tableNameMappings,
                      def => def.name,     // JSON name
                      map => map.field,     // DB mapping name
                      (def, map) => new ColumnMap
                      {
                          ColumnName = map.field, // e.g. FormData01
                          Label = def.label       // e.g. "Text Field"
                      })
                .ToList();

            if (!columnMappings.Any())
                return BadRequest("No matching columns found for this form");

            // 5. Fetch actual data rows from SW_formTableDatum
            var dataRows = await _context.SW_formTableData
                .Where(d => d.SW_formsId == formId)
                .ToListAsync();

            // 6. Convert data into dictionary per row
            var rowList = new List<Dictionary<string, string>>();
            foreach (var row in dataRows)
            {
                var dict = new Dictionary<string, string>();
                foreach (var col in columnMappings)
                {
                    // use reflection to get the property value dynamically
                    var prop = typeof(SW_formTableDatum).GetProperty(col.ColumnName);
                    if (prop != null)
                    {
                        var val = prop.GetValue(row)?.ToString() ?? string.Empty;
                        dict[col.ColumnName] = val;
                    }
                }
                rowList.Add(dict);
            }

            // 7. Build ViewModel
            var model = new FormTableViewModel
            {
                FormName = swForm.name,
                Columns = columnMappings,
                Rows = rowList
            };

            return View(model);
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
        public async Task<IActionResult> Create([Bind("Id,uuid,name,desc,form,dateModified,SW_identityId,is_linking,image,header")] SW_form sW_form, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Define a path to save the file (e.g., in wwwroot/uploads)
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Create a unique file name to avoid conflicts
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // Save the file to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            if (ModelState.IsValid)
            {
                _context.Add(new SW_form 
                { 
                    uuid = sW_form.uuid,
                    name = sW_form.name,
                    desc = sW_form.desc,
                    form = sW_form.form,
                    dateModified = sW_form.dateModified,
                    SW_identityId = sW_form.SW_identityId,
                    is_linking = sW_form.is_linking,
                    image = uniqueFileName,
                    header = sW_form.header
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = sW_form.Id});
            }
            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name", sW_form.SW_identityId);
            return View(sW_form);
        }

        public IActionResult Complete(int? id)
        {
            ViewBag.id = id;
            ViewBag.frm = _context.SW_forms.Where(x => x.Id == id).Select(x => x.form).FirstOrDefault();
            ViewBag.header = _context.SW_forms.Where(x => x.Id == id).Select(x => x.header).FirstOrDefault();
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
            ViewBag.frm = _context.SW_forms.Where(x => x.Id == id).Select(x => x.form).FirstOrDefault();
            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name", sW_form.SW_identityId);
            return View(sW_form);
        }

        // POST: form/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,uuid,name,desc,form,dateModified,SW_identityId,is_linking,image,header")] SW_form sW_form, IFormFile image)
        {
            if (id != sW_form.Id)
            {
                return NotFound();
            }

            if (image == null || image.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Define a path to save the file (e.g., in wwwroot/uploads)
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Create a unique file name to avoid conflicts
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            sW_form.image = uniqueFileName;
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // Save the file to the server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
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
            // remove corresponding form Data
            await _context.SW_formTableData
                .Where(c => c.SW_formsId == id)
                .ExecuteDeleteAsync();

            // remove corresponding form Data Types
            await _context.SW_formTableData_Types
                .Where(c => c.SW_formsId == id)
                .ExecuteDeleteAsync();

            // remove corresponding form Data Names
            await _context.SW_formTableNames
                .Where(c => c.SW_formsId == id)
                .ExecuteDeleteAsync();

            // remove corresponding form Processes
            await _context.SW_formProcesses
                .Where(c => c.SW_formsId == id)
                .ExecuteDeleteAsync();

            // remove corresponding form Reports
            await _context.SW_formReports
                .Where(c => c.SW_formsId == id)
                .ExecuteDeleteAsync();

            // finally remove form
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
