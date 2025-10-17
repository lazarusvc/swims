namespace SWIMS.Services.Notifications;

public interface INotificationEmailComposer
{
    Task<(string subject, string html, string text)> ComposeAsync(
        int userId,
        string type,
        string usernameOrEmail,
        string payloadJson,
        CancellationToken ct = default);
}
