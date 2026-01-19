using Microsoft.AspNetCore.Authentication;
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

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        // For API/static/non-HTML calls, don't 302 to the login page.
        // Return 401 so fetch() / assets don’t trigger mixed-content redirects.
        if (IsNonHtmlOrApiRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        // Build a RELATIVE redirect so the browser keeps the current scheme (https)
        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;

        var loginPath = context.Options.LoginPath.HasValue
            ? context.Options.LoginPath.Value
            : "/Identity/Account/Login";

        var redirectUrl =
            context.Request.PathBase
            + loginPath
            + QueryString.Create(context.Options.ReturnUrlParameter, returnUrl);

        context.Response.Redirect(redirectUrl);
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (IsNonHtmlOrApiRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;

        var deniedPath = context.Options.AccessDeniedPath.HasValue
            ? context.Options.AccessDeniedPath.Value
            : "/Identity/Account/AccessDenied";

        var redirectUrl =
            context.Request.PathBase
            + deniedPath
            + QueryString.Create(context.Options.ReturnUrlParameter, returnUrl);

        context.Response.Redirect(redirectUrl);
        return Task.CompletedTask;
    }

    private static bool IsNonHtmlOrApiRequest(HttpRequest request)
    {
        // API
        if (request.Path.StartsWithSegments("/api"))
            return true;

        // If the browser tells us it's not a document navigation, treat as non-HTML
        var fetchDest = request.Headers["Sec-Fetch-Dest"].ToString();
        if (!string.IsNullOrWhiteSpace(fetchDest) &&
            !fetchDest.Equals("document", StringComparison.OrdinalIgnoreCase) &&
            !fetchDest.Equals("iframe", StringComparison.OrdinalIgnoreCase))
            return true;

        // If caller doesn't accept HTML, don't redirect to HTML login
        var accept = request.Headers["Accept"].ToString();
        if (!string.IsNullOrWhiteSpace(accept) &&
            !accept.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

}

