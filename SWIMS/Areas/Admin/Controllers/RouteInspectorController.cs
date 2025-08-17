using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWIMS.Models.Security;
using SWIMS.Services.Auth;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    // Attribute route to avoid any conventional/area routing ambiguity
    [Route("Admin/RouteInspector")]
    public class RouteInspectorController : Controller
    {
        private readonly IPublicAccessStore _public;
        private readonly IEndpointPolicyAssignmentStore _assignments;

        public RouteInspectorController(
            IPublicAccessStore @public,
            IEndpointPolicyAssignmentStore assignments)
        {
            _public = @public;
            _assignments = assignments;
        }

        // GET /Admin/RouteInspector
        [HttpGet("")]
        public IActionResult Index() => View();

        // POST /Admin/RouteInspector/Inspect
        [HttpPost("Inspect")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inspect(
            [FromForm] string matchType,
            [FromForm] string? area,
            [FromForm] string? controller,
            [FromForm] string? action,
            [FromForm] string? page,
            [FromForm] string? path)
        {
            Normalize(matchType, ref area, ref controller, ref action, ref page, ref path);

            var isPublic = await _public.IsPublicAsync(area, controller, action, page, path);
            var policies = await _assignments.GetPolicyNamesForAsync(area, controller, action, page, path);

            return Json(new
            {
                matchType,
                area,
                controller,
                action,
                page,
                path,
                isPublic,
                policies
            });
        }

        private static void Normalize(
            string matchType,
            ref string? area,
            ref string? controller,
            ref string? action,
            ref string? page,
            ref string? path)
        {
            // align with store preview semantics
            area = string.IsNullOrWhiteSpace(area) ? null : area;

            switch (matchType)
            {
                case MatchTypes.ControllerAction:
                    page = null; path = null; break;
                case MatchTypes.Controller:
                    action = null; page = null; path = null; break;
                case MatchTypes.RazorPage:
                    controller = null; action = null; path = null; break;
                case MatchTypes.Path:
                case MatchTypes.Regex:
                    controller = null; action = null; page = null; break;
            }
        }
    }
}
