using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models.ViewModels;
using SWIMS.Services;

namespace SWIMS.Controllers
{
    [Authorize(Roles = "Admin")] // or "Admin,Runner" if operators should run
    public class StoredProcessesController : Controller
    {
        private readonly SwimsStoredProcsDbContext _db;
        private readonly StoredProcedureRunner _runner;

        public StoredProcessesController(SwimsStoredProcsDbContext db, StoredProcedureRunner runner)
        {
            _db = db;
            _runner = runner;
        }

        // GET: /StoredProcesses
        public async Task<IActionResult> Index()
        {
            var procs = await _db.StoredProcesses
                                 .AsNoTracking()
                                 .OrderBy(x => x.Name)
                                 .ToListAsync();
            return View(procs);
        }

        // GET: /StoredProcesses/Run/5
        [HttpGet]
        public async Task<IActionResult> Run(int id, int? formId = null, int? orgId = null)
        {
            var sp = await _db.StoredProcesses
                              .Include(x => x.Params)
                              .FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            var vm = new RunStoredProcessViewModel
            {
                ProcessId = sp.Id,
                Name = sp.Name,
                Description = sp.Description,
                ConnectionDisplay = !string.IsNullOrWhiteSpace(sp.ConnectionKey)
                    ? $"Connection: {sp.ConnectionKey}"
                    : $"{sp.DataSource}/{sp.Database}",
                Params = sp.Params
                           .OrderBy(p => p.Key)
                           .Select(p => new RunParamViewModel
                           {
                               Id = p.Id,
                               Key = p.Key,
                               DataType = p.DataType,
                               Value = p.Value
                           })
                           .ToList()
            };
            return View(vm);
        }

        // POST: /StoredProcesses/Run/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(int id, RunStoredProcessViewModel model, int? formId = null, int? orgId = null)
        {
            var sp = await _db.StoredProcesses.Include(x => x.Params).FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            // persist edited values
            var map = sp.Params.ToDictionary(p => p.Id);
            foreach (var p in model.Params)
                if (map.TryGetValue(p.Id, out var row)) row.Value = p.Value;
            await _db.SaveChangesAsync();

            // --- uuid-aware tokenization (unchanged) ---
            var uid = Request.Query["uid"].FirstOrDefault()
                   ?? Request.Form["uid"].FirstOrDefault()
                   ?? Request.Query["uuid"].FirstOrDefault()
                   ?? Request.Query["UID"].FirstOrDefault();

            var ctx = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FormId"] = formId?.ToString() ?? string.Empty,
                ["OrganizationId"] = orgId?.ToString() ?? string.Empty,
                ["FormUUID"] = uid ?? string.Empty,
                ["UserName"] = User?.Identity?.Name ?? "system"
            };
            var tokenizedParams = ApplyTokens(sp.Params, ctx);

            // ✅ Stash context so Export (GET) doesn’t read Request.Form
            TempData["uid"] = uid ?? string.Empty;
            TempData["formId"] = formId?.ToString() ?? string.Empty;
            TempData["orgId"] = orgId?.ToString() ?? string.Empty;
            TempData.Keep();

            var (table, error) = await _runner.ExecuteAsync(sp, tokenizedParams);

            // make context available to the RunResult view so Export can include it
            ViewBag.uid = uid;
            ViewBag.formId = formId;
            ViewBag.orgId = orgId;

            return View("RunResult", new RunStoredProcessResultViewModel
            {
                ProcessId = id,
                Name = sp.Name,
                Description = sp.Description,
                Error = error,
                Table = table
            });
        }

        [HttpGet]
        public async Task<IActionResult> Export(int id, string format = "csv", int? formId = null, int? orgId = null)
        {
            var sp = await _db.StoredProcesses.Include(x => x.Params).FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            // ✅ Only read Query on GET; fall back to TempData.Peek
            var uidQ = Request.Query["uid"].FirstOrDefault()
                    ?? Request.Query["uuid"].FirstOrDefault()
                    ?? Request.Query["UID"].FirstOrDefault()
                    ?? (TempData.Peek("uid") as string);

            var formIdStr = formId?.ToString() ?? (TempData.Peek("formId") as string ?? string.Empty);
            var orgIdStr = orgId?.ToString() ?? (TempData.Peek("orgId") as string ?? string.Empty);

            var ctx = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FormId"] = formIdStr,
                ["OrganizationId"] = orgIdStr,
                ["FormUUID"] = uidQ ?? string.Empty,
                ["UserName"] = User?.Identity?.Name ?? "system"
            };
            var tokenizedParams = ApplyTokens(sp.Params, ctx);

            var (table, error) = await _runner.ExecuteAsync(sp, tokenizedParams);
            if (!string.IsNullOrWhiteSpace(error) || table is null)
            {
                TempData["Error"] = error ?? "No data returned.";
                return RedirectToAction(nameof(Run), new { id, formId = formIdStr, orgId = orgIdStr, uid = uidQ });
            }

            if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only CSV is supported right now.");

            var csv = DataTableToCsv(table);
            var fileName = $"{sp.Name.Replace(':', '_').Replace('/', '_')}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        private static string DataTableToCsv(System.Data.DataTable dt)
        {
            var sb = new System.Text.StringBuilder();

            // headers
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(EscapeCsv(dt.Columns[i].ColumnName));
            }
            sb.AppendLine();

            // rows
            foreach (System.Data.DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    var val = row[i]?.ToString() ?? string.Empty;
                    sb.Append(EscapeCsv(val));
                }
                sb.AppendLine();
            }

            return sb.ToString();

            static string EscapeCsv(string s)
            {
                // wrap in quotes if it contains comma, quote, or newline; double the quotes inside
                var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                if (needsQuotes)
                    return $"\"{s.Replace("\"", "\"\"")}\"";
                return s;
            }
        }

        // ------------------- TOKEN HELPERS (tiny, self-contained) -------------------
        private static string ReplaceTokens(string? value, IDictionary<string, string> ctx)
        {
            if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
            foreach (var kv in ctx)
                value = value.Replace("{" + kv.Key + "}", kv.Value, StringComparison.OrdinalIgnoreCase);
            return value;
        }

        private static IEnumerable<SWIMS.Models.StoredProcessParam> ApplyTokens(
            IEnumerable<SWIMS.Models.StoredProcessParam> src,
            IDictionary<string, string> ctx)
        {
            foreach (var p in src)
            {
                // return a transient copy — DB values remain unchanged
                yield return new SWIMS.Models.StoredProcessParam
                {
                    Id = p.Id,
                    StoredProcessId = p.StoredProcessId,
                    Key = p.Key,
                    DataType = p.DataType,
                    Value = ReplaceTokens(p.Value, ctx)
                };
            }
        }
        // ---------------------------------------------------------------------------
    }
}