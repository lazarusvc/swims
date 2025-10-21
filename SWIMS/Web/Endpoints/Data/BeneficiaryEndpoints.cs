using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using SWIMS.Models;

namespace SWIMS.Web.Endpoints.Data;

public static class BeneficiaryEndpoints
{
    public static IEndpointRouteBuilder MapBeneficiaryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("beneficiary").WithTags(nameof(SW_beneficiary));

        group.MapGet("/", async (SwimsDb_moreContext db) =>
            await db.SW_beneficiaries.ToListAsync())
        .WithName("GetAllSW_beneficiaries")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<SW_beneficiary>, NotFound>> (int id, SwimsDb_moreContext db) =>
            await db.SW_beneficiaries.AsNoTracking().FirstOrDefaultAsync(model => model.Id == id)
                is SW_beneficiary model ? TypedResults.Ok(model) : TypedResults.NotFound())
        .WithName("GetSW_beneficiaryById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, SW_beneficiary sW_beneficiary, SwimsDb_moreContext db) =>
        {
            var affected = await db.SW_beneficiaries
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, sW_beneficiary.Id)
                    .SetProperty(m => m.uuid, sW_beneficiary.uuid)
                    .SetProperty(m => m.first_name, sW_beneficiary.first_name)
                    .SetProperty(m => m.middle_name, sW_beneficiary.middle_name)
                    .SetProperty(m => m.last_name, sW_beneficiary.last_name)
                    .SetProperty(m => m.dob, sW_beneficiary.dob)
                    .SetProperty(m => m.gender, sW_beneficiary.gender)
                    .SetProperty(m => m.martial_status, sW_beneficiary.martial_status)
                    .SetProperty(m => m.id_number, sW_beneficiary.id_number)
                    .SetProperty(m => m.telephone_number, sW_beneficiary.telephone_number)
                    .SetProperty(m => m.status, sW_beneficiary.status)
                    .SetProperty(m => m.approved_amount, sW_beneficiary.approved_amount)
                    .SetProperty(m => m.name, sW_beneficiary.name)
                );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateSW_beneficiary")
        .WithOpenApi();

        group.MapPost("/", async (SW_beneficiary sW_beneficiary, SwimsDb_moreContext db) =>
        {
            db.SW_beneficiaries.Add(sW_beneficiary);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/v1/beneficiary/{sW_beneficiary.Id}", sW_beneficiary);
        })
        .WithName("CreateSW_beneficiary")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, SwimsDb_moreContext db) =>
        {
            var affected = await db.SW_beneficiaries
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteSW_beneficiary")
        .WithOpenApi();

        return routes;
    }
}
