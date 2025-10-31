using Microsoft.AspNetCore.Routing;
using SWIMS.Web.Endpoints;          // Core/Messaging/Notifications/Operations/Push extensions
using SWIMS.Web.Endpoints.Data;     // City/Beneficiary/FinancialInstitution/Organization

namespace SWIMS.Web.Endpoints;

public static class ApiEndpoints
{
    /// <summary>
    /// Single entry point to register ALL API endpoints under /api/v1.
    /// </summary>
    public static IEndpointRouteBuilder MapSwimsApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var v1 = api.MapGroup("/v1");

        // Meta (feeds the dashboard)
        v1.MapMetaEndpoints();

        // Core app APIs
        v1.MapSwimsCoreEndpoints();
        v1.MapSwimsMessagingEndpoints();
        v1.MapSwimsNotificationsEndpoints();
        v1.MapSwimsOperationsEndpoints();
        v1.MapSwimsPushEndpoints();

        // Data endpoints
        v1.MapCityEndpoints();
        v1.MapBeneficiaryEndpoints();
        v1.MapFinancialInstitutionEndpoints();
        v1.MapOrganizationEndpoints();

        return app;
    }
}
