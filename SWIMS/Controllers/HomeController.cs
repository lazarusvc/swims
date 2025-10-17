// -------------------------------------------------------------------
// File:    HomeController.cs
// Author:  N/A
// Created: N/A
// Purpose: Controller for public-facing pages including home, privacy, and error.
// Dependencies:
//   - Microsoft.AspNetCore.Mvc.Controller
//   - SWIMS.Models.ErrorViewModel
//   - System.Diagnostics.Activity
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using System.Diagnostics;

namespace SWIMS.Controllers
{
    /// <summary>
    /// Provides actions for the home page, privacy policy, and error display.
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="HomeController"/> with the specified logger.
    /// </remarks>
    /// <param name="context"></param>
    /// <param name="logger">
    /// The <see cref="ILogger{HomeController}"/> used for logging.
    /// </param>
    public class HomeController(SwimsDb_moreContext context, ILogger<HomeController> logger) : Controller
    {
        private readonly SwimsDb_moreContext _context = context;
        private readonly ILogger<HomeController> _logger = logger;

        /// <summary>
        /// Displays the application home page.
        /// </summary>
        /// <returns>A <see cref="ViewResult"/> for the Index view.</returns>
        public IActionResult Index()
        {
            ViewBag.frmBtn = _context.SW_forms.AsNoTracking().ToList();
            return View();
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        /// <returns>A <see cref="ViewResult"/> for the Privacy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page with diagnostic information.
        /// </summary>
        /// <returns>
        /// A <see cref="ViewResult"/> for the Error view, populated with an
        /// <see cref="ErrorViewModel"/> containing the current request ID.
        /// </returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
