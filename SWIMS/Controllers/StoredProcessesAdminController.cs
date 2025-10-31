using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models;
using SWIMS.Models.Security;
using SWIMS.Models.ViewModels;

namespace SWIMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StoredProcessesAdminController : Controller
    {
        private readonly SwimsStoredProcsDbContext _db;
        private readonly IDataProtector? _protector;

        public StoredProcessesAdminController(SwimsStoredProcsDbContext db, IDataProtectionProvider dp)
        {
            _db = db;
            _protector = dp.CreateProtector(DataProtectionPurposes.StoredProcedures);
        }

        // GET: /StoredProcessesAdmin
        public async Task<IActionResult> Index()
        {
            var rows = await _db.StoredProcesses.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
            return View(rows);
        }

        // GET: /StoredProcessesAdmin/Create
        public IActionResult Create() => View(new StoredProcessEditViewModel());

        // POST: /StoredProcessesAdmin/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoredProcessEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (string.IsNullOrWhiteSpace(vm.ConnectionKey) &&
                (string.IsNullOrWhiteSpace(vm.DataSource) || string.IsNullOrWhiteSpace(vm.Database)))
            {
                ModelState.AddModelError(string.Empty, "Provide either a ConnectionKey or a DataSource + Database.");
                return View(vm);
            }

            var row = new StoredProcess
            {
                Name = vm.Name.Trim(),
                Description = vm.Description?.Trim(),
                ConnectionKey = string.IsNullOrWhiteSpace(vm.ConnectionKey) ? null : vm.ConnectionKey!.Trim(),
                DataSource = string.IsNullOrWhiteSpace(vm.DataSource) ? null : vm.DataSource!.Trim(),
                Database = string.IsNullOrWhiteSpace(vm.Database) ? null : vm.Database!.Trim(),
                DbUserEncrypted = string.IsNullOrWhiteSpace(vm.DbUser) ? null : Protect(vm.DbUser!),
                DbPasswordEncrypted = string.IsNullOrWhiteSpace(vm.DbPassword) ? null : Protect(vm.DbPassword!)
            };

            _db.StoredProcesses.Add(row);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /StoredProcessesAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var row = await _db.StoredProcesses.FindAsync(id);
            if (row == null) return NotFound();

            var vm = new StoredProcessEditViewModel
            {
                Id = row.Id,
                Name = row.Name,
                Description = row.Description,
                ConnectionKey = row.ConnectionKey,
                DataSource = row.DataSource,
                Database = row.Database
                // Do NOT echo creds
            };
            return View(vm);
        }

        // POST: /StoredProcessesAdmin/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StoredProcessEditViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            if (string.IsNullOrWhiteSpace(vm.ConnectionKey) &&
                (string.IsNullOrWhiteSpace(vm.DataSource) || string.IsNullOrWhiteSpace(vm.Database)))
            {
                ModelState.AddModelError(string.Empty, "Provide either a ConnectionKey or a DataSource + Database.");
                return View(vm);
            }

            var row = await _db.StoredProcesses.FirstOrDefaultAsync(x => x.Id == id);
            if (row == null) return NotFound();

            row.Name = vm.Name.Trim();
            row.Description = vm.Description?.Trim();
            row.ConnectionKey = string.IsNullOrWhiteSpace(vm.ConnectionKey) ? null : vm.ConnectionKey!.Trim();
            row.DataSource = string.IsNullOrWhiteSpace(vm.DataSource) ? null : vm.DataSource!.Trim();
            row.Database = string.IsNullOrWhiteSpace(vm.Database) ? null : vm.Database!.Trim();

            if (!string.IsNullOrWhiteSpace(vm.DbUser)) row.DbUserEncrypted = Protect(vm.DbUser!);
            if (!string.IsNullOrWhiteSpace(vm.DbPassword)) row.DbPasswordEncrypted = Protect(vm.DbPassword!);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /StoredProcessesAdmin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.StoredProcesses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (row == null) return NotFound();
            return View(row);
        }

        // POST: /StoredProcessesAdmin/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var row = await _db.StoredProcesses.Include(x => x.Params).FirstOrDefaultAsync(x => x.Id == id);
            if (row != null)
            {
                _db.StoredProcesses.Remove(row); // cascade deletes params
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private string Protect(string plaintext) =>
            _protector == null ? plaintext : _protector.Protect(plaintext);
    }
}
