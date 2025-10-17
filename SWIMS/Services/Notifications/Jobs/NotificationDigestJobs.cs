using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using SWIMS.Data;
using SWIMS.Services.Notifications;
using SWIMS.Services.Outbox;

namespace SWIMS.Services.Notifications.Jobs;

public sealed class NotificationDigestJobs
{
    private readonly SwimsIdentityDbContext _db;
    private readonly IEmailOutbox _outbox;
    private readonly INotificationPreferences _prefs;
    private readonly INotificationEmailComposer _composer;

    public NotificationDigestJobs(
        SwimsIdentityDbContext db,
        IEmailOutbox outbox,
        INotificationPreferences prefs,
        INotificationEmailComposer composer)
    {
        _db = db;
        _outbox = outbox;
        _prefs = prefs;
        _composer = composer;
    }

    /// <summary>
    /// Build and send per-type digests for "yesterday" (UTC) by default.
    /// </summary>
    public async Task<int> RunDailyAsync(DateTime? utcNow = null, CancellationToken ct = default)
    {
        var now = utcNow ?? DateTime.UtcNow;
        var windowStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(-1);
        var windowEnd = windowStart.AddDays(1);

        // Which users had activity in the window?
        var userIds = await _db.Notifications.AsNoTracking()
            .Where(n => n.CreatedUtc >= windowStart && n.CreatedUtc < windowEnd)
            .Select(n => n.UserId)
            .Distinct()
            .ToListAsync(ct);

        var totalSent = 0;

        foreach (var uid in userIds)
        {
            // Resolve recipient email
            var email = await _db.Users.Where(u => u.Id == uid)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(email))
                continue;

            // Gather yesterday's notifications for this user (unseen only is typical for digests)
            var rows = await _db.Notifications.AsNoTracking()
                .Where(n => n.UserId == uid
                            && n.CreatedUtc >= windowStart && n.CreatedUtc < windowEnd
                            && !n.Seen)
                .Select(n => new { n.Type, n.PayloadJson, n.CreatedUtc })
                .ToListAsync(ct);

            if (rows.Count == 0)
                continue;

            // Group by Type and filter to types that have digest enabled for this user
            var groups = rows
                .GroupBy(r => r.Type)
                .ToList();

            var include = new List<(string type, int count)>();
            foreach (var g in groups)
            {
                var (_, _, digest) = await _prefs.GetEffectiveAsync(uid, g.Key, ct);
                if (digest)
                    include.Add((g.Key, g.Count()));
            }

            if (include.Count == 0)
                continue;

            // Build a small summary HTML (rendered through your existing email template via the composer)
            var totalItems = include.Sum(x => x.count);
            var itemsHtml = new StringBuilder();
            itemsHtml.Append("<ul>");
            foreach (var (type, count) in include.OrderByDescending(x => x.count))
            {
                itemsHtml.Append($"<li><strong>{System.Net.WebUtility.HtmlEncode(type)}</strong>: {count} new</li>");
            }
            itemsHtml.Append("</ul>");

            var payload = new
            {
                subject = $"SWIMS: Daily digest for {windowStart:yyyy-MM-dd}",
                // MainParagraph accepts HTML in your templates; we include a compact summary:
                message = $"You received <strong>{totalItems}</strong> notifications yesterday across <strong>{include.Count}</strong> types:{itemsHtml}",
                url = "/portal/notifications/prefs",
                actionLabel = "Update preferences",
                // helpful reference id
                @ref = $"digest-{uid}-{windowStart:yyyyMMdd}"
            };

            var (subject, html, text) = await _composer.ComposeAsync(
                userId: uid,
                type: "DigestDaily",
                usernameOrEmail: email,
                payloadJson: JsonSerializer.Serialize(payload),
                ct: ct);

            await _outbox.EnqueueAsync(email, subject, html: html, text: text);
            totalSent++;
        }

        return totalSent;
    }
}
