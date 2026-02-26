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
using SWIMS.Models.Notifications;
using SWIMS.Services.Notifications;
using SWIMS.Services.Diagnostics.Auditing;


namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PublicEndpointsController : Controller
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IPublicAccessStore _store;
        private readonly IEndpointCatalog _catalog;
        private readonly IElsaWorkflowClient _elsa;
        private readonly IAuditLogger _audit;

        public PublicEndpointsController(
            SwimsIdentityDbContext db,
            IPublicAccessStore store,
            IEndpointCatalog catalog,
            IElsaWorkflowClient elsa,
            IAuditLogger audit)
        {
            _db = db;
            _store = store;
            _catalog = catalog;
            _elsa = elsa;
            _audit = audit;
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

            // 📝 Audit: Public endpoint created
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "PublicEndpointCreated",
                entity: "PublicEndpoint",
                entityId: row.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: null,
                newObj: new
                {
                    endpointId = row.Id,
                    matchType = row.MatchType,
                    area = row.Area,
                    controller = row.Controller,
                    actionName = row.Action,
                    page = row.Page,
                    path = row.Path,
                    regex = row.Regex,
                    notes = row.Notes,
                    isEnabled = row.IsEnabled,
                    priority = row.Priority
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Public endpoint created
            var actorName = User?.Identity?.Name ?? "An admin";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.PublicEndpoints.Created,
                subject: "Public endpoint created",
                body: $"You created public endpoint #{row.Id}.",
                url: Url.Action(nameof(Edit), new { id = row.Id }),
                extraMeta_: new
                {
                    endpointId = row.Id,
                    matchType = row.MatchType,
                    area = row.Area,
                    controller = row.Controller,
                    actionName = row.Action,
                    page = row.Page,
                    path = row.Path,
                    regex = row.Regex,
                    notes = row.Notes,
                    isEnabled = row.IsEnabled,
                    priority = row.Priority
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Public endpoint created",
                        body = $"You created public endpoint #{row.Id}."
                    },
                    routed = new
                    {
                        subject = "Public endpoint created",
                        body = $"{actorName} created public endpoint #{row.Id}."
                    },
                    superadmin = new
                    {
                        subject = "Public endpoint created",
                        body = $"{actorName} created public endpoint #{row.Id}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


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

            var oldObj = new
            {
                endpointId = x.Id,
                matchType = x.MatchType,
                area = x.Area,
                controller = x.Controller,
                actionName = x.Action,
                page = x.Page,
                path = x.Path,
                regex = x.Regex,
                notes = x.Notes,
                isEnabled = x.IsEnabled,
                priority = x.Priority
            };

            x.MatchType = vm.MatchType; x.Area = vm.Area; x.Controller = vm.Controller; x.Action = vm.Action;
            x.Page = vm.Page; x.Path = vm.Path; x.Regex = vm.Regex;
            x.Notes = vm.Notes; x.IsEnabled = vm.IsEnabled; x.Priority = vm.Priority;
            x.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 📝 Audit: Public endpoint updated
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            var newObj = new
            {
                endpointId = x.Id,
                matchType = x.MatchType,
                area = x.Area,
                controller = x.Controller,
                actionName = x.Action,
                page = x.Page,
                path = x.Path,
                regex = x.Regex,
                notes = x.Notes,
                isEnabled = x.IsEnabled,
                priority = x.Priority
            };

            await _audit.TryLogAsync(
                action: "PublicEndpointUpdated",
                entity: "PublicEndpoint",
                entityId: x.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: oldObj,
                newObj: newObj,
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Public endpoint updated
            var actorName = User?.Identity?.Name ?? "An admin";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.PublicEndpoints.Updated,
                subject: "Public endpoint updated",
                body: $"You updated public endpoint #{x.Id}.",
                url: Url.Action(nameof(Edit), new { id = x.Id }),
                extraMeta_: new
                {
                    endpointId = x.Id,
                    matchType = x.MatchType,
                    area = x.Area,
                    controller = x.Controller,
                    actionName = x.Action,
                    page = x.Page,
                    path = x.Path,
                    regex = x.Regex,
                    notes = x.Notes,
                    isEnabled = x.IsEnabled,
                    priority = x.Priority
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Public endpoint updated",
                        body = $"You updated public endpoint #{x.Id}."
                    },
                    routed = new
                    {
                        subject = "Public endpoint updated",
                        body = $"{actorName} updated public endpoint #{x.Id}."
                    },
                    superadmin = new
                    {
                        subject = "Public endpoint updated",
                        body = $"{actorName} updated public endpoint #{x.Id}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = "Public endpoint updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var x = await _db.PublicEndpoints.FindAsync(id);
            if (x is null) return NotFound();

            var oldIsEnabled = x.IsEnabled;

            x.IsEnabled = !x.IsEnabled;
            x.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 📝 Audit: Public endpoint toggled
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "PublicEndpointToggled",
                entity: "PublicEndpoint",
                entityId: x.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: new { isEnabled = oldIsEnabled },
                newObj: new { isEnabled = x.IsEnabled },
                extra: new
                {
                    endpointId = x.Id
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Public endpoint toggled
            var actorName = User?.Identity?.Name ?? "An admin";
            var stateWord = x.IsEnabled ? "enabled" : "disabled";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.PublicEndpoints.Toggled,
                subject: "Public endpoint toggled",
                body: $"You {stateWord} public endpoint #{x.Id}.",
                url: Url.Action(nameof(Edit), new { id = x.Id }),
                extraMeta_: new
                {
                    endpointId = x.Id,
                    isEnabled = x.IsEnabled
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Public endpoint toggled",
                        body = $"You {stateWord} public endpoint #{x.Id}."
                    },
                    routed = new
                    {
                        subject = "Public endpoint toggled",
                        body = $"{actorName} {stateWord} public endpoint #{x.Id}."
                    },
                    superadmin = new
                    {
                        subject = "Public endpoint toggled",
                        body = $"{actorName} {stateWord} public endpoint #{x.Id}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


            TempData["Ok"] = $"Public endpoint {(x.IsEnabled ? "enabled" : "disabled")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _db.PublicEndpoints.FindAsync(id);
            if (x is null) return NotFound();

            var endpointId = x.Id;

            var oldObj = new
            {
                endpointId = x.Id,
                matchType = x.MatchType,
                area = x.Area,
                controller = x.Controller,
                actionName = x.Action,
                page = x.Page,
                path = x.Path,
                regex = x.Regex,
                notes = x.Notes,
                isEnabled = x.IsEnabled,
                priority = x.Priority
            };

            _db.PublicEndpoints.Remove(x);
            await _db.SaveChangesAsync();
            await _store.InvalidateAsync();

            // 📝 Audit: Public endpoint deleted
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            await _audit.TryLogAsync(
                action: "PublicEndpointDeleted",
                entity: "PublicEndpoint",
                entityId: endpointId.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: oldObj,
                newObj: null,
                extra: new { endpointId },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Public endpoint deleted
            var actorName = User?.Identity?.Name ?? "An admin";

            await NotifyAdminAsync(
                eventKey: SwimsEventKeys.Security.PublicEndpoints.Deleted,
                subject: "Public endpoint deleted",
                body: $"You deleted public endpoint #{endpointId}.",
                url: Url.Action(nameof(Index)),
                extraMeta_: new
                {
                    endpointId = endpointId
                },
                texts_: new
                {
                    actor = new
                    {
                        subject = "Public endpoint deleted",
                        body = $"You deleted public endpoint #{endpointId}."
                    },
                    routed = new
                    {
                        subject = "Public endpoint deleted",
                        body = $"{actorName} deleted public endpoint #{endpointId}."
                    },
                    superadmin = new
                    {
                        subject = "Public endpoint deleted",
                        body = $"{actorName} deleted public endpoint #{endpointId}."
                    }
                },
                ct: HttpContext.RequestAborted);
            // 🔔 Notify: END


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

        private async Task NotifyAdminAsync(
    string eventKey,
    string subject,
    string body,
    object? extraMeta_ = null,
    object? texts_ = null,
    string? url = null,
    int? targetUserId = null,
    IEnumerable<int>? targetUserIds = null,
    CancellationToken ct = default)
        {
            try
            {
                var recipient = User?.FindFirstValue(ClaimTypes.NameIdentifier)
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
                        type = NotificationTypes.System,
                        eventKey,
                        url,
                        metadata = new
                        {
                            actorUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier),
                            actorUserName = User?.Identity?.Name,

                            targetUserId,
                            targetUserIds = targetUserIds?.ToArray(),

                            texts = texts_,
                            extra = extraMeta_
                        }
                    })
                };

                await _elsa.ExecuteByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
            }
        }



    }
}
