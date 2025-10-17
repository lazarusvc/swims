namespace SWIMS.Models.Logging;

public sealed class AuditLog
{
    public long Id { get; set; }
    public DateTime Utc { get; set; } = DateTime.UtcNow;

    // Your Identity uses int keys
    public int? UserId { get; set; }
    public string? Username { get; set; }

    // Insert | Update | Delete
    public string Action { get; set; } = default!;

    public string Entity { get; set; } = default!;
    public string? EntityId { get; set; }

    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }

    public string? Ip { get; set; }
    public string? ExtraJson { get; set; }
}

// Marker to opt entities IN to auditing
public interface IAudited { }
