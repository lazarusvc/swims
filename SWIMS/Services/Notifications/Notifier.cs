using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using SWIMS.Web.Hubs;

namespace SWIMS.Services.Notifications;

public sealed class Notifier : INotifier
{
    private readonly SwimsIdentityDbContext _db;
    private readonly IHubContext<NotifsHub> _hub;

    public Notifier(SwimsIdentityDbContext db, IHubContext<NotifsHub> hub)
    {
        _db = db; _hub = hub;
    }

    public async Task NotifyUserAsync(int userId, string username, string type, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var row = new Notification
        {
            UserId = userId,
            Username = username,
            Type = type,
            PayloadJson = json
        };

        _db.Notifications.Add(row);
        await _db.SaveChangesAsync();

        // Push real-time to the user's group
        await _hub.Clients.Group($"u:{userId}")
            .SendAsync("notif", new { row.Id, row.Type, row.PayloadJson, row.CreatedUtc });
    }
}
