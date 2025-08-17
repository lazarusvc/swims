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
        public async Task<IActionResult> Run(int id)
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
        public async Task<IActionResult> Run(int id, RunStoredProcessViewModel model)
        {
            var sp = await _db.StoredProcesses
                              .Include(x => x.Params)
                              .FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            // Update param values (no raw SQL)
            var map = sp.Params.ToDictionary(p => p.Id);
            foreach (var p in model.Params)
            {
                if (map.TryGetValue(p.Id, out var row))
                {
                    row.Value = p.Value;
                }
            }
            await _db.SaveChangesAsync();

            var (table, error) = await _runner.ExecuteAsync(sp, sp.Params);
            var resultVm = new RunStoredProcessResultViewModel
            {
                ProcessId = id,
                Name = sp.Name,
                Description = sp.Description,
                Error = error,
                Table = table
            };
            return View("RunResult", resultVm);
        }

        [HttpGet]
        public async Task<IActionResult> Export(int id, string format = "csv")
        {
            // re-run with the latest saved params
            var sp = await _db.StoredProcesses
                              .Include(x => x.Params)
                              .FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            var (table, error) = await _runner.ExecuteAsync(sp, sp.Params);
            if (!string.IsNullOrWhiteSpace(error) || table is null)
            {
                TempData["Error"] = error ?? "No data returned.";
                return RedirectToAction(nameof(Run), new { id });
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


    }
}
