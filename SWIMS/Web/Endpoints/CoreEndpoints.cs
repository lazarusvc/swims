using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SWIMS.Services.Diagnostics.Sessions;
using SWIMS.Services.Notifications;
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
            if (!(http.User?.Identity?.IsAuthenticated ?? false)) return Results.Unauthorized();
            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)) return Results.Unauthorized();

            var sid = http.Request.Cookies["sid"];
            var ip = http.Connection.RemoteIpAddress?.ToString();
            var ua = http.Request.Headers.UserAgent.ToString();
            var username = http.User.Identity?.Name ?? $"user:{uid}";

            if (string.IsNullOrWhiteSpace(sid))
            {
                // First heartbeat after deploy/login → create session + cookie now
                sid = Guid.NewGuid().ToString("N");
                http.Response.Cookies.Append("sid", sid, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax // dev-friendly
                });
                await logger.OnSignedInAsync(uid, username, sid, ip, ua);
                return Results.NoContent(); // 204
            }

            await logger.OnHeartbeatAsync(uid, sid);
            return Results.Ok();
        }).RequireAuthorization();

        var env = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (env.IsDevelopment())
        {
            app.MapPost("/__dev__/notify-me", async (HttpContext http, INotifier notifier) =>
            {
                if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                    return Results.Unauthorized();

                var username = http.User.Identity?.Name ?? $"user:{uid}";
                await notifier.NotifyUserAsync(uid, username, "DevTest", new { message = "Hello from dev endpoint" });
                return Results.Ok(new { ok = true });
            }).RequireAuthorization();
        }

        return app.MapSwimsNotificationsEndpoints()
          .MapSwimsOperationsEndpoints();

    }
}
