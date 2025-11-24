using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SWIMS.Security;
using SWIMS.Services.SystemSettings;
using SWIMS.Areas.Admin.ViewModels.SystemSettings;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = Permissions.Admin_Settings)]
    public class SystemSettingsController : Controller
    {
        private readonly ISystemSettingsService _settings;
        private readonly IWebHostEnvironment _env;

        public SystemSettingsController(
            ISystemSettingsService settings,
            IWebHostEnvironment env)
        {
            _settings = settings;
            _env = env;
        }

        // GET: /Admin/SystemSettings
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var overview = await _settings.GetOverviewAsync(ct);
            var vm = new SystemSettingsIndexViewModel
            {
                ActiveEnvironment = overview.ActiveEnvironment,
                Sections = overview.Sections
            };
            return View(vm);
        }

        // GET: /Admin/SystemSettings/Edit?key=Emailing&env=Development
        public async Task<IActionResult> Edit(string key, string? env, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest("Missing section key.");

            var section = await _settings.GetSectionAsync(key, env, ct);
            var vm = SystemSettingsEditViewModel.FromSection(section);
            ViewBag.IsDevelopment = _env.IsDevelopment();
            return View(vm);
        }

        // POST: /Admin/SystemSettings/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SystemSettingsEditViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsDevelopment = _env.IsDevelopment();
                return View(vm);
            }

            if (!_env.IsDevelopment())
            {
                ModelState.AddModelError(string.Empty,
                    "Editing configuration is currently only enabled in the Development environment.");
                ViewBag.IsDevelopment = false;
                return View(vm);
            }

            try
            {
                var section = vm.ToSection();
                await _settings.SaveSectionAsync(section, vm.EnvironmentName, ct);

                TempData["StatusMessage"] = $"Saved settings for {vm.DisplayName}.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // friendly error (e.g. invalid JSON or comments in file)
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.IsDevelopment = _env.IsDevelopment();
                return View(vm);
            }
        }
    }
}
