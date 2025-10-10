using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using SWIMS.Services.Outbox;

namespace SWIMS.Web.Endpoints;

public static class OperationsEndpoints
{
    public static IEndpointRouteBuilder MapSwimsOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var env = app.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
        {
            var group = app.MapGroup("/__dev__/ops").RequireAuthorization();

            group.MapPost("/email-test", async (IEmailOutbox outbox, string to) =>
            {
                var id = await outbox.EnqueueAsync(to, "SWIMS test email", "<p>Hello from SWIMS outbox!</p>", "Hello from SWIMS outbox!");
                return Results.Ok(new { id });
            });
        }

        return app;
    }
}
