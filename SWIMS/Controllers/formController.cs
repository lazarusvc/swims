using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Microsoft.SqlServer.Server;
using SWIMS.Data;
using SWIMS.Data.Lookups;
using SWIMS.Models;
using SWIMS.Models.Lookups;
using SWIMS.Services.Elsa;
using SWIMS.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;


namespace SWIMS.Controllers
{
    public class formController : Controller
    {
        private readonly SwimsDb_moreContext _context;
        private readonly SwimsLookupDbContext _lookup;
        private readonly IElsaWorkflowClient _elsa;

        public formController(SwimsDb_moreContext context, SwimsLookupDbContext lookup, IElsaWorkflowClient elsa)
        {
            _context = context;
            _lookup = lookup;
            _elsa = elsa;
        }

        /// <summary>
        /// Forms UUID
        /// </summary>
        /// 
        /// <remarks>
        /// Generates the routing unique that represents forms
        /// </remarks>
        /// 
        public string GenerateNewUuidAsString()
        {
            // Generates a new GUID and converts it to a string representation
            return Guid.NewGuid().ToString();
        }

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
                ViewBag.formLINK = _context.SW_forms.Where(x => x.is_linking == true).ToList();

            ViewBag.processes = await _context.SW_formProcesses
                .Where(c => c.SW_formsId == formId)
                .Select(c => new { c.name, c.url })      // anonymous with only what the view needs
                .ToListAsync();

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
                ViewBag.formLINK = _context.SW_forms.Where(x => x.is_linking == true).ToList();

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

        public IActionResult Approval(string? uuid)
        {
            var formId = Convert.ToInt32(_context.SW_forms.Where(m => m.uuid == uuid).Select(m => m.Id).FirstOrDefault());

            ViewBag.uuid = uuid;
            ViewBag.formId = formId;

            ViewBag.appAmt = Convert.ToInt32(_context.SW_forms.Where(m => m.uuid == uuid).Select(m => m.approvalAmt).FirstOrDefault());
            var al1 = _context.SW_formTableData.Where(x => x.isApproval_01 == 0 && x.SW_formsId == formId).ToList();
            var al2 = _context.SW_formTableData.Where(x => x.isApproval_02 == 0 && x.SW_formsId == formId).ToList();
            var al3 = _context.SW_formTableData.Where(x => x.isApproval_03 == 0 && x.SW_formsId == formId).ToList();
            var al4 = _context.SW_formTableData.Where(x => x.isApproval_04 == 0 && x.SW_formsId == formId).ToList();
            var al5 = _context.SW_formTableData.Where(x => x.isApproval_05 == 0 && x.SW_formsId == formId).ToList();
            ViewBag.appList01 = al1;
            ViewBag.appList02 = al2;
            ViewBag.appList03 = al3;
            ViewBag.appList04 = al4;
            ViewBag.appList05 = al5;
            ViewBag.appList01Ctn = al1.Count();
            ViewBag.appList02Ctn = al2.Count();
            ViewBag.appList03Ctn = al3.Count();
            ViewBag.appList04Ctn = al4.Count();
            ViewBag.appList05Ctn = al5.Count();

            return View();
        }

