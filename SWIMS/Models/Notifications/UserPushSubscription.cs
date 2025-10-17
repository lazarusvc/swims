namespace SWIMS.Models.Notifications;

public class UserPushSubscription
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string Endpoint { get; set; } = default!;
    public string P256dh { get; set; } = default!;
    public string Auth { get; set; } = default!;
    public string? UserAgent { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? LastSeenUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
