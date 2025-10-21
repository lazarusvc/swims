using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SWIMS.Data;
using SWIMS.Services.Notifications;
using SWIMS.Services.Outbox;
using System.Text;
using System.Text.Json;

namespace SWIMS.Web.Endpoints;

public static class OperationsEndpoints
{
    public static IEndpointRouteBuilder MapSwimsOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var env = app.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
        {
            var group = app.MapGroup("__dev__/ops").RequireAuthorization();

            group.MapPost("email-test", async (IEmailOutbox outbox, string to) =>
            {
                var id = await outbox.EnqueueAsync(to, "SWIMS test email", "<p>Hello from SWIMS outbox!</p>", "Hello from SWIMS outbox!");
                return Results.Ok(new { id });
            });

            group.MapGet("notif-template/preview", async (INotificationEmailComposer composer) =>
            {
                var payload = new
                {
                    subject = "DevTest Notification",
                    message = "Hello from SWIMS (preview)",
                    url = "https://apps.gov.dm/swims-test",
                    actionLabel = "Open SWIMS"
                };

                // Avoid tuple deconstruction to dodge inference issues
                var result = await composer.ComposeAsync(
                    userId: 1,
                    type: "DevTest",
                    usernameOrEmail: "you@domain.com",
                    payloadJson: JsonSerializer.Serialize(payload)
                );

                var html = result.html; // tuple field name from (subject, html, text)
                return Results.Content(html, "text/html");
            });

        }

