using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using SWIMS.Models;

namespace SWIMS.Web.Endpoints.Data;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("organization").WithTags(nameof(SW_organization));

        group.MapGet("/", async (SwimsDb_moreContext db) =>
            await db.SW_organizations.ToListAsync())
        .WithName("GetAllSW_organizations")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<SW_organization>, NotFound>> (int id, SwimsDb_moreContext db) =>
            await db.SW_organizations.AsNoTracking().FirstOrDefaultAsync(model => model.Id == id)
                is SW_organization model ? TypedResults.Ok(model) : TypedResults.NotFound())
        .WithName("GetSW_organizationById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, SW_organization sW_organization, SwimsDb_moreContext db) =>
        {
            var affected = await db.SW_organizations
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, sW_organization.Id)
                    .SetProperty(m => m.vendor_id, sW_organization.vendor_id)
                    .SetProperty(m => m.name, sW_organization.name)
                    .SetProperty(m => m.type, sW_organization.type)
                    .SetProperty(m => m.active, sW_organization.active)
                );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateSW_organization")
        .WithOpenApi();

        group.MapPost("/", async (SW_organization sW_organization, SwimsDb_moreContext db) =>
        {
            db.SW_organizations.Add(sW_organization);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/v1/organization/{sW_organization.Id}", sW_organization);
        })
        .WithName("CreateSW_organization")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, SwimsDb_moreContext db) =>
        {
            var affected = await db.SW_organizations
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteSW_organization")
        .WithOpenApi();

        return routes;
    }
}
