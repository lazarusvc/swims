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

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SWIMS.Models;

namespace SWIMS.Controllers
{
    /// <summary>
    /// Provides actions for the home page, privacy policy, and error display.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Creates a new <see cref="HomeController"/> with the specified logger.
        /// </summary>
        /// <param name="logger">
        /// The <see cref="ILogger{HomeController}"/> used for logging.
        /// </param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Displays the application home page.
        /// </summary>
        /// <returns>A <see cref="ViewResult"/> for the Index view.</returns>
        public IActionResult Index()
        {
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
