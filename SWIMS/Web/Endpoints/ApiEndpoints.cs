// File: Web/Endpoints/ApiEndpoints.cs
using Microsoft.AspNetCore.Routing;
using SWIMS.Models;

namespace SWIMS.Web.Endpoints;

public static class ApiEndpoints
{
    /// <summary>
    /// Single entry point to register ALL API endpoints under /api/v1.
    /// Keep this file as the only place Program.cs calls for API wiring.
    /// </summary>
    public static IEndpointRouteBuilder MapSwimsApi(this IEndpointRouteBuilder app)
    {
        // /api/v1 root
        var api = app.MapGroup("/api");
        var v1 = api.MapGroup("/v1");

        // ====== OWNED minimal APIs (ensure these map RELATIVE paths inside each module) ======
        // Example calls — update the modules in Commit 2 to map relative paths (no leading '/')
        v1.MapSwimsCoreEndpoints();
        v1.MapSwimsMessagingEndpoints();
        v1.MapSwimsNotificationsEndpoints();
        v1.MapSwimsOperationsEndpoints();
        v1.MapSwimsPushEndpoints();

        // ====== EF-generated or data endpoints ======
        // In Commit 2 we’ll make their route templates relative, so composing here works as /api/v1/...
        v1.MapSW_beneficiaryEndpoints();
        v1.MapSW_cityEndpoints();
        v1.MapSW_financial_institutionEndpoints();
        v1.MapSW_organizationEndpoints();

        return app;
    }
}