        public IActionResult ApprovalAction(int? dataID, int? appCnt, string uuid)
        {
            ViewBag.AppCnt = appCnt;
            ViewBag.dataID = dataID;
            ViewBag.uuid = uuid;
            ViewBag.formId = Convert.ToInt32(_context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.SW_formsId).FirstOrDefault());
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApprovalAction([FromBody] IFormCollection frm)
        {
            if (!int.TryParse(frm["Id"], out int dataID))
            {
                return BadRequest("Invalid ID");
            }

            var sw_frmData = await _context.SW_formTableData.FindAsync(dataID);

            if (sw_frmData == null)
            {
                return NotFound();
            }

            // Handle integers
            // --------------------------------------------------
            if (int.TryParse(frm["isApproval_01"], out int app1))
                sw_frmData.isApproval_01 = app1;

            if (int.TryParse(frm["isApproval_02"], out int app2))
                sw_frmData.isApproval_02 = app2;

            if (int.TryParse(frm["isApproval_03"], out int app3))
                sw_frmData.isApproval_03 = app3;

            if (int.TryParse(frm["isApproval_04"], out int app4))
                sw_frmData.isApproval_04 = app4;

            if (int.TryParse(frm["isApproval_05"], out int app5))
                sw_frmData.isApproval_05 = app5;

            // Handle comments (string fields)
            // --------------------------------------------------
            if (!string.IsNullOrWhiteSpace(frm["isAppComment_01"]))
                sw_frmData.isAppComment_01 = frm["isAppComment_01"];

            if (!string.IsNullOrWhiteSpace(frm["isAppComment_02"]))
                sw_frmData.isAppComment_02 = frm["isAppComment_02"];

            if (!string.IsNullOrWhiteSpace(frm["isAppComment_03"]))
                sw_frmData.isAppComment_03 = frm["isAppComment_03"];

            if (!string.IsNullOrWhiteSpace(frm["isAppComment_04"]))
                sw_frmData.isAppComment_04 = frm["isAppComment_04"];

            if (!string.IsNullOrWhiteSpace(frm["isAppComment_05"]))
                sw_frmData.isAppComment_05 = frm["isAppComment_05"];


            // Handle approvers (string fields)
            // --------------------------------------------------
            if (!string.IsNullOrWhiteSpace(frm["isApprover_01"]))
                sw_frmData.isApprover_01 = frm["isApprover_01"];

            if (!string.IsNullOrWhiteSpace(frm["isApprover_02"]))
                sw_frmData.isApprover_02 = frm["isApprover_02"];

            if (!string.IsNullOrWhiteSpace(frm["isApprover_03"]))
                sw_frmData.isApprover_03 = frm["isApprover_03"];

            if (!string.IsNullOrWhiteSpace(frm["isApprover_04"]))
                sw_frmData.isApprover_04 = frm["isApprover_04"];

            if (!string.IsNullOrWhiteSpace(frm["isApprover_05"]))
                sw_frmData.isApprover_05 = frm["isApprover_05"];


            // Handle approvers (datetime fields)
            // --------------------------------------------------
            if (DateTime.TryParse(frm["isApp_dateTime_01"], out var parsedDate1))
                sw_frmData.isApp_dateTime_01 = parsedDate1;

            if (DateTime.TryParse(frm["isApp_dateTime_02"], out var parsedDate2))
                sw_frmData.isApp_dateTime_02 = parsedDate2;

            if (DateTime.TryParse(frm["isApp_dateTime_03"], out var parsedDate3))
                sw_frmData.isApp_dateTime_03 = parsedDate3;

            if (DateTime.TryParse(frm["isApp_dateTime_04"], out var parsedDate4))
                sw_frmData.isApp_dateTime_04 = parsedDate4;

            if (DateTime.TryParse(frm["isApp_dateTime_05"], out var parsedDate5))
                sw_frmData.isApp_dateTime_05 = parsedDate5;


            string uuid = frm["uuid"].ToString();
            await _context.SaveChangesAsync();

            // 🔔 Notify: Approval progressed (next pending level or final approved)
            try
            {
                var formInfo = await _context.SW_forms
                    .AsNoTracking()
                    .Where(x => x.Id == sw_frmData.SW_formsId)
                    .Select(x => new { x.uuid, x.name, x.approvalAmt })
                    .FirstOrDefaultAsync(HttpContext.RequestAborted);

                var formUuid = !string.IsNullOrWhiteSpace(uuid) ? uuid : formInfo?.uuid;
                var formName = formInfo?.name ?? $"Form #{sw_frmData.SW_formsId}";
                var approvalAmt = formInfo?.approvalAmt ?? 0;

                if (approvalAmt > 0 && !string.IsNullOrWhiteSpace(formUuid))
                {
                    var nextPending = GetNextPendingApprovalLevel(sw_frmData, approvalAmt);

                    var actorName = User?.Identity?.Name ?? "A user";

                    if (nextPending.HasValue)
                    {
                        var lvl = nextPending.Value;

                        await NotifyApprovalEventAsync(
                            formId: sw_frmData.SW_formsId,
                            entryId: sw_frmData.Id,
                            approvalAmt: approvalAmt,
                            pendingLevel: lvl,
                            eventKey: SwimsEventKeys.Approvals.PendingForLevel(lvl),
                            subject: $"Approval required (Level {lvl})",
                            actorBody: $"Approval updated for entry #{sw_frmData.Id} on '{formName}'. Next: Level {lvl} of {approvalAmt}.",
                            routedBody: $"{actorName} advanced entry #{sw_frmData.Id} on '{formName}'. Next approval: Level {lvl} of {approvalAmt}.",
                            ct: HttpContext.RequestAborted,
                            formUuid: formUuid,
                            formName: formName,
                            recipientOverride: "system" // route-only
                        );
                    }
                    else
                    {
                        // final approval complete
                        await NotifyApprovalEventAsync(
                            formId: sw_frmData.SW_formsId,
                            entryId: sw_frmData.Id,
                            approvalAmt: approvalAmt,
                            pendingLevel: 0,
                            eventKey: SwimsEventKeys.Approvals.FinalApproved,
                            subject: "Final approval completed",
                            actorBody: $"You completed final approval for entry #{sw_frmData.Id} on '{formName}'.",
                            routedBody: $"{actorName} completed final approval for entry #{sw_frmData.Id} on '{formName}'.",
                            ct: HttpContext.RequestAborted,
                            formUuid: formUuid,
                            formName: formName,
                            recipientOverride: User?.FindFirstValue(ClaimTypes.NameIdentifier) // actor gets a record
                        );
                    }
                }
            }
            catch { }


            return RedirectToAction("Program", "Form", new { uuid });
        }

