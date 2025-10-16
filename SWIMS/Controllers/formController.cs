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
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

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
            // 
            var f_Linq = _context.SW_forms.Where(m => m.uuid.Equals(uuid));
            int formId = Convert.ToInt32(f_Linq.Select(m => m.Id).FirstOrDefault());
            ViewBag.uuid = uuid;
            ViewBag.formId = formId;
            ViewBag.form = f_Linq.Select(m => m.form).FirstOrDefault();
            ViewBag.formName = f_Linq.Select(m => m.name).FirstOrDefault();
            ViewBag.formImage = f_Linq.Select(m => m.image).FirstOrDefault();
            ViewBag.formDesc = f_Linq.Select(m => m.desc).FirstOrDefault();
            ViewBag.header = f_Linq.Select(x => x.header).FirstOrDefault();
            var linking = f_Linq.Select(x => x.is_linking).FirstOrDefault();
            if (linking == true)
            {
                ViewBag.formLINK = _context.SW_forms.Where(x => x.is_linking == true).ToList();
                ViewBag.LK = 1;
            }

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
                        dict["IDS"] = Convert.ToString(row.Id);
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

        public async Task<IActionResult> ProgramExpand(string? uuid)
        {
            // 0. Varibales
            // 
            var f_Linq = _context.SW_forms.Where(m => m.uuid.Equals(uuid));
            int formId = Convert.ToInt32(f_Linq.Select(m => m.Id).FirstOrDefault());
            ViewBag.uuid = uuid;
            ViewBag.formId = formId;
            ViewBag.form = f_Linq.Select(m => m.form).FirstOrDefault();
            ViewBag.formName = f_Linq.Select(m => m.name).FirstOrDefault();
            ViewBag.formImage = f_Linq.Select(m => m.image).FirstOrDefault();
            ViewBag.formDesc = f_Linq.Select(m => m.desc).FirstOrDefault();
            ViewBag.header = f_Linq.Select(x => x.header).FirstOrDefault();
            var linking = f_Linq.Select(x => x.is_linking).FirstOrDefault();
            if (linking == true)
            {
                ViewBag.formLINK = _context.SW_forms.Where(x => x.is_linking == true).ToList();
                ViewBag.LK = 1;
            }

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
                        dict["IDS"] = Convert.ToString(row.Id);
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

        [HttpGet]
        public IActionResult Approval(string? uuid)
        {
            var formId = Convert.ToInt32(_context.SW_forms.Where(m => m.uuid == uuid).Select(m => m.Id).FirstOrDefault());

            ViewBag.uuid = uuid;
            ViewBag.appAmt = Convert.ToInt32(_context.SW_forms.Where(m => m.uuid == uuid).Select(m => m.approvalAmt).FirstOrDefault());
            ViewBag.appList = _context.SW_formTableData.Where(
                x => x.isApproval_01 == 0 ||
                x.isApproval_02 == 0 ||
                x.isApproval_03 == 0 &&
                x.SW_formsId == formId
            ).ToList();
            return View();
        }

        public async Task<IActionResult> Preview(string? dataID, string? uuid)
        {
            // ************** Varibales
            //
            int id = Convert.ToInt32(dataID);
            var f_Linq = _context.SW_forms.Where(m => m.uuid.Equals(uuid));
            int formId = f_Linq.Select(m => m.Id).FirstOrDefault();
            ViewBag.formId = formId;
            ViewBag.form = f_Linq.Select(m => m.form).FirstOrDefault();
            ViewBag.formName = f_Linq.Select(m => m.name).FirstOrDefault();
            ViewBag.formImage = f_Linq.Select(m => m.image).FirstOrDefault();
            ViewBag.formDesc = f_Linq.Select(m => m.desc).FirstOrDefault();
            ViewBag.header = f_Linq.Select(x => x.header).FirstOrDefault();

            // ************* FormData
            //
            var _fData = await _context.SW_formTableData.FindAsync(id);
            if (_fData == null)
            {
                return NotFound();
            }
            var stringArray = new string[250]
            {
                _fData.FormData01,
                _fData.FormData02,
                _fData.FormData03,
                _fData.FormData04,
                _fData.FormData05,
                _fData.FormData06,
                _fData.FormData07,
                _fData.FormData08,
                _fData.FormData09,
                _fData.FormData10,
                _fData.FormData11,
                _fData.FormData12,
                _fData.FormData13,
                _fData.FormData14,
                _fData.FormData15,
                _fData.FormData16,
                _fData.FormData17,
                _fData.FormData18,
                _fData.FormData19,
                _fData.FormData20,
                _fData.FormData21,
                _fData.FormData22,
                _fData.FormData23,
                _fData.FormData24,
                _fData.FormData25,
                _fData.FormData26,
                _fData.FormData27,
                _fData.FormData28,
                _fData.FormData29,
                _fData.FormData30,
                _fData.FormData31,
                _fData.FormData32,
                _fData.FormData33,
                _fData.FormData34,
                _fData.FormData35,
                _fData.FormData36,
                _fData.FormData37,
                _fData.FormData38,
                _fData.FormData39,
                _fData.FormData40,
                _fData.FormData41,
                _fData.FormData42,
                _fData.FormData43,
                _fData.FormData44,
                _fData.FormData45,
                _fData.FormData46,
                _fData.FormData47,
                _fData.FormData48,
                _fData.FormData49,
                _fData.FormData50,
                _fData.FormData51,
                _fData.FormData52,
                _fData.FormData53,
                _fData.FormData54,
                _fData.FormData55,
                _fData.FormData56,
                _fData.FormData57,
                _fData.FormData58,
                _fData.FormData59,
                _fData.FormData60,
                _fData.FormData61,
                _fData.FormData62,
                _fData.FormData63,
                _fData.FormData64,
                _fData.FormData65,
                _fData.FormData66,
                _fData.FormData67,
                _fData.FormData68,
                _fData.FormData69,
                _fData.FormData70,
                _fData.FormData71,
                _fData.FormData72,
                _fData.FormData73,
                _fData.FormData74,
                _fData.FormData75,
                _fData.FormData76,
                _fData.FormData77,
                _fData.FormData78,
                _fData.FormData79,
                _fData.FormData80,
                _fData.FormData81,
                _fData.FormData82,
                _fData.FormData83,
                _fData.FormData84,
                _fData.FormData85,
                _fData.FormData86,
                _fData.FormData87,
                _fData.FormData88,
                _fData.FormData89,
                _fData.FormData90,
                _fData.FormData91,
                _fData.FormData92,
                _fData.FormData93,
                _fData.FormData94,
                _fData.FormData95,
                _fData.FormData96,
                _fData.FormData97,
                _fData.FormData98,
                _fData.FormData99,
                _fData.FormData100,
                _fData.FormData101,
                _fData.FormData102,
                _fData.FormData103,
                _fData.FormData104,
                _fData.FormData105,
                _fData.FormData106,
                _fData.FormData107,
                _fData.FormData108,
                _fData.FormData109,
                _fData.FormData110,
                _fData.FormData111,
                _fData.FormData112,
                _fData.FormData113,
                _fData.FormData114,
                _fData.FormData115,
                _fData.FormData116,
                _fData.FormData117,
                _fData.FormData118,
                _fData.FormData119,
                _fData.FormData120,
                _fData.FormData121,
                _fData.FormData122,
                _fData.FormData123,
                _fData.FormData124,
                _fData.FormData125,
                _fData.FormData126,
                _fData.FormData127,
                _fData.FormData128,
                _fData.FormData129,
                _fData.FormData130,
                _fData.FormData131,
                _fData.FormData132,
                _fData.FormData133,
                _fData.FormData134,
                _fData.FormData135,
                _fData.FormData136,
                _fData.FormData137,
                _fData.FormData138,
                _fData.FormData139,
                _fData.FormData140,
                _fData.FormData141,
                _fData.FormData142,
                _fData.FormData143,
                _fData.FormData144,
                _fData.FormData145,
                _fData.FormData146,
                _fData.FormData147,
                _fData.FormData148,
                _fData.FormData149,
                _fData.FormData150,
                _fData.FormData151,
                _fData.FormData152,
                _fData.FormData153,
                _fData.FormData154,
                _fData.FormData155,
                _fData.FormData156,
                _fData.FormData157,
                _fData.FormData158,
                _fData.FormData159,
                _fData.FormData160,
                _fData.FormData161,
                _fData.FormData162,
                _fData.FormData163,
                _fData.FormData164,
                _fData.FormData165,
                _fData.FormData166,
                _fData.FormData167,
                _fData.FormData168,
                _fData.FormData169,
                _fData.FormData170,
                _fData.FormData171,
                _fData.FormData172,
                _fData.FormData173,
                _fData.FormData174,
                _fData.FormData175,
                _fData.FormData176,
                _fData.FormData177,
                _fData.FormData178,
                _fData.FormData179,
                _fData.FormData180,
                _fData.FormData181,
                _fData.FormData182,
                _fData.FormData183,
                _fData.FormData184,
                _fData.FormData185,
                _fData.FormData186,
                _fData.FormData187,
                _fData.FormData188,
                _fData.FormData189,
                _fData.FormData190,
                _fData.FormData191,
                _fData.FormData192,
                _fData.FormData193,
                _fData.FormData194,
                _fData.FormData195,
                _fData.FormData196,
                _fData.FormData197,
                _fData.FormData198,
                _fData.FormData199,
                _fData.FormData200,
                _fData.FormData201,
                _fData.FormData202,
                _fData.FormData203,
                _fData.FormData204,
                _fData.FormData205,
                _fData.FormData206,
                _fData.FormData207,
                _fData.FormData208,
                _fData.FormData209,
                _fData.FormData210,
                _fData.FormData211,
                _fData.FormData212,
                _fData.FormData213,
                _fData.FormData214,
                _fData.FormData215,
                _fData.FormData216,
                _fData.FormData217,
                _fData.FormData218,
                _fData.FormData219,
                _fData.FormData220,
                _fData.FormData221,
                _fData.FormData222,
                _fData.FormData223,
                _fData.FormData224,
                _fData.FormData225,
                _fData.FormData226,
                _fData.FormData227,
                _fData.FormData228,
                _fData.FormData229,
                _fData.FormData230,
                _fData.FormData231,
                _fData.FormData232,
                _fData.FormData233,
                _fData.FormData234,
                _fData.FormData235,
                _fData.FormData236,
                _fData.FormData237,
                _fData.FormData238,
                _fData.FormData239,
                _fData.FormData240,
                _fData.FormData241,
                _fData.FormData242,
                _fData.FormData243,
                _fData.FormData244,
                _fData.FormData245,
                _fData.FormData246,
                _fData.FormData247,
                _fData.FormData248,
                _fData.FormData249,
                _fData.FormData250
            };
            ViewBag.Collection = stringArray;

            // ************* FormData Types
            var _fDataType = _context.SW_formTableData_Types.Where(x => x.SW_formsId == formId).ToList();
            if (_fDataType == null)
            {
                return NotFound();
            }
            ViewBag.Collection2 = _fDataType;
            return PartialView("Views/Shared/_formPreview.cshtml");
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
        public async Task<IActionResult> Create([Bind("Id,uuid,name,desc,form,dateModified,SW_identityId,is_linking,image,header,approvalAmt")] SW_form sW_form, IFormFile image)
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
                return RedirectToAction(nameof(Index), new { id = sW_form.Id });
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
            // Resolve formId robustly (avoid FormatException when the field is missing/empty)
            int fID;
            var formIdRaw = frm["formId"].FirstOrDefault();
            if (!int.TryParse(formIdRaw, out fID))
            {
                // try alternate keys or route values
                var idRaw = frm["id"].FirstOrDefault();
                if (int.TryParse(idRaw, out var altId))
                {
                    fID = altId;
                }
                else if (int.TryParse(HttpContext.Request.RouteValues["id"]?.ToString(), out var routeId))
                {
                    fID = routeId;
                }
                else
                {
                    var uuid = frm["uuid"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(uuid))
                    {
                        fID = await _context.SW_forms
                            .Where(x => x.uuid == uuid)
                            .Select(x => x.Id)
                            .FirstOrDefaultAsync();
                    }
                    else
                    {
                        return BadRequest("Missing form identifier (formId or uuid).");
                    }
                }
            }
            if (fID <= 0)
            {
                return BadRequest("Form not found for provided identifier.");
            }

            // fetch form JSON from DB
            var swForm = await _context.SW_forms.FindAsync(fID);
            if (swForm == null) return NotFound("Form not found");

            // deserialize into a collection
            var formDefinition = JsonSerializer.Deserialize<List<form_FieldAttributes>>(swForm.form);

            // --- STATIC field counters seeded from what's already saved for this form ---
            var staticFields = _context.SW_formTableNames
                .Where(n => n.SW_formsId == fID && n.field != null && n.field.StartsWith("STATIC_"))
                .Select(n => n.field!)
                .AsEnumerable(); // switch to LINQ-to-Objects before regex work

            int nextH = staticFields
                .Where(f => f.StartsWith("STATIC_H_", StringComparison.Ordinal))
                .Select(f => {
                    var m = StaticKeyRx.Match(f);
                    return m.Success ? int.Parse(m.Groups[1].Value) : 0;
                })
                .DefaultIfEmpty(0)
                .Max() + 1;

            int nextP = staticFields
                .Where(f => f.StartsWith("STATIC_P_", StringComparison.Ordinal))
                .Select(f => {
                    var m = StaticKeyRx.Match(f);
                    return m.Success ? int.Parse(m.Groups[1].Value) : 0;
                })
                .DefaultIfEmpty(0)
                .Max() + 1;


            // Normalize static blocks (header/paragraph) so they get valid keys
            var perTypeCounter = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var def in formDefinition)
            {
                var t = def.type?.ToLowerInvariant();
                if (t == "header" || t == "paragraph")
                {
                    perTypeCounter.TryGetValue(t, out var idx);
                    idx++; perTypeCounter[t] = idx;

                    // 1) Give a display name if the builder omitted it
                    var prefix = t == "header" ? "Header" : "Paragraph";
                    def.label = EnsureName(def.name, def.label, prefix, idx);

                    // 2) Give a synthetic field key if missing (never collides with FormData##)
                    //    STATIC_H_### for headers, STATIC_P_### for paragraphs
                    if (string.IsNullOrWhiteSpace(def.name))
                    {
                        def.name = t == "header"
                            ? $"STATIC_H_{nextH++:D3}"
                            : $"STATIC_P_{nextP++:D3}";
                    }

                }
            }

            if (formDefinition == null || !formDefinition.Any())
                return BadRequest("Form definition empty or invalid JSON");

            // ---------------------------
            // UPSERT instead of AddRange
            // ---------------------------
            var incoming = formDefinition.Select(d => new
            {
                Field = d.name!.Trim(),                                // e.g., FormData07 or STATIC_H_001
                DisplayName = (d.label ?? d.name)!.Trim(),             // human-friendly name in SW_formTableName
                Type = d.type?.Trim().ToLowerInvariant() ?? "text"     // stored in SW_formTableData_Types
            }).ToList();

            var existingNames = await _context.SW_formTableNames
                .Where(n => n.SW_formsId == fID)
                .ToDictionaryAsync(n => n.field!, StringComparer.OrdinalIgnoreCase);

            var existingTypes = await _context.SW_formTableData_Types
                .Where(t => t.SW_formsId == fID)
                .ToDictionaryAsync(t => t.field!, StringComparer.OrdinalIgnoreCase);

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var x in incoming)
            {
                seen.Add(x.Field);

                // Names: update-or-insert
                if (existingNames.TryGetValue(x.Field, out var nRow))
                {
                    nRow.name = x.DisplayName;
                }
                else
                {
                    _context.SW_formTableNames.Add(new SW_formTableName
                    {
                        SW_formsId = fID,
                        name = x.DisplayName,
                        field = x.Field
                    });
                }

                // Types: update-or-insert
                if (existingTypes.TryGetValue(x.Field, out var tRow))
                {
                    tRow.type = x.Type;
                }
                else
                {
                    _context.SW_formTableData_Types.Add(new SW_formTableData_Type
                    {
                        SW_formsId = fID,
                        field = x.Field,
                        type = x.Type
                    });
                }
            }

            // OPTIONAL: prune rows removed from the builder (keeps DB in sync with current form)
            var prune = true; // set false if you prefer to retain old mappings
            if (prune)
            {
                var staleTypeIds = existingTypes.Values
                    .Where(t => !seen.Contains(t.field!))
                    .Select(t => t.Id)
                    .ToList();

                if (staleTypeIds.Count > 0)
                    await _context.SW_formTableData_Types
                        .Where(t => staleTypeIds.Contains(t.Id))
                        .ExecuteDeleteAsync();

                var staleNameIds = existingNames.Values
                    .Where(n => !seen.Contains(n.field!))
                    .Select(n => n.Id)
                    .ToList();

                if (staleNameIds.Count > 0)
                    await _context.SW_formTableNames
                        .Where(n => staleNameIds.Contains(n.Id))
                        .ExecuteDeleteAsync();
            }

            // Save atomically
            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }

            // After successful publish, go to the Program page for this form
            return RedirectToAction(nameof(Program), new { uuid = swForm.uuid });
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
            ViewBag.img = _context.SW_forms.Where(x => x.Id == id).Select(x => x.image).FirstOrDefault();
            ViewBag.appAmt = _context.SW_forms.Where(x => x.Id == id).Select(x => x.approvalAmt).FirstOrDefault();
            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name", sW_form.SW_identityId);
            return View(sW_form);
        }

        // POST: form/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,uuid,name,desc,form,dateModified,SW_identityId,is_linking,image,header,approvalAmt")] SW_form sW_form)
        {
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

        // GET: form/Edit/5
        public async Task<IActionResult> EditUpload(int? id)
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
            ViewBag.img = _context.SW_forms.Where(x => x.Id == id).Select(x => x.image).FirstOrDefault();
            ViewBag.appAmt = _context.SW_forms.Where(x => x.Id == id).Select(x => x.approvalAmt).FirstOrDefault();
            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name", sW_form.SW_identityId);
            return View(sW_form);
        }

        // POST: form/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUpload(int id, [Bind("Id,uuid,name,desc,form,dateModified,SW_identityId,is_linking,image,header,approvalAmt")] SW_form sW_form, IFormFile image)
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


        private static readonly Regex StaticKeyRx = new(@"^STATIC_[HP]_(\d{3})$", RegexOptions.Compiled);

        private static string EnsureName(string? nameFromBuilder, string? label, string fallbackPrefix, int indexWithinType)
        {
            var name = (nameFromBuilder ?? label)?.Trim();
            if (string.IsNullOrEmpty(name))
                name = $"{fallbackPrefix} {indexWithinType:D3}";
            return name;
        }

    }
}
