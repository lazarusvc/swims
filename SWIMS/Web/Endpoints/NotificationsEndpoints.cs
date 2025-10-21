using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Services.Diagnostics.Auditing;
using SWIMS.Services.Notifications;
using System.Security.Claims;

namespace SWIMS.Web.Endpoints;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapSwimsNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("me/notifications").RequireAuthorization();

        // GET /me/notifications?unseenOnly=true&skip=0&take=20
        group.MapGet("", async (HttpContext http, SwimsIdentityDbContext db, bool? unseenOnly, int? skip, int? take) =>
        {
            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Results.Unauthorized();

            var q = db.Notifications.AsNoTracking().Where(n => n.UserId == uid);
            if (unseenOnly == true) q = q.Where(n => !n.Seen);

            var sk = Math.Max(0, skip ?? 0);
            var tk = Math.Clamp(take ?? 20, 1, 100);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(n => n.CreatedUtc).Skip(sk).Take(tk).ToListAsync();

            return Results.Ok(new { total, skip = sk, take = tk, items });
        });

        // POST /me/notifications/{id}/seen
        group.MapPost("{id:guid}/seen", async (HttpContext http, SwimsIdentityDbContext db, Guid id) =>
        {
            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Results.Unauthorized();

            var row = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == uid);
            if (row is null) return Results.NotFound();

            row.Seen = true;
            await db.SaveChangesAsync();
            return Results.Ok(new { ok = true });
        });

        // POST /me/notifications/seen-all
        group.MapPost("seen-all", async (HttpContext http, SwimsIdentityDbContext db) =>
        {
            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Results.Unauthorized();

            await db.Notifications.Where(n => n.UserId == uid && !n.Seen)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Seen, true));

            return Results.Ok(new { ok = true });
        });

        group.MapGet("count", async (HttpContext http, SwimsIdentityDbContext db) =>
        {
            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Results.Unauthorized();

            var count = await db.Notifications.AsNoTracking()
                .Where(n => n.UserId == uid && !n.Seen)
                .CountAsync();

            return Results.Ok(new { count });
        });

        group.MapGet("prefs", async (HttpContext http, INotificationPreferences svc) =>
        {
            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Results.Unauthorized();

            var rows = await svc.ListAsync(uid);
            return Results.Ok(rows.Select(x => new { type = x.type, inApp = x.inApp, email = x.email, digest = x.digest }));
        });

        group.MapPut("prefs", async (
            HttpContext http,
            INotificationPreferences svc,
            IAuditLogger audit,
            PrefUpsert dto) =>
        {
            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Results.Unauthorized();

            var username = http.User.Identity?.Name ?? "unknown";

            // snapshot old
            var oldRows = await svc.ListAsync(uid);
            var oldOne = oldRows.FirstOrDefault(x => x.type == dto.Type);

            // apply change
            await svc.UpsertAsync(uid, dto.Type, dto.InApp, dto.Email, dto.Digest);

            // snapshot new
            var newRows = await svc.ListAsync(uid);
            var newOne = newRows.FirstOrDefault(x => x.type == dto.Type);

            await audit.LogAsync(
                action: "PrefsUpsert",
                entity: "NotificationPreference",
                entityId: dto.Type ?? "(global)",
                userId: uid,
                username: username,
                oldObj: oldOne,
                newObj: newOne
            );

            return Results.Ok(new { ok = true });
        });


        group.MapGet("types", async (HttpContext http, SwimsIdentityDbContext db) =>
        {
            if (!int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Results.Unauthorized();

            var types = await db.Notifications.AsNoTracking()
                .Where(n => n.UserId == uid)
                .Select(n => n.Type)
                .Distinct()
                .OrderBy(t => t)
                .Take(50)
                .ToListAsync();

            return Results.Ok(types);
        });


        return app;
    }

    internal record PrefUpsert(string? Type, bool InApp, bool Email, bool Digest);
}
