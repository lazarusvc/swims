namespace SWIMS.Models.Notifications;

public class SwimsNotificationRequest
{
    /// <summary>
    /// Channel to send on (e.g. "Email", "InApp", "Push", "Test").
    /// </summary>
    public string Channel { get; set; } = default!;

    /// <summary>
    /// Recipient identifier (user ID, username, email, etc.).
    /// </summary>
    public string Recipient { get; set; } = default!;

    /// <summary>
    /// Optional subject/title (mainly for email / push).
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Notification body/content.
    /// </summary>
    public string Body { get; set; } = default!;

    /// <summary>
    /// Optional JSON metadata blob for extra data.
    /// </summary>
    public string? MetadataJson { get; set; }
}
