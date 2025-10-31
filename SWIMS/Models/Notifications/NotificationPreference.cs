namespace SWIMS.Models.Notifications;

public sealed class NotificationPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int UserId { get; set; }

    /// <summary>
    /// Optional type key (e.g., "ApplicationAssigned"). NULL means "global default" for the user.
    /// </summary>
    public string? Type { get; set; }

    public bool InAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = false;
    public bool DigestEnabled { get; set; } = false;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
