namespace SWIMS.Models.Logging;

public sealed class SessionLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Your Identity uses int keys
    public int UserId { get; set; }
    public string Username { get; set; } = default!;

    public string SessionId { get; set; } = default!; // we’ll set & keep in cookie "sid"

    public DateTime LoginUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LogoutUtc { get; set; }

    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
}
