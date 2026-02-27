using System;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SWIMS.Models.Notifications;

namespace SWIMS.Services.Notifications;

public sealed class NotificationDispatchJobs
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<NotificationDispatchJobs> _logger;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public NotificationDispatchJobs(
        INotificationDispatcher dispatcher,
        ILogger<NotificationDispatchJobs> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    [Queue("default")]
    [AutomaticRetry(Attempts = 0)] // IMPORTANT: avoid duplicate notifs on retry
    public async Task DispatchAsync(string requestJson)
    {
        try
        {
            var req = JsonSerializer.Deserialize<SwimsNotificationRequest>(requestJson, _json);
            if (req is null) return;

            await _dispatcher.DispatchAsync(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification dispatch job failed.");
        }
    }
}