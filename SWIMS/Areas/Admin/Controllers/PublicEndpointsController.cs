using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Areas.Admin.ViewModels.AccessControl;
using SWIMS.Data;
using SWIMS.Models.Security;
using SWIMS.Services.Auth;
using SWIMS.Services.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PublicEndpointsController : Controller
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IPublicAccessStore _store;
        private readonly IEndpointCatalog _catalog;
        private readonly IElsaWorkflowClient _elsa;

        public PublicEndpointsController(
            SwimsIdentityDbContext db,
            IPublicAccessStore store,
            IEndpointCatalog catalog,
            IElsaWorkflowClient elsa)
        {
            _db = db;
            _store = store;
            _catalog = catalog;
            _elsa = elsa;
        }

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

            // 🔔 Notify: Public endpoint created
            await NotifyAdminAsync(
                subject: "Public endpoint created",
                body: "A public endpoint was created.",
                metadata: new
                {
                    action = "PublicEndpointCreated",
                    endpointId = row.Id,
                    matchType = row.MatchType,
                    area = row.Area,
                    controller = row.Controller,
                    actionName = row.Action,
                    page = row.Page,
                    path = row.Path
                });

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

            // 🔔 Notify: Public endpoint updated
            await NotifyAdminAsync(
                subject: "Public endpoint updated",
                body: $"Public endpoint ID {x.Id} was updated.",
                metadata: new
                {
                    action = "PublicEndpointUpdated",
                    endpointId = x.Id,
                    matchType = x.MatchType,
                    area = x.Area,
                    controller = x.Controller,
                    actionName = x.Action,
                    page = x.Page,
                    path = x.Path
                });

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

            // 🔔 Notify: Public endpoint toggled
            await NotifyAdminAsync(
                subject: "Public endpoint toggled",
                body: $"Public endpoint ID {x.Id} was {(x.IsEnabled ? "enabled" : "disabled")}.",
                metadata: new
                {
                    action = "PublicEndpointToggled",
                    endpointId = x.Id,
                    isEnabled = x.IsEnabled
                });

            TempData["Ok"] = $"Public endpoint {(x.IsEnabled ? "enabled" : "disabled")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.PublicEndpoints.FindAsync(id);
            if (x is null) return NotFound();

            var endpointId = x.Id;

            _db.PublicEndpoints.Remove(x);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 🔔 Notify: Public endpoint deleted
            await NotifyAdminAsync(
                subject: "Public endpoint deleted",
                body: $"Public endpoint ID {endpointId} was deleted.",
                metadata: new
                {
                    action = "PublicEndpointDeleted",
                    endpointId = endpointId
                });

            TempData["Ok"] = "Public endpoint deleted.";
            return RedirectToAction(nameof(Index));

        }



        // Prefill Create with a Razor Page
        [HttpGet]
        public IActionResult CreatePresetControllerAction(
            [FromQuery(Name = "area")] string? area,
            [FromQuery(Name = "controller")] string controllerName,
            [FromQuery(Name = "action")] string actionName)
        {
            var vm = new PublicEndpointEditViewModel
            {
                MatchType = MatchTypes.ControllerAction,
                Area = area,
                Controller = controllerName,
                Action = actionName,
                IsEnabled = true,
                Priority = 100
            };
            return View("Create", vm);
        }

        [HttpGet]
        public IActionResult CreatePresetPage(
            [FromQuery(Name = "area")] string? area,
            [FromQuery(Name = "page")] string page)
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

        private async Task NotifyAdminAsync(string subject, string body, object? metadata = null)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? recipient = !string.IsNullOrWhiteSpace(userIdClaim)
                ? userIdClaim
                : User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(recipient))
                return;

            var payload = new
            {
                Recipient = recipient,
                Channel = "InApp",
                Subject = subject,
                Body = body,
                MetadataJson = metadata == null ? null : JsonSerializer.Serialize(metadata)
            };

            try
            {
                // 🔔 Notify: Admin authorization config event
                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload);
            }
            catch
            {
            }
        }


    }
}
