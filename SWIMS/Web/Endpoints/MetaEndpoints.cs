// File: Web/Endpoints/MetaEndpoints.cs
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

namespace SWIMS.Web.Endpoints;

public static class MetaEndpoints
{
    public sealed record ApiEndpointDto(
        string Pattern,
        string[] Methods,
        string? DisplayName,
        string[] Tags,
        bool RequiresAuth,
        bool AllowAnonymous,
        bool IsApi,
        bool IsV1
    );

    public static IEndpointRouteBuilder MapMetaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("meta").WithTags("Meta");

        group.MapGet("endpoints", (EndpointDataSource dataSource) =>
        {
            var list = dataSource.Endpoints
                .OfType<RouteEndpoint>()
                .Where(e =>
                    e.RoutePattern.RawText is { } p &&
                    !p.StartsWith("/hubs/", StringComparison.OrdinalIgnoreCase) &&
                    !p.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
                .Select(e =>
                {
                    var pattern = e.RoutePattern.RawText ?? "";

                    var methods = e.Metadata.OfType<HttpMethodMetadata>()
                        .SelectMany(m => m.HttpMethods)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .DefaultIfEmpty("GET")
                        .ToArray();

                    var tagsList = e.Metadata.GetMetadata<ITagsMetadata>()?.Tags;
                    var tags = (tagsList is null ? Array.Empty<string>() : tagsList.ToArray());

                    var requiresAuth = e.Metadata.OfType<IAuthorizeData>().Any();
                    var allowAnon = e.Metadata.OfType<AllowAnonymousAttribute>().Any();

                    // Be tolerant of patterns with or without a leading slash
                    var pat = pattern.StartsWith("/") ? pattern : "/" + pattern;
                    var isApi = pat.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
                    var isV1 = pat.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase);

                    return new ApiEndpointDto(
                        Pattern: pat,                  // normalized with leading slash
                        Methods: methods,
                        DisplayName: e.DisplayName,
                        Tags: tags,
                        RequiresAuth: requiresAuth && !allowAnon,
                        AllowAnonymous: allowAnon,
                        IsApi: isApi,
                        IsV1: isV1
                    );
                })
                .OrderBy(e => e.Pattern, StringComparer.OrdinalIgnoreCase)
                .ThenBy(e => string.Join(",", e.Methods))
                .ToList();

            return TypedResults.Ok(list);
        })
        .WithName("GetAllApiEndpoints")
        .WithOpenApi();

        group.MapGet("ping", () => Results.Ok(new { ok = true, ts = DateTimeOffset.UtcNow }))
             .WithOpenApi();

        return routes;
    }
}
