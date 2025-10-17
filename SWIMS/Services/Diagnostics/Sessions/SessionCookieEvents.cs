using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using SWIMS.Services.Diagnostics.Auditing;
using SWIMS.Services.Diagnostics.Sessions;
using System.Security.Claims;

namespace SWIMS.Services.Diagnostics.Sessions;

public sealed class SessionCookieEvents : CookieAuthenticationEvents
{
    private readonly ISessionLogger _logger;
    private readonly IAuditLogger _audit; // 👈 add
    private const string SidCookieName = "sid";

    public SessionCookieEvents(ISessionLogger logger, IAuditLogger audit) // 👈 inject
    {
        _logger = logger;
        _audit = audit;
    }

    public override async Task SignedIn(CookieSignedInContext context)
    {
        // ... your existing session log code (creates sid, calls _logger.OnSignedInAsync, etc.)

        var uidStr = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(uidStr, out var uid))
        {
            var uname = context.Principal?.Identity?.Name ?? "unknown";
            var sid = context.HttpContext.Request.Cookies[SidCookieName];
            var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();

            await _audit.LogAsync(
                action: "Login",
                entity: "Auth",
                entityId: sid,
                userId: uid,
                username: uname,
                extra: new { userAgent = ua }
            );
        }

        await base.SignedIn(context);
    }

    public override async Task SigningOut(CookieSigningOutContext context)
    {
        // ... your existing session log code (marks logout + clears sid cookie)

        var uidStr = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(uidStr, out var uid))
        {
            var uname = context.HttpContext.User?.Identity?.Name ?? "unknown";
            var sid = context.HttpContext.Request.Cookies[SidCookieName];
            var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();

            await _audit.LogAsync(
                action: "Logout",
                entity: "Auth",
                entityId: sid,
                userId: uid,
                username: uname,
                extra: new { userAgent = ua }
            );
        }

        await base.SigningOut(context);
    }
}

