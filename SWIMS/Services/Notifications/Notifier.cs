using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using SWIMS.Services.Diagnostics.Auditing;
using SWIMS.Services.Outbox;
using SWIMS.Web.Hubs;
using System.Text.Json;

namespace SWIMS.Services.Notifications;

public sealed class Notifier : INotifier
{
    private readonly SwimsIdentityDbContext _db;
    private readonly IHubContext<NotifsHub> _hub;
    private readonly INotificationPreferences _prefs;
    private readonly IEmailOutbox _outbox;
    private readonly INotificationEmailComposer _composer;
    private readonly IAuditLogger _audit;

    public Notifier(
        SwimsIdentityDbContext db,
        IHubContext<NotifsHub> hub,
        INotificationPreferences prefs,
        IEmailOutbox outbox,
        INotificationEmailComposer composer,
        IAuditLogger audit)
    {
        _db = db; _hub = hub; _prefs = prefs; _outbox = outbox;
        _composer = composer; _audit = audit;
    }

    public async Task NotifyUserAsync(int userId, string username, string type, object payload)
    {
        var (inApp, email, _) = await _prefs.GetEffectiveAsync(userId, type);
        var json = JsonSerializer.Serialize(payload);

        // 1) In-app notification (if enabled)
        Guid? notifId = null;
        if (inApp)
        {
            var row = new Notification
            {
                UserId = userId,
                Username = username,   // display name/alias is fine here
                Type = type,
                PayloadJson = json
            };
            _db.Notifications.Add(row);
            await _db.SaveChangesAsync();
            notifId = row.Id;

            await _hub.Clients.Group($"u:{userId}")
                .SendAsync("notif", new { row.Id, row.Type, row.PayloadJson, row.CreatedUtc });
        }

        // 2) Email hand-off (if enabled) — resolve actual email from Users table
        if (email)
        {
            var userEmail = await _db.Users.Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync();
            string? to = !string.IsNullOrWhiteSpace(userEmail) ? userEmail :
                         (LooksLikeEmail(username) ? username : null);

            if (!string.IsNullOrWhiteSpace(to))
            {
                var (subject, html, text) = await _composer.ComposeAsync(userId, type, username, json);
                await _outbox.EnqueueAsync(to!, subject, html: html, text: text);
            }
        }

        await _audit.LogAsync(
            action: "Notify",
            entity: "Notification",
            entityId: type,
            userId: userId,
            username: username,
            oldObj: null,
            newObj: new { type, channels = new { inApp, email } }
        );
    }

    private static bool LooksLikeEmail(string? s)
        => !string.IsNullOrWhiteSpace(s) && s.Contains('@') && s.Contains('.');
}
