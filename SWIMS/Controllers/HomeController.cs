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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using System.Diagnostics;

namespace SWIMS.Controllers
{
    /// <summary>
    /// Provides actions for the home page, privacy policy, and error display.
    /// </summary>
    public class HomeController : Controller
    {

        //private readonly ILogger<HomeController> _logger;



        private readonly SwimsDb_moreContext _context;
        public HomeController(SwimsDb_moreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            SwimsDb_moreContext context = _context;
            ViewBag.frmBtn = _context.SW_forms
            .Select(c => new SelectListItem() { Text = c.uuid, Value = c.name })
            .ToList();
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
