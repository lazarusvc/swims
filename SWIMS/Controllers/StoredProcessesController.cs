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
                Name = sp.Name,
                Description = sp.Description,
                Error = error,
                Table = table
            };
            return View("RunResult", resultVm);
        }
    }
}
