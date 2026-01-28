using SWIMS.Models.Notifications;

namespace SWIMS.Services.Notifications;

public interface INotificationDispatcher
{
    Task DispatchAsync(SwimsNotificationRequest request, CancellationToken ct = default);
}