        public IActionResult ApprovalHistory(int? dataID, int? appCnt)
        {
            ViewBag.AppCnt = appCnt;
            ViewBag.dataID = dataID;
            ViewBag.AppNm1 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApprover_01).FirstOrDefault();
            ViewBag.AppNm2 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApprover_02).FirstOrDefault();
            ViewBag.AppNm3 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApprover_03).FirstOrDefault();
            ViewBag.AppNm4 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApprover_04).FirstOrDefault();
            ViewBag.AppNm5 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApprover_05).FirstOrDefault();
            ViewBag.AppCmt1 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isAppComment_01).FirstOrDefault();
            ViewBag.AppCmt2 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isAppComment_02).FirstOrDefault();
            ViewBag.AppCmt3 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isAppComment_03).FirstOrDefault();
            ViewBag.AppCmt4 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isAppComment_04).FirstOrDefault();
            ViewBag.AppCmt5 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isAppComment_05).FirstOrDefault();
            ViewBag.AppDate1 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApp_dateTime_01).FirstOrDefault()?.ToString("dd, MM yyyy");
            ViewBag.AppDate2 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApp_dateTime_02).FirstOrDefault()?.ToString("dd, MM yyyy");
            ViewBag.AppDate3 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApp_dateTime_03).FirstOrDefault()?.ToString("dd, MM yyyy");
            ViewBag.AppDate4 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApp_dateTime_04).FirstOrDefault()?.ToString("dd, MM yyyy");
            ViewBag.AppDate5 = _context.SW_formTableData.Where(m => m.Id == dataID).Select(m => m.isApp_dateTime_05).FirstOrDefault()?.ToString("dd, MM yyyy");

            return PartialView("Views/Shared/_ApprovalHistory.cshtml");
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
            var stringArray = new string[]
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
                _fData.FormData250,
                _fData.isAppComment_01,
                _fData.isAppComment_02,
                _fData.isAppComment_03,
                _fData.isAppComment_04,
                _fData.isAppComment_05,    
                _fData.isApprover_01,
                _fData.isApprover_02,
                _fData.isApprover_03,
                _fData.isApprover_04,
                _fData.isApprover_05,
                _fData.isLinkingForm
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
        
        public IActionResult Update(int? dataID, string? uuid)
        {
            // 0. Varibales
            // 
            var f_Linq = _context.SW_forms.Where(m => m.uuid.Equals(uuid));
            int formId = Convert.ToInt32(f_Linq.Select(m => m.Id).FirstOrDefault());
            ViewBag.dataID = Convert.ToInt32(dataID);
            ViewBag.uuid = uuid;
            ViewBag.formId = formId;
            ViewBag.form = f_Linq.Select(m => m.form).FirstOrDefault();
            ViewBag.formName = f_Linq.Select(m => m.name).FirstOrDefault();
            ViewBag.header = f_Linq.Select(x => x.header).FirstOrDefault();
            return View();
        }

        public IActionResult Linking(string? uuid, string? originUUID)
        {
            var formLINK = _context.SW_forms.Where(x => x.is_linking == true && x.uuid == uuid);

            ViewBag.header = formLINK.Select(x =>x.header).FirstOrDefault();
            ViewBag.form = formLINK.Select(x => x.form).FirstOrDefault();
            ViewBag.SW_formsId = formLINK.Select(x => x.Id).FirstOrDefault();
            ViewBag.isLinkingForm = originUUID;
            return PartialView("Views/Shared/_LinkingForm.cshtml");
        }

        // GET: form
        public async Task<IActionResult> Index()
        {
            // main forms list (already used by the view)
            var forms = await _context.SW_forms
                .Include(s => s.SW_identity)
                .AsNoTracking()
                .ToListAsync();

            var formIds = forms.Select(f => f.Id).ToList();

            // --- Form Type (0..1) display map ---
            var formTypeByFormId = await _lookup.SW_formFormTypes
                .Where(x => formIds.Contains(x.SW_formsId))
                .Join(_lookup.SW_formTypes,
                    link => link.SW_formTypeId,
                    type => type.Id,
                    (link, type) => new { link.SW_formsId, TypeName = type.name })
                .ToDictionaryAsync(x => x.SW_formsId, x => x.TypeName);

            // --- Program Tags (many-to-many) display map ---
            var programTagsByFormId = await _lookup.SW_formProgramTags
                .Where(x => formIds.Contains(x.SW_formsId))
                .Join(_lookup.SW_programTags,
                    link => link.SW_programTagId,
                    tag => tag.Id,
                    (link, tag) => new { link.SW_formsId, TagName = tag.name })
                .GroupBy(x => x.SW_formsId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(x => x.TagName).Distinct().OrderBy(n => n).ToList()
                );

            // pass to view (display only)
            ViewBag.FormTypeByFormId = formTypeByFormId;
            ViewBag.ProgramTagsByFormId = programTagsByFormId;

            return View(forms);
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
            PopulateFormClassificationDropdowns();
            return View();
        }

        // POST: form/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,uuid,name,desc,form,dateModified,SW_identityId,is_linking,image,header,approvalAmt")] SW_form sW_form,
            IFormFile image,
            int? formTypeId,
            int[] programTagIds
        )

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
                var createdForm = new SW_form
                {
                    uuid = sW_form.uuid,
                    name = sW_form.name,
                    desc = sW_form.desc,
                    form = sW_form.form,
                    dateModified = sW_form.dateModified,
                    SW_identityId = sW_form.SW_identityId,
                    is_linking = sW_form.is_linking,
                    image = uniqueFileName,
                    header = sW_form.header,
                    approvalAmt = sW_form.approvalAmt
                };

                _context.Add(createdForm);
                await _context.SaveChangesAsync();

                await SaveFormClassificationAsync(createdForm.Id, formTypeId, programTagIds);

                var actorName = User?.Identity?.Name ?? "A user";

                // 🔔 Notify: Form Created
                await NotifyFormEventAsync(
                    formId: createdForm.Id,
                    eventKey: SwimsEventKeys.Forms.DefinitionCreated,
                    subject: "Form created",
                    body: $"You created form '{createdForm.name}'.",
                    ct: HttpContext.RequestAborted,
                    url: Url.Action(nameof(Details), new { id = createdForm.Id }),
                    formUuid: createdForm.uuid,
                    formName: createdForm.name,
                    extraMeta: new { identityId = createdForm.SW_identityId },
                    texts: new
                    {
                        actor = new { subject = "Form created", body = $"You created form '{createdForm.name}'." },
                        routed = new { subject = "Form created", body = $"{actorName} created form '{createdForm.name}'." },
                        superadmin = new { subject = "Form created", body = $"{actorName} created form '{createdForm.name}'." }
                    }
                );



                return RedirectToAction(nameof(Index));
            }


            ViewData["SW_identityId"] = new SelectList(_context.SW_identities, "Id", "name", sW_form.SW_identityId);
            PopulateFormClassificationDropdowns(formTypeId, programTagIds);
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

            if (formDefinition == null || formDefinition.Count == 0)
                return BadRequest("Form definition empty or invalid JSON");

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
                // IMPORTANT: do NOT treat data-ish custom controls as STATIC_* here.
                // beneficiary / organization / city / financial_institution should keep whatever
                // naming scheme the builder/runtime expects.
            }

            // ---------------------------
            // UPSERT (dedupe-safe)
            // ---------------------------

            // ✅ CHANGED: Build incoming rows and dedupe by Field (prevents inserting duplicates)
            var incomingRaw = formDefinition
                .Where(d => !string.IsNullOrWhiteSpace(d.name))
                .Select(d => new
                {
                    Field = d.name!.Trim(),
                    DisplayName = (d.label ?? d.name)!.Trim(),
                    Type = d.type?.Trim().ToLowerInvariant() ?? "text"
                })
                .ToList();

            // ✅ CHANGED: Deduplicate incoming by Field (case-insensitive). "Last wins".
            var incoming = incomingRaw
                .GroupBy(x => x.Field, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last())
                .ToList();

            // ✅ CHANGED: Load existing rows as lists (avoids ToDictionary duplicate-key crash)
            var existingNameList = await _context.SW_formTableNames
                .Where(n => n.SW_formsId == fID && n.field != null)
                .ToListAsync();

            var existingTypeList = await _context.SW_formTableData_Types
                .Where(t => t.SW_formsId == fID && t.field != null)
                .ToListAsync();

            // ✅ OPTIONAL (recommended): clean existing duplicates in DB so we stop carrying bad state
            // Keep the lowest Id row for each field; delete the rest.
            var dupNameIds = existingNameList
                .GroupBy(n => n.field!, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(x => x.Id).Skip(1).Select(x => x.Id))
                .ToList();

            if (dupNameIds.Count > 0)
            {
                await _context.SW_formTableNames
                    .Where(n => dupNameIds.Contains(n.Id))
                    .ExecuteDeleteAsync();

                existingNameList = existingNameList
                    .Where(n => !dupNameIds.Contains(n.Id))
                    .ToList();
            }

            var dupTypeIds = existingTypeList
                .GroupBy(t => t.field!, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(x => x.Id).Skip(1).Select(x => x.Id))
                .ToList();

            if (dupTypeIds.Count > 0)
            {
                await _context.SW_formTableData_Types
                    .Where(t => dupTypeIds.Contains(t.Id))
                    .ExecuteDeleteAsync();

                existingTypeList = existingTypeList
                    .Where(t => !dupTypeIds.Contains(t.Id))
                    .ToList();
            }

            // ✅ CHANGED: Build dictionaries from de-duped lists
            var existingNames = existingNameList
                .GroupBy(n => n.field!, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderBy(x => x.Id).First())
                .ToDictionary(x => x.field!, StringComparer.OrdinalIgnoreCase);

            var existingTypes = existingTypeList
                .GroupBy(t => t.field!, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderBy(x => x.Id).First())
                .ToDictionary(x => x.field!, StringComparer.OrdinalIgnoreCase);

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
                    var newName = new SW_formTableName
                    {
                        SW_formsId = fID,
                        name = x.DisplayName,
                        field = x.Field
                    };
                    _context.SW_formTableNames.Add(newName);

                    // ✅ CHANGED: update dictionary so duplicates in incoming don't create duplicates in DB
                    existingNames[x.Field] = newName;
                }

                // Types: update-or-insert
                if (existingTypes.TryGetValue(x.Field, out var tRow))
                {
                    tRow.type = x.Type;
                }
                else
                {
                    var newType = new SW_formTableData_Type
                    {
                        SW_formsId = fID,
                        field = x.Field,
                        type = x.Type
                    };
                    _context.SW_formTableData_Types.Add(newType);

                    // ✅ CHANGED: update dictionary so duplicates in incoming don't create duplicates in DB
                    existingTypes[x.Field] = newType;
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
                {
                    var staleTypes = await _context.SW_formTableData_Types
                        .Where(t => staleTypeIds.Contains(t.Id))
                        .ToListAsync();

                    _context.SW_formTableData_Types.RemoveRange(staleTypes);
                }

                var staleNameIds = existingNames.Values
                    .Where(n => !seen.Contains(n.field!))
                    .Select(n => n.Id)
                    .ToList();

                if (staleNameIds.Count > 0)
                {
                    var staleNames = await _context.SW_formTableNames
                        .Where(n => staleNameIds.Contains(n.Id))
                        .ToListAsync();

                    _context.SW_formTableNames.RemoveRange(staleNames);
                }
            }

            // Save atomically
            await _context.SaveChangesAsync();

            var actorName = User?.Identity?.Name ?? "A user";

            // 🔔 Notify: Form published
            await NotifyFormEventAsync(
                formId: fID,
                eventKey: SwimsEventKeys.Forms.DefinitionPublished,
                subject: "Form published",
                body: $"You published form '{swForm.name}'.",
                ct: HttpContext.RequestAborted,
                url: Url.Action(nameof(Program), new { uuid = swForm.uuid }),
                formUuid: swForm.uuid,
                formName: swForm.name,
                extraMeta: new { published = true },
                texts: new
                {
                    actor = new { subject = "Form published", body = $"You published form '{swForm.name}'." },
                    routed = new { subject = "Form published", body = $"{actorName} published form '{swForm.name}'." },
                    superadmin = new { subject = "Form published", body = $"{actorName} published form '{swForm.name}'." }
                }
            );
            // 🔔 Notify: Form published (end)


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

            var selectedTypeId = await _lookup.SW_formFormTypes
                .Where(x => x.SW_formsId == sW_form.Id)
                .Select(x => (int?)x.SW_formTypeId)
                .SingleOrDefaultAsync();

            var selectedTags = await _lookup.SW_formProgramTags
                .Where(x => x.SW_formsId == sW_form.Id)
                .Select(x => x.SW_programTagId)
                .ToListAsync();

            PopulateFormClassificationDropdowns(selectedTypeId, selectedTags);


            return View(sW_form);
        }

        // POST: form/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            // ✅ Bind only properties that actually exist + are editable
            [Bind("Id,uuid,name,form,image,approvalAmt,SW_identityId,is_linking,header,desc")]
    SW_form posted,
            IFormFile? imageFile,
            string? formFile,
            int? formTypeId,
            int[]? programTagIds
        )
        {
            if (id != posted.Id) return NotFound();

            // ✅ Load TRACKED entity so we only update allowed fields
            var existing = await _context.SW_forms
                .FirstOrDefaultAsync(f => f.Id == id);

            if (existing == null) return NotFound();

            // Normalize classification inputs
            if (formTypeId.HasValue && formTypeId.Value <= 0) formTypeId = null;

            programTagIds ??= Array.Empty<int>();
            programTagIds = programTagIds.Where(x => x > 0).Distinct().ToArray();

            if (!ModelState.IsValid)
            {
                PopulateFormClassificationDropdowns(id);
                ViewBag.SW_identityId = new SelectList(_context.SW_identities, "Id", "name", posted.SW_identityId);

                ViewBag.frm = !string.IsNullOrWhiteSpace(formFile)
                    ? formFile
                    : (!string.IsNullOrWhiteSpace(posted.form) ? posted.form : existing.form);

                ViewBag.img = string.IsNullOrWhiteSpace(posted.image) ? existing.image : posted.image;
                ViewBag.appAmt = posted.approvalAmt;

                return View(posted);
            }

            // ✅ Update allowed fields only
            existing.name = posted.name;
            existing.desc = posted.desc;
            existing.header = posted.header;
            existing.SW_identityId = posted.SW_identityId;
            existing.is_linking = posted.is_linking;
            existing.approvalAmt = posted.approvalAmt;

            // ✅ Update form JSON safely (prefer formFile if provided)
            if (!string.IsNullOrWhiteSpace(formFile))
                existing.form = formFile;
            else if (!string.IsNullOrWhiteSpace(posted.form))
                existing.form = posted.form;
            // else preserve existing.form

            // ✅ Image handling
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                existing.image = uniqueFileName;
            }
            else
            {
                // preserve existing unless the page explicitly posted something else
                if (!string.IsNullOrWhiteSpace(posted.image))
                    existing.image = posted.image;
            }

            // ✅ IMPORTANT: update modification timestamp
            existing.dateModified = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

                // ✅ Save FormType + ProgramTags selection on edit
                await SaveFormClassificationAsync(existing.Id, formTypeId, programTagIds);

                // 🔔 Notify: Form updated
                var actorName = User?.Identity?.Name ?? "A user";

                await NotifyFormEventAsync(
                    formId: existing.Id,
                    eventKey: SwimsEventKeys.Forms.DefinitionUpdated,
                    subject: "Form updated",
                    body: $"You updated form '{existing.name}'.",
                    ct: HttpContext.RequestAborted,
                    url: Url.Action(nameof(Details), new { id = existing.Id }),
                    formUuid: existing.uuid,
                    formName: existing.name,
                    extraMeta: new
                    {
                        identityId = existing.SW_identityId,
                        updatedImage = imageFile != null
                    },
                    texts: new
                    {
                        actor = new { subject = "Form updated", body = $"You updated form '{existing.name}'." },
                        routed = new { subject = "Form updated", body = $"{actorName} updated form '{existing.name}'." },
                        superadmin = new { subject = "Form updated", body = $"{actorName} updated form '{existing.name}'." }
                    }
                );

                // 🔔 Notify: Form updated (end)



                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SW_formExists(existing.Id)) return NotFound();
                throw;
            }
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

                    // 🔔 Notify: Form image updated
                    var actorName = User?.Identity?.Name ?? "A user";

                    await NotifyFormEventAsync(
                        formId: sW_form.Id,
                        eventKey: SwimsEventKeys.Forms.DefinitionUpdated,
                        subject: "Form image updated",
                        body: $"You updated the image for form '{sW_form.name}'.",
                        ct: HttpContext.RequestAborted,
                        url: Url.Action(nameof(Details), new { id = sW_form.Id }),
                        formUuid: sW_form.uuid,
                        formName: sW_form.name,
                        extraMeta: new { updatedImage = true },
                        texts: new
                        {
                            actor = new { subject = "Form image updated", body = $"You updated the image for form '{sW_form.name}'." },
                            routed = new { subject = "Form image updated", body = $"{actorName} updated the image for form '{sW_form.name}'." },
                            superadmin = new { subject = "Form image updated", body = $"{actorName} updated the image for form '{sW_form.name}'." }
                        }
                    );

                    // 🔔 Notify: Form image updated (end)


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

            var typeLink = await _lookup.SW_formFormTypes.SingleOrDefaultAsync(x => x.SW_formsId == id);
            if (typeLink != null) _lookup.SW_formFormTypes.Remove(typeLink);

            var tagLinks = await _lookup.SW_formProgramTags.Where(x => x.SW_formsId == id).ToListAsync();
            if (tagLinks.Count > 0) _lookup.SW_formProgramTags.RemoveRange(tagLinks);

            await _lookup.SaveChangesAsync();


            // finally remove form
            var sW_form = await _context.SW_forms.FindAsync(id);
            if (sW_form != null)
            {
                _context.SW_forms.Remove(sW_form);
            }

            await _context.SaveChangesAsync();

            // 🔔 Notify: Form deleted
            var actorName = User?.Identity?.Name ?? "A user";

            var formName = sW_form?.name ?? $"Form #{id}";
            var formUuid = sW_form?.uuid;

            await NotifyFormEventAsync(
                formId: id,
                eventKey: SwimsEventKeys.Forms.DefinitionDeleted,
                subject: "Form deleted",
                body: $"You deleted form '{formName}'.",
                ct: HttpContext.RequestAborted,
                url: Url.Action(nameof(Index)),
                formUuid: formUuid,
                formName: formName,
                extraMeta: new { deleted = true },
                texts: new
                {
                    actor = new { subject = "Form deleted", body = $"You deleted form '{formName}'." },
                    routed = new { subject = "Form deleted", body = $"{actorName} deleted form '{formName}'." },
                    superadmin = new { subject = "Form deleted", body = $"{actorName} deleted form '{formName}'." }
                }
            );

            // 🔔 Notify: Form deleted (end)



            return RedirectToAction(nameof(Index));
        }

        private async Task NotifyFormEventAsync(
      int formId,
      string eventKey,
      string subject,
      string body,
      CancellationToken ct = default,
      string? url = null,
      object? extraMeta = null,
      string? recipientOverride = null,
      int? targetUserId = null,
      IEnumerable<int>? targetUserIds = null,
      object? texts = null,
      string? formUuid = null,
      string? formName = null)
        {
            try
            {
                var recipient = recipientOverride
                    ?? User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User?.Identity?.Name;

                if (string.IsNullOrWhiteSpace(recipient))
                    return;

                var payload = new
                {
                    Recipient = recipient,
                    Channel = "InApp",
                    Subject = subject,
                    Body = body,

                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        type = "Forms",
                        eventKey,
                        url,
                        metadata = new
                        {
                            formId,
                            formUuid,
                            formName,

                            actorUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                            actorUserName = User?.Identity?.Name,

                            targetUserId,
                            targetUserIds = targetUserIds?.ToArray(),

                            texts = texts,
                            extra = extraMeta
                        }
                    })
                };

                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch { }
        }

        private static int? GetNextPendingApprovalLevel(SW_formTableDatum d, int approvalAmt)
        {
            if (approvalAmt <= 0) return null;

            var max = Math.Min(approvalAmt, 5);

            bool IsApproved(int level) => level switch
            {
                1 => (d.isApproval_01 ?? 0) == 1,
                2 => (d.isApproval_02 ?? 0) == 1,
                3 => (d.isApproval_03 ?? 0) == 1,
                4 => (d.isApproval_04 ?? 0) == 1,
                5 => (d.isApproval_05 ?? 0) == 1,
                _ => true
            };

            for (var i = 1; i <= max; i++)
                if (!IsApproved(i))
                    return i;

            return null; // all required approvals complete
        }

        private async Task NotifyApprovalEventAsync(
            int formId,
            int entryId,
            int approvalAmt,
            int pendingLevel,
            string eventKey,
            string subject,
            string actorBody,
            string routedBody,
            CancellationToken ct = default,
            string? formUuid = null,
            string? formName = null,
            string? recipientOverride = null)
        {
            try
            {
                var recipient =
                    recipientOverride
                    ?? User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User?.Identity?.Name;

                if (string.IsNullOrWhiteSpace(recipient))
                    return;

                string? url = null;

                if (!string.IsNullOrWhiteSpace(formUuid))
                {
                    url = pendingLevel >= 1
                        ? Url.Action("ApprovalAction", "form", new { dataId = entryId, appCnt = pendingLevel, uuid = formUuid })
                        : Url.Action("Approval", "form", new { uuid = formUuid });
                }

                var payload = new
                {
                    Recipient = recipient,
                    Channel = "InApp",
                    Subject = subject,
                    Body = actorBody,
                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        type = "Approvals",
                        eventKey,
                        url,
                        metadata = new
                        {
                            formId,
                            formName,
                            formUuid,
                            entryId,
                            approvalAmt,
                            pendingLevel,

                            actorUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                            actorUserName = User?.Identity?.Name,

                            texts = new
                            {
                                actor = new { subject, body = actorBody },
                                routed = new { subject, body = routedBody },
                                superadmin = new { subject, body = routedBody }
                            }
                        }
                    })
                };

                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch { }
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

        private void PopulateFormClassificationDropdowns(int? selectedFormTypeId = null, IEnumerable<int>? selectedProgramTagIds = null)
        {
            ViewData["formTypeId"] = new SelectList(
                _lookup.SW_formTypes.OrderBy(x => x.name),
                "Id",
                "name",
                selectedFormTypeId
            );

            ViewData["programTagIds"] = new MultiSelectList(
                _lookup.SW_programTags.OrderBy(x => x.name),
                "Id",
                "name",
                selectedProgramTagIds
            );
        }

        private async Task SaveFormClassificationAsync(int formId, int? formTypeId, int[]? programTagIds)
        {
            // --- Form Type: 0..1 ---
            var existingType = await _lookup.SW_formFormTypes.SingleOrDefaultAsync(x => x.SW_formsId == formId);
            if (existingType != null)
                _lookup.SW_formFormTypes.Remove(existingType);

            if (formTypeId.HasValue)
            {
                _lookup.SW_formFormTypes.Add(new SW_formFormType
                {
                    SW_formsId = formId,
                    SW_formTypeId = formTypeId.Value
                });
            }

            // --- Program Tags: many-to-many ---
            var existingTags = await _lookup.SW_formProgramTags.Where(x => x.SW_formsId == formId).ToListAsync();
            if (existingTags.Count > 0)
                _lookup.SW_formProgramTags.RemoveRange(existingTags);

            if (programTagIds != null && programTagIds.Length > 0)
            {
                foreach (var tagId in programTagIds.Distinct())
                {
                    _lookup.SW_formProgramTags.Add(new SW_formProgramTag
                    {
                        SW_formsId = formId,
                        SW_programTagId = tagId
                    });
                }
            }

            await _lookup.SaveChangesAsync();
        }


    }
}