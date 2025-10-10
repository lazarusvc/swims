using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SWIMS.Services.Diagnostics.Sessions;
using System.Security.Claims;

namespace SWIMS.Web.Endpoints;

public static class CoreEndpoints
{
    /// <summary>
    /// Registers core app endpoints added in this branch.
    /// Keep this as the single entry for minimal APIs to keep Program.cs clean.
    /// </summary>
    public static IEndpointRouteBuilder MapSwimsCoreEndpoints(this IEndpointRouteBuilder app)
    {
        // Health + readiness (anonymous)
        app.MapHealthChecks("/healthz").AllowAnonymous();

        app.MapGet("/readyz", () => Results.Ok(new { status = "ready" }))
           .AllowAnonymous();

        // Authenticated heartbeat for session last-seen
        app.MapPost("/me/heartbeat", async (HttpContext http, ISessionLogger logger) =>
        {
            if (!(http.User?.Identity?.IsAuthenticated ?? false))
                return Results.Unauthorized();

            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Results.Unauthorized();

            var sid = http.Request.Cookies["sid"];
            if (string.IsNullOrWhiteSpace(sid))
                return Results.BadRequest(new { error = "Missing sid cookie" });

            await logger.OnHeartbeatAsync(uid, sid!);
            return Results.Ok(new { ok = true });
        }).RequireAuthorization();

        return app;
    }
}
