using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SWIMS.Services.Setup;
using System;
using System.Threading.Tasks;

namespace SWIMS.Web.Setup
{
    public sealed class SetupGuardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public SetupGuardMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context, ISetupStateService setupState)
        {
            // Toggle via config: "App": { "Setup": { "GuardEnabled": true } }
            var guardEnabled = _config.GetValue<bool?>("App:Setup:GuardEnabled") ?? true;
            if (!guardEnabled)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path;

            // Always let setup, static content and docs/hangfire through as-is
            if (path.HasValue)
            {
                var value = path.Value!;

                if (value.StartsWith("/setup", StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("/docs", StringComparison.OrdinalIgnoreCase) ||
                    value.StartsWith("/ops/hangfire", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }
            }

            // Only intercept the root landing page for now (/)
            if (!path.HasValue || path == "/")
            {
                var isConfigured = await setupState.IsAppConfiguredAsync();
                if (!isConfigured)
                {
                    context.Response.Redirect("/Setup");
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class SetupGuardMiddlewareExtensions
    {
        public static IApplicationBuilder UseSetupGuard(this IApplicationBuilder app)
            => app.UseMiddleware<SetupGuardMiddleware>();
    }
}
