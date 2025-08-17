using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Areas.Admin.ViewModels.AccessControl;
using SWIMS.Data;
using SWIMS.Models.Security;
using SWIMS.Services.Auth;
using SWIMS.Services.Diagnostics;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PublicEndpointsController : Controller
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IPublicAccessStore _store;
        private readonly IEndpointCatalog _catalog;

        public PublicEndpointsController(SwimsIdentityDbContext db, IPublicAccessStore store, IEndpointCatalog catalog)
        { _db = db; _store = store; _catalog = catalog; }

        public async Task<IActionResult> Index()
        {
            var rows = await _db.PublicEndpoints.AsNoTracking()
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.MatchType)
                .Select(x => new PublicEndpointListItemViewModel
                {
                    Id = x.Id,
                    MatchType = x.MatchType,
                    Area = x.Area,
                    Controller = x.Controller,
                    Action = x.Action,
                    Page = x.Page,
                    Path = x.Path,
                    Regex = x.Regex,
                    Notes = x.Notes,
                    IsEnabled = x.IsEnabled,
                    Priority = x.Priority,
                    UpdatedAt = x.UpdatedAt
                }).ToListAsync();

            // Catalog for cascading picker
            var actions = _catalog.GetControllerActions();   // Area, Controller, Action
            var pages = _catalog.GetRazorPages();          // Area, PageRoute

            var areaSet = new HashSet<string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in actions) areaSet.Add(a.Area);
            foreach (var p in pages) areaSet.Add(p.Area);
            var areas = areaSet.OrderBy(a => a ?? "(root)").ToList();

            ViewBag.CatalogAreas = areas;
            ViewBag.CatalogActions = actions;
            ViewBag.CatalogPages = pages;

            return View(rows);
        }


        public IActionResult Create() => View(new PublicEndpointEditViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PublicEndpointEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var row = new PublicEndpoint
            {
                MatchType = vm.MatchType,
                Area = vm.Area,
                Controller = vm.Controller,
                Action = vm.Action,
                Page = vm.Page,
                Path = vm.Path,
                Regex = vm.Regex,
                Notes = vm.Notes,
                IsEnabled = vm.IsEnabled,
                Priority = vm.Priority,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.PublicEndpoints.Add(row);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();
            TempData["Ok"] = "Public endpoint created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var x = await _db.PublicEndpoints.FindAsync(id);
            if (x is null) return NotFound();

            return View(new PublicEndpointEditViewModel
            {
                Id = x.Id,
                MatchType = x.MatchType,
                Area = x.Area,
                Controller = x.Controller,
                Action = x.Action,
                Page = x.Page,
                Path = x.Path,
                Regex = x.Regex,
                Notes = x.Notes,
                IsEnabled = x.IsEnabled,
                Priority = x.Priority
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PublicEndpointEditViewModel vm)
        {
            var x = await _db.PublicEndpoints.FindAsync(id);
            if (x is null) return NotFound();

            if (!ModelState.IsValid) return View(vm);

            x.MatchType = vm.MatchType; x.Area = vm.Area; x.Controller = vm.Controller; x.Action = vm.Action;
            x.Page = vm.Page; x.Path = vm.Path; x.Regex = vm.Regex;
            x.Notes = vm.Notes; x.IsEnabled = vm.IsEnabled; x.Priority = vm.Priority;
            x.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();
            TempData["Ok"] = "Public endpoint updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var x = await _db.PublicEndpoints.FindAsync(id);
            if (x is null) return NotFound();

            x.IsEnabled = !x.IsEnabled;
            x.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();
            TempData["Ok"] = $"Public endpoint {(x.IsEnabled ? "enabled" : "disabled")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.PublicEndpoints.FindAsync(id);
            if (x is null) return NotFound();

            _db.PublicEndpoints.Remove(x);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();
            TempData["Ok"] = "Public endpoint deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Prefill Create with a ControllerAction
        [HttpGet]
        public IActionResult CreatePresetControllerAction(string? area, string controller, string action)
        {
            var vm = new PublicEndpointEditViewModel
            {
                MatchType = MatchTypes.ControllerAction,
                Area = area,
                Controller = controller,
                Action = action,
                IsEnabled = true,
                Priority = 100
            };
            return View("Create", vm);
        }

        // Prefill Create with a Razor Page
        [HttpGet]
        public IActionResult CreatePresetPage(string? area, string page)
        {
            var vm = new PublicEndpointEditViewModel
            {
                MatchType = MatchTypes.RazorPage,
                Area = area,
                Page = page,
                IsEnabled = true,
                Priority = 100
            };
            return View("Create", vm);
        }

    }
}
