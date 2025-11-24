using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using SWIMS.Services.Outbox;                // IEmailOutbox
using SWIMS.Web.Hubs;

namespace SWIMS.Services.Notifications;

public sealed class Notifier : INotifier
{
    private readonly SwimsIdentityDbContext _db;
    private readonly IHubContext<NotifsHub> _hub;
    private readonly IWebPushSender _webPush;
    private readonly INotificationPreferences _prefs;           // <- your prefs interface
    private readonly INotificationEmailComposer _composer;      // <- your email composer
    private readonly IEmailOutbox _outbox;                      // <- your outbox

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public Notifier(
        SwimsIdentityDbContext db,
        IHubContext<NotifsHub> hub,
        IWebPushSender webPush,
        INotificationPreferences prefs,
        INotificationEmailComposer composer,
        IEmailOutbox outbox)
    {
        _db = db;
        _hub = hub;
        _webPush = webPush;
        _prefs = prefs;
        _composer = composer;
        _outbox = outbox;
    }

    public async Task NotifyUserAsync(int userId, string username, string type, object payload)
    {
        // Persist in-app notification
        var json = payload is string s ? s : JsonSerializer.Serialize(payload, _json);

        var row = new Notification
        {
            UserId = userId,
            Username = username,
            Type = type,
            PayloadJson = json
        };

        _db.Notifications.Add(row);
        await _db.SaveChangesAsync();

        // SignalR - live update to user's group
        await _hub.Clients.Group($"u:{userId}")
            .SendAsync("notif", new { row.Id, row.Type, row.PayloadJson, row.CreatedUtc });

        // Web Push (best-effort, never breaks flow)
        try
        {
            var pushPayload = BuildPushPayload(type, payload);
            await _webPush.SendToUserAsync(userId, pushPayload);
        }
        catch
        {
            // swallow push errors
        }

        // Email via Outbox — honors per-type prefs; uses NotificationEmailComposer
        // await SendEmailIfEnabledAsync(userId, type, username, json);
    }

    private async Task SendEmailIfEnabledAsync(int userId, string type, string usernameOrEmail, string payloadJson)
    {
        try
        {
            // 1) Check effective prefs for this type
            var eff = await _prefs.GetEffectiveAsync(userId, type);
            if (!(eff.email)) return;

            // 2) Resolve recipient email
            var email = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(email))
                return;

            // 3) Compose via your existing templating engine
            var (subject, html, text) = await _composer.ComposeAsync(
                userId: userId,
                type: type,
                usernameOrEmail: usernameOrEmail,
                payloadJson: payloadJson
            );

            // 4) Enqueue (Hangfire processes dispatch)
            await _outbox.EnqueueAsync(
                to: email,
                subject: subject,
                html: html,
                text: text
            );
        }
        catch
        {
            // email is best-effort; never block or throw
        }
    }

    private static object BuildPushPayload(string type, object payloadObj)
    {
        try
        {
            JsonElement root;
            if (payloadObj is string s)
            {
                using var doc = JsonDocument.Parse(s);
                root = doc.RootElement.Clone();
            }
            else
            {
                using var doc = JsonDocument.Parse(JsonSerializer.Serialize(payloadObj, _json));
                root = doc.RootElement.Clone();
            }

            // url (avoid short variable names which can shadow in some scopes)
            string? url = null;
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("url", out var urlEl))
            {
                url = urlEl.GetString();
            }

            if (string.Equals(type, "NewMessage", StringComparison.OrdinalIgnoreCase))
            {
                // fromName
                var fromName = "Someone";
                if (root.TryGetProperty("fromName", out var fromNameEl))
                {
                    fromName = fromNameEl.GetString() ?? "Someone";
                }

                // snippet
                var snippet = "";
                if (root.TryGetProperty("snippet", out var snippetEl))
                {
                    snippet = snippetEl.GetString() ?? "";
                }

                // unique tag per message (or fallback)
                var tag = $"msg:{Guid.NewGuid()}";
                if (root.TryGetProperty("messageId", out var msgIdEl))
                {
                    tag = $"msg:{(msgIdEl.ValueKind == JsonValueKind.String ? msgIdEl.GetString() : msgIdEl.ToString())}";
                }

                return new
                {
                    title = $"New message from {fromName}",
                    body = snippet,
                    url,
                    tag,
                    renotify = true
                };
            }

            // generic
            string? message = null;
            if (root.TryGetProperty("message", out var messageEl))
            {
                message = messageEl.GetString();
            }

            return new
            {
                title = "SWIMS",
                body = string.IsNullOrWhiteSpace(message) ? type : message!,
                url,
                tag = $"notif:{type}"
            };
        }
        catch
        {
            return new { title = "SWIMS", body = type, tag = $"notif:{type}" };
        }
    }
}
