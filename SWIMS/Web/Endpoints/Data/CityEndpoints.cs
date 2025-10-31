using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using SWIMS.Models;

namespace SWIMS.Web.Endpoints.Data;

public static class CityEndpoints
{
    public static IEndpointRouteBuilder MapCityEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("city").WithTags(nameof(SW_city));

        group.MapGet("/", async (SwimsDb_moreContext db) =>
            await db.SW_cities.ToListAsync())
        .WithName("GetAllSW_cities")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<SW_city>, NotFound>> (int id, SwimsDb_moreContext db) =>
            await db.SW_cities.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id)
                is SW_city model ? TypedResults.Ok(model) : TypedResults.NotFound())
        .WithName("GetSW_cityById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, SW_city sW_city, SwimsDb_moreContext db) =>
        {
            var affected = await db.SW_cities
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, sW_city.Id)
                    .SetProperty(m => m.name, sW_city.name)
                );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateSW_city")
        .WithOpenApi();

        group.MapPost("/", async (SW_city sW_city, SwimsDb_moreContext db) =>
        {
            db.SW_cities.Add(sW_city);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/v1/city/{sW_city.Id}", sW_city);
        })
        .WithName("CreateSW_city")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, SwimsDb_moreContext db) =>
        {
            var affected = await db.SW_cities
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteSW_city")
        .WithOpenApi();

        return routes;
    }
}