        // secured ops/logs group (Admin role; swap for your claims policy if you prefer)
        var logs = app.MapGroup("ops/logs")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // --- AUDIT LOG -----------------------------------------------------------
        // GET /ops/logs/audit?skip=&take=&userId=&username=&action=&entity=&entityId=&from=&to=&contains=
        logs.MapGet("audit", async (
            SwimsIdentityDbContext db,
            int skip = 0,
            int take = 25,
            int? userId = null,
            string? username = null,
            string? action = null,
            string? entity = null,
            string? entityId = null,
            DateTime? from = null,
            DateTime? to = null,
            string? contains = null) =>
        {
            take = Math.Clamp(take, 1, 200);

            var q = db.AuditLogs.AsNoTracking();

            // Exact-match filters (all optional)
            if (userId.HasValue) q = q.Where(x => x.UserId == userId.Value);
            if (!string.IsNullOrWhiteSpace(username)) q = q.Where(x => x.Username == username);
            if (!string.IsNullOrWhiteSpace(action)) q = q.Where(x => x.Action == action);
            if (!string.IsNullOrWhiteSpace(entity)) q = q.Where(x => x.Entity == entity);
            if (!string.IsNullOrWhiteSpace(entityId)) q = q.Where(x => x.EntityId == entityId);

            // Time window (by Utc)
            if (from.HasValue) q = q.Where(x => x.Utc >= from.Value);
            if (to.HasValue) q = q.Where(x => x.Utc <= to.Value);

            // Text search across common text/JSON columns
            if (!string.IsNullOrWhiteSpace(contains))
            {
                q = q.Where(x =>
                    (x.Username != null && x.Username.Contains(contains)) ||
                    (x.Action != null && x.Action.Contains(contains)) ||
                    (x.Entity != null && x.Entity.Contains(contains)) ||
                    (x.EntityId != null && x.EntityId.Contains(contains)) ||
                    (x.OldValuesJson != null && x.OldValuesJson.Contains(contains)) ||
                    (x.NewValuesJson != null && x.NewValuesJson.Contains(contains)) ||
                    (x.ExtraJson != null && x.ExtraJson.Contains(contains)));
            }

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.Utc)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    x.Id,
                    x.Utc,
                    x.UserId,
                    x.Username,
                    x.Action,
                    x.Entity,
                    x.EntityId,
                    x.Ip,
                    x.OldValuesJson,
                    x.NewValuesJson,
                    x.ExtraJson
                })
                .ToListAsync();

            return Results.Ok(new { total, skip, take, items });
        });

        // --- SESSION LOG ---------------------------------------------------------
        // GET /ops/logs/sessions?skip=&take=&userId=&username=&sessionId=&from=&to=&activeOnly=&contains=
        logs.MapGet("sessions", async (
            SwimsIdentityDbContext db,
            int skip = 0,
            int take = 25,
            int? userId = null,
            string? username = null,
            string? sessionId = null,
            DateTime? from = null,
            DateTime? to = null,
            bool? activeOnly = null,
            string? contains = null) =>
        {
            take = Math.Clamp(take, 1, 200);

            var q = db.SessionLogs.AsNoTracking();

            // Exact-match filters
            if (userId.HasValue) q = q.Where(x => x.UserId == userId.Value);
            if (!string.IsNullOrWhiteSpace(username)) q = q.Where(x => x.Username == username);
            if (!string.IsNullOrWhiteSpace(sessionId)) q = q.Where(x => x.SessionId == sessionId);

            // Time window (by activity)
            if (from.HasValue) q = q.Where(x => x.LastSeenUtc >= from.Value);
            if (to.HasValue) q = q.Where(x => x.LastSeenUtc <= to.Value);

            // Active only (no logout recorded)
            if (activeOnly == true) q = q.Where(x => x.LogoutUtc == null);

            // Contains across IP / UserAgent (quick text search)
            if (!string.IsNullOrWhiteSpace(contains))
            {
                q = q.Where(x =>
                    (x.Ip != null && x.Ip.Contains(contains)) ||
                    (x.UserAgent != null && x.UserAgent.Contains(contains)));
            }

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.LastSeenUtc)
                .ThenByDescending(x => x.LoginUtc)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.Username,
                    SessionId = x.SessionId,
                    x.LoginUtc,
                    x.LastSeenUtc,
                    x.LogoutUtc,
                    x.Ip,
                    x.UserAgent
                })
                .ToListAsync();

            return Results.Ok(new { total, skip, take, items });
        });

        // --- AUDIT CSV --------------------------------------------------------------
        logs.MapGet("audit.csv", async (
            SwimsIdentityDbContext db,
            int skip = 0,
            int take = 1000,          // CSV: default bigger page
            int? userId = null,
            string? username = null,
            string? action = null,
            string? entity = null,
            string? entityId = null,
            DateTime? from = null,
            DateTime? to = null,
            string? contains = null) =>
        {
            take = Math.Clamp(take, 1, 5000);
            var q = db.AuditLogs.AsNoTracking();

            if (userId.HasValue) q = q.Where(x => x.UserId == userId.Value);
            if (!string.IsNullOrWhiteSpace(username)) q = q.Where(x => x.Username == username);
            if (!string.IsNullOrWhiteSpace(action)) q = q.Where(x => x.Action == action);
            if (!string.IsNullOrWhiteSpace(entity)) q = q.Where(x => x.Entity == entity);
            if (!string.IsNullOrWhiteSpace(entityId)) q = q.Where(x => x.EntityId == entityId);
            if (from.HasValue) q = q.Where(x => x.Utc >= from.Value);
            if (to.HasValue) q = q.Where(x => x.Utc <= to.Value);
            if (!string.IsNullOrWhiteSpace(contains))
            {
                q = q.Where(x =>
                    (x.Username != null && x.Username.Contains(contains)) ||
                    (x.Action != null && x.Action.Contains(contains)) ||
                    (x.Entity != null && x.Entity.Contains(contains)) ||
                    (x.EntityId != null && x.EntityId.Contains(contains)) ||
                    (x.OldValuesJson != null && x.OldValuesJson.Contains(contains)) ||
                    (x.NewValuesJson != null && x.NewValuesJson.Contains(contains)) ||
                    (x.ExtraJson != null && x.ExtraJson.Contains(contains)));
            }

            var rows = await q.OrderByDescending(x => x.Utc)
                              .Skip(skip).Take(take)
                              .Select(x => new {
                                  x.Id,
                                  x.Utc,
                                  x.UserId,
                                  x.Username,
                                  x.Action,
                                  x.Entity,
                                  x.EntityId,
                                  x.Ip,
                                  x.OldValuesJson,
                                  x.NewValuesJson,
                                  x.ExtraJson
                              })
                              .ToListAsync();

            string Esc(string? s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                var needs = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                return needs ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,Utc,UserId,Username,Action,Entity,EntityId,Ip,OldValuesJson,NewValuesJson,ExtraJson");
            foreach (var x in rows)
            {
                sb.AppendLine(string.Join(",",
                    x.Id, x.Utc.ToString("o"), x.UserId, Esc(x.Username), Esc(x.Action),
                    Esc(x.Entity), Esc(x.EntityId), Esc(x.Ip),
                    Esc(x.OldValuesJson), Esc(x.NewValuesJson), Esc(x.ExtraJson)
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv", $"audit_{DateTime.UtcNow:yyyyMMdd_HHmm}_utc.csv");
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        // --- SESSIONS CSV -----------------------------------------------------------
        logs.MapGet("sessions.csv", async (
            SwimsIdentityDbContext db,
            int skip = 0,
            int take = 5000,
            int? userId = null,
            string? username = null,
            string? sessionId = null,
            DateTime? from = null,
            DateTime? to = null,
            bool? activeOnly = null,
            string? contains = null) =>
        {
            take = Math.Clamp(take, 1, 10000);
            var q = db.SessionLogs.AsNoTracking();

            if (userId.HasValue) q = q.Where(x => x.UserId == userId.Value);
            if (!string.IsNullOrWhiteSpace(username)) q = q.Where(x => x.Username == username);
            if (!string.IsNullOrWhiteSpace(sessionId)) q = q.Where(x => x.SessionId == sessionId);
            if (from.HasValue) q = q.Where(x => x.LastSeenUtc >= from.Value);
            if (to.HasValue) q = q.Where(x => x.LastSeenUtc <= to.Value);
            if (activeOnly == true) q = q.Where(x => x.LogoutUtc == null);
            if (!string.IsNullOrWhiteSpace(contains))
                q = q.Where(x => (x.Ip != null && x.Ip.Contains(contains)) || (x.UserAgent != null && x.UserAgent.Contains(contains)));

            var rows = await q.OrderByDescending(x => x.LastSeenUtc)
                              .ThenByDescending(x => x.LoginUtc)
                              .Skip(skip).Take(take)
                              .Select(x => new {
                                  x.Id,
                                  x.UserId,
                                  x.Username,
                                  x.SessionId,
                                  x.LoginUtc,
                                  x.LastSeenUtc,
                                  x.LogoutUtc,
                                  x.Ip,
                                  x.UserAgent
                              })
                              .ToListAsync();

            string Esc(string? s)
            {
                if (string.IsNullOrEmpty(s)) return "";
                var needs = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                return needs ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,UserId,Username,SessionId,LoginUtc,LastSeenUtc,LogoutUtc,Ip,UserAgent");
            foreach (var x in rows)
            {
                sb.AppendLine(string.Join(",",
                    x.Id, x.UserId, Esc(x.Username), Esc(x.SessionId),
                    x.LoginUtc.ToString("o"), x.LastSeenUtc.ToString("o"), x.LogoutUtc?.ToString("o"),
                    Esc(x.Ip), Esc(x.UserAgent)
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv", $"sessions_{DateTime.UtcNow:yyyyMMdd_HHmm}_utc.csv");
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));




        return app;
    }
}
