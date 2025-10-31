using Hangfire.Dashboard;
using System.Security.Claims;

namespace SWIMS.Web.Ops;

public sealed class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();

        if (!(http.User?.Identity?.IsAuthenticated ?? false))
            return false;

        // Allow if user is in Admin role OR has a permission claim (tweak to match your auth model)
        if (http.User.IsInRole("Admin"))
            return true;

        // Example claim checks; adjust to your real claims
        if (http.User.HasClaim("permission", "ops.hangfire") ||
            http.User.HasClaim(ClaimTypes.Role, "Ops"))
            return true;

        return false;
    }
}
