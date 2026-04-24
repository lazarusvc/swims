// -------------------------------------------------------------------
// File:    HomeController.cs
// Author:  N/A
// Created: N/A
// Purpose: Controller for public-facing pages including home, privacy, and error.
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using System.Diagnostics;

namespace SWIMS.Controllers
{
    public class HomeController(
        SwimsDb_moreContext context,
        ILogger<HomeController> logger,
        IMemoryCache cache) : Controller
    {
        private readonly SwimsDb_moreContext _context = context;
        private readonly ILogger<HomeController> _logger = logger;
        private readonly IMemoryCache _cache = cache;
        private const string FormsCacheKey = "home_sw_forms";

        private async Task<List<dynamic>> GetFormsAsync()
        {
            if (_cache.TryGetValue(FormsCacheKey, out List<dynamic>? cached) && cached is not null)
                return cached;
            var forms = (await _context.SW_forms
                .AsNoTracking()
                .OrderByDescending(x => x.dateModified)
                .Select(f => new { f.Id, f.name, f.desc, f.image, f.uuid, f.dateModified })
                .ToListAsync())
                .Cast<dynamic>()
                .ToList();
            _cache.Set(FormsCacheKey, forms,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return forms;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.frmBtn = await GetFormsAsync();
            return View();
        }

        public async Task<IActionResult> ProgramDashboard()
        {
            ViewBag.frmBtn = await GetFormsAsync();
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
