using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using SWIMS.Services.Outbox;

namespace SWIMS.Services.Notifications;

public sealed class NotificationDeliveryJobs
{
    private readonly SwimsIdentityDbContext _db;
    private readonly IWebPushSender _webPush;
    private readonly INotificationPreferences _prefs;
    private readonly INotificationEmailComposer _composer;
    private readonly IEmailOutbox _outbox;
    private readonly ILogger<NotificationDeliveryJobs> _logger;

    private readonly NotificationEmailOptions _emailOptions;
    private readonly HashSet<string> _mandatory;
    private readonly HashSet<string> _allow;

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public NotificationDeliveryJobs(
        SwimsIdentityDbContext db,
        IWebPushSender webPush,
        INotificationPreferences prefs,
        INotificationEmailComposer composer,
        IEmailOutbox outbox,
        IOptions<NotificationEmailOptions> emailOptions,
        ILogger<NotificationDeliveryJobs> logger)
    {
        _db = db;
        _webPush = webPush;
        _prefs = prefs;
        _composer = composer;
        _outbox = outbox;
        _logger = logger;

        _emailOptions = emailOptions.Value ?? new NotificationEmailOptions();
        _mandatory = new HashSet<string>(_emailOptions.MandatoryEventKeys ?? new(), StringComparer.Ordinal);
        _allow = new HashSet<string>(_emailOptions.AllowEventKeys ?? new(), StringComparer.Ordinal);
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 0)] // IMPORTANT: avoid duplicate push/emails on retry
    public async Task DeliverAsync(Guid notificationId)
    {
        Notification? row;
        try
        {
            row = await _db.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed loading notification row. Id={Id}", notificationId);
            return;
        }

        if (row is null)
            return;

        // 1) Web push (best-effort)
        try
        {
            var pushPayload = BuildPushPayload(row.Type, row.PayloadJson);
            await _webPush.SendToUserAsync(row.UserId, pushPayload);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Push send failed (best-effort). UserId={UserId}", row.UserId);
        }

        // 2) Email (best-effort, still goes through Outbox)
        try
        {
            await SendEmailIfEnabledAsync(row.UserId, row.Type, row.Username, row.PayloadJson);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Email enqueue failed (best-effort). UserId={UserId}", row.UserId);
        }
    }

    private async Task SendEmailIfEnabledAsync(int userId, string type, string usernameOrEmail, string payloadJson)
    {
        if (!_emailOptions.ImmediateEnabled)
            return;

        var eventKey = TryReadEventKey(payloadJson);
        if (string.IsNullOrWhiteSpace(eventKey))
            return;

        var isMandatory = _mandatory.Contains(eventKey);
        var isOptionalAllowed = _allow.Contains(eventKey);

        if (!isMandatory && !isOptionalAllowed)
            return;

        if (!isMandatory)
        {
            var eff = await _prefs.GetEffectiveAsync(userId, type);
            if (!eff.email)
                return;
        }

        var email = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(email))
            return;

        var (subject, html, text) = await _composer.ComposeAsync(
            userId: userId,
            type: type,
            usernameOrEmail: usernameOrEmail,
            payloadJson: payloadJson
        );

        await _outbox.EnqueueAsync(to: email, subject: subject, html: html, text: text);
    }

    private static string? TryReadEventKey(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("eventKey", out var ek) &&
                ek.ValueKind == JsonValueKind.String)
                return ek.GetString();
        }
        catch { }
        return null;
    }

    private static object BuildPushPayload(string type, string payloadJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            string? url = null;
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("url", out var urlEl))
            {
                url = urlEl.GetString();
            }

            if (string.Equals(type, "NewMessage", StringComparison.OrdinalIgnoreCase))
            {
                var fromName = root.TryGetProperty("fromName", out var fn) ? (fn.GetString() ?? "Someone") : "Someone";
                var snippet = root.TryGetProperty("snippet", out var sn) ? (sn.GetString() ?? "") : "";

                var tag = $"msg:{Guid.NewGuid()}";
                if (root.TryGetProperty("messageId", out var mid))
                    tag = $"msg:{(mid.ValueKind == JsonValueKind.String ? mid.GetString() : mid.ToString())}";

                return new { title = $"New message from {fromName}", body = snippet, url, tag, renotify = true };
            }

            var message = root.TryGetProperty("message", out var m) ? m.GetString() : null;
            return new { title = "SWIMS", body = string.IsNullOrWhiteSpace(message) ? type : message!, url, tag = $"notif:{type}" };
        }
        catch
        {
            return new { title = "SWIMS", body = type, tag = $"notif:{type}" };
        }
    }
}