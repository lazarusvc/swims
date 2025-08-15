using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using SWIMS.Services;

namespace SWIMS.Controllers
{
    [Authorize(Roles = "Admin")] // adjust role name if needed
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
        public async Task<IActionResult> Run(int id)
        {
            var sp = await _db.StoredProcesses
                               .Include(x => x.Params)
                               .FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            var vm = new RunStoredProcessVm
            {
                ProcessId = sp.Id,
                Name = sp.Name,
                Description = sp.Description,
                ConnectionDisplay = !string.IsNullOrWhiteSpace(sp.ConnectionKey)
                    ? $"Connection: {sp.ConnectionKey}"
                    : $"{sp.DataSource}/{sp.Database}",
                Params = sp.Params
                          .OrderBy(p => p.Key)
                          .Select(p => new RunParamVm { Id = p.Id, Key = p.Key, DataType = p.DataType, Value = p.Value })
                          .ToList()
            };
            return View(vm);
        }

        // POST: /StoredProcesses/Run/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(int id, RunStoredProcessVm model)
        {
            var sp = await _db.StoredProcesses
                               .Include(x => x.Params)
                               .FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            // Update param values (no raw SQL)
            var map = sp.Params.ToDictionary(p => p.Id);
            foreach (var p in model.Params)
                if (map.TryGetValue(p.Id, out var row)) row.Value = p.Value;
            await _db.SaveChangesAsync();

            var (table, error) = await _runner.ExecuteAsync(sp, sp.Params);
            var vm = new RunStoredProcessResultVm
            {
                Name = sp.Name,
                Description = sp.Description,
                Error = error,
                Table = table
            };
            return View("RunResult", vm);
        }
    }

    // ViewModels
    public class RunStoredProcessVm
    {
        public int ProcessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ConnectionDisplay { get; set; }
        public List<RunParamVm> Params { get; set; } = new();
    }

    public class RunParamVm
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string DataType { get; set; } = "NVarChar";
        public string? Value { get; set; }
    }

    public class RunStoredProcessResultVm
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Error { get; set; }
        public DataTable? Table { get; set; }
    }
}