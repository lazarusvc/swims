namespace SWIMS.Models.Notifications;

public sealed class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // int-key Identity
    public int UserId { get; set; }
    public string Username { get; set; } = default!;

    public string Type { get; set; } = default!;      // e.g., "ApplicationAssigned"
    public string PayloadJson { get; set; } = "{}";   // lightweight data blob

    public bool Seen { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
