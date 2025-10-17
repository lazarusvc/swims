namespace SWIMS.Services.Notifications;

public interface INotificationPreferences
{
    Task<(bool inApp, bool email, bool digest)> GetEffectiveAsync(int userId, string type, CancellationToken ct = default);
    Task UpsertAsync(int userId, string? type, bool inApp, bool email, bool digest, CancellationToken ct = default);
    Task<IReadOnlyList<(string? type, bool inApp, bool email, bool digest)>> ListAsync(int userId, CancellationToken ct = default);
}
