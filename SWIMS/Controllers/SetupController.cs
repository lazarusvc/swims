using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWIMS.Services.Setup;

namespace SWIMS.Controllers
{
    [AllowAnonymous]
    public sealed class SetupController : Controller
    {
        private readonly ISetupStateService _setupState;

        public SetupController(ISetupStateService setupState)
        {
            _setupState = setupState;
        }

        // Main entry: GET /Setup
        [HttpGet]
        [Route("Setup")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var summary = await _setupState.GetSummaryAsync(cancellationToken);
            return View(summary);
        }

        // Optional JSON endpoint if you ever want to poll from JS or health checks: GET /setup/status
        [HttpGet]
        [Route("setup/status")]
        public async Task<IActionResult> Status(CancellationToken cancellationToken)
        {
            var summary = await _setupState.GetSummaryAsync(cancellationToken);
            return Ok(summary);
        }
    }
}
