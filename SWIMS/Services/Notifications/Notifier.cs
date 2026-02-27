using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using SWIMS.Web.Hubs;

namespace SWIMS.Services.Notifications;

public sealed class Notifier : INotifier
{
    private readonly SwimsIdentityDbContext _db;
    private readonly IHubContext<NotifsHub> _hub;
    private readonly IBackgroundJobClient _jobs;

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public Notifier(
        SwimsIdentityDbContext db,
        IHubContext<NotifsHub> hub,
        IBackgroundJobClient jobs)
    {
        _db = db;
        _hub = hub;
        _jobs = jobs;
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

        // SignalR - live update to user's group (keep inline)
        await _hub.Clients.Group($"u:{userId}")
            .SendAsync("notif", new { row.Id, row.Type, row.PayloadJson, row.CreatedUtc });

        // Push + Email moved off-thread
        try
        {
            _jobs.Enqueue<NotificationDeliveryJobs>(j => j.DeliverAsync(row.Id));
        }
        catch
        {
            // best-effort; never break the primary flow
        }
    }
}