using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using SWIMS.Services.Diagnostics.Sessions;

namespace SWIMS.Services.Diagnostics.Sessions;

public sealed class SessionCookieEvents : CookieAuthenticationEvents
{
    private readonly ISessionLogger _logger;
    private const string SidCookieName = "sid";

    public SessionCookieEvents(ISessionLogger logger) => _logger = logger;

    public override async Task SignedIn(CookieSignedInContext context)
    {
        var http = context.HttpContext;
        var user = http.User;

        if (user?.Identity?.IsAuthenticated == true &&
            int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
        {
            var username = user.Identity!.Name ?? $"user:{uid}";
            // Generate a new session id at sign-in
            var sid = Guid.NewGuid().ToString("N");

            http.Response.Cookies.Append(SidCookieName, sid, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict
            });

            var ip = http.Connection.RemoteIpAddress?.ToString();
            var ua = http.Request.Headers.UserAgent.ToString();

            await _logger.OnSignedInAsync(uid, username, sid, ip, ua);
        }

        await base.SignedIn(context);
    }

    public override async Task SigningOut(CookieSigningOutContext context)
    {
        var http = context.HttpContext;
        var user = http.User;

        if (user?.Identity?.IsAuthenticated == true &&
            int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
        {
            var sid = http.Request.Cookies[SidCookieName];
            if (!string.IsNullOrWhiteSpace(sid))
            {
                await _logger.OnSignedOutAsync(uid, sid!);

                // expire cookie
                http.Response.Cookies.Delete(SidCookieName, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict
                });
            }
        }

        await base.SigningOut(context);
    }
}
