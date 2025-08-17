using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWIMS.Models.Security;
using SWIMS.Services.Auth;
using SWIMS.Services.Diagnostics;

namespace SWIMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RouteInspectorController : Controller
    {
        private readonly IEndpointCatalog _catalog;
        private readonly IPublicAccessStore _public;
        private readonly IEndpointPolicyAssignmentStore _assignments;

        public RouteInspectorController(IEndpointCatalog catalog, IPublicAccessStore @public, IEndpointPolicyAssignmentStore assignments)
        { _catalog = catalog; _public = @public; _assignments = assignments; }

        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inspect(string matchType, string? area, string? controller, string? action, string? page, string? path)
        {
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
    }
}
