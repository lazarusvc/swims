namespace SWIMS.Services.Notifications;

public interface INotifier
{
    Task NotifyUserAsync(int userId, string username, string type, object payload);
}
