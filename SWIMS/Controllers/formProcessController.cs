using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using SWIMS.Data;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    public class formProcessController : Controller
    {
        private readonly SwimsDb_moreContext _context;
        private readonly SwimsStoredProcsDbContext _context_sp;

        private static int? TryExtractProcIdFromRunUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var m = Regex.Match(url, @"/StoredProcesses/Run/(\d+)", RegexOptions.IgnoreCase);
            return m.Success ? int.Parse(m.Groups[1].Value) : (int?)null;
        }

        public formProcessController(SwimsDb_moreContext context, SwimsStoredProcsDbContext sp)
        {
            _context = context;
            _context_sp = sp;
        }

        // GET: formProcess
        public async Task<IActionResult> Index()
        {
            return View(await _context.SW_formProcesses.ToListAsync());
        }

        // GET: formProcess/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }

            return View(sW_formProcess);
        }

        // GET: formProcess/Create
        public IActionResult Create()
        {
            ViewBag.processes = _context_sp.StoredProcesses
                .Select(c => new SelectListItem()
                {
                    Text = c.Name,
                    Value = "../StoredProcesses/Run/" + Convert.ToString(c.Id)
                })
                .ToList();

            ViewData["SW_formsId"] = new SelectList(_context.SW_forms, "Id", "name");
            return View();
        }

        // POST: formProcess/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,url,name,SW_formsId")] SW_formProcess sW_formProcess)
        {
            if (ModelState.IsValid)
            {
                // --- Auto-fill name from Stored Procedure when blank ---
                if (string.IsNullOrWhiteSpace(sW_formProcess.name))
                {
                    var procId = TryExtractProcIdFromRunUrl(sW_formProcess.url);
                    if (procId.HasValue)
                    {
                        var spName = await _context_sp.StoredProcesses
                            .Where(x => x.Id == procId.Value)
                            .Select(x => x.Name)
                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrWhiteSpace(spName))
                            sW_formProcess.name = spName;
                    }
                }
                // -------------------------------------------------------

                _context.Add(sW_formProcess);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sW_formProcess);
        }

        // GET: formProcess/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses.FindAsync(id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }
            return View(sW_formProcess);
        }

        // POST: formProcess/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,url,name,SW_formsId")] SW_formProcess sW_formProcess)
        {
            if (id != sW_formProcess.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // --- Auto-fill name from Stored Procedure when blank (on edit too) ---
                    if (string.IsNullOrWhiteSpace(sW_formProcess.name))
                    {
                        var procId = TryExtractProcIdFromRunUrl(sW_formProcess.url);
                        if (procId.HasValue)
                        {
                            var spName = await _context_sp.StoredProcesses
                                .Where(x => x.Id == procId.Value)
                                .Select(x => x.Name)
                                .FirstOrDefaultAsync();

                            if (!string.IsNullOrWhiteSpace(spName))
                                sW_formProcess.name = spName;
                        }
                    }
                    // ---------------------------------------------------------------------

                    _context.Update(sW_formProcess);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SW_formProcessExists(sW_formProcess.Id))
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
            return View(sW_formProcess);
        }

        // GET: formProcess/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sW_formProcess = await _context.SW_formProcesses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sW_formProcess == null)
            {
                return NotFound();
            }

            return View(sW_formProcess);
        }

        // POST: formProcess/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sW_formProcess = await _context.SW_formProcesses.FindAsync(id);
            if (sW_formProcess != null)
            {
                _context.SW_formProcesses.Remove(sW_formProcess);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SW_formProcessExists(int id)
        {
            return _context.SW_formProcesses.Any(e => e.Id == id);
        }
    }
}
