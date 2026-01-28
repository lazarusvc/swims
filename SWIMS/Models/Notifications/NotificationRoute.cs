using System.ComponentModel.DataAnnotations;

namespace SWIMS.Models.Notifications;

public class NotificationRoute
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string EventKey { get; set; } = "";

    // Module-bucket type: "Forms", "Cases", etc.
    [Required, MaxLength(50)]
    public string Type { get; set; } = NotificationTypes.System;

    public bool IsEnabled { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<NotificationRouteRole> Roles { get; set; } = new();
    public List<NotificationRouteUser> Users { get; set; } = new();
}

public class NotificationRouteRole
{
    public int Id { get; set; }

    public int RouteId { get; set; }
    public NotificationRoute Route { get; set; } = default!;

    public int RoleId { get; set; }
    [Required, MaxLength(256)]
    public string RoleName { get; set; } = "";
}

public class NotificationRouteUser
{
    public int Id { get; set; }

    public int RouteId { get; set; }
    public NotificationRoute Route { get; set; } = default!;

    public int UserId { get; set; }

    [MaxLength(256)]
    public string? UserNameSnapshot { get; set; }

    [MaxLength(256)]
    public string? EmailSnapshot { get; set; }
}

public static class NotificationTypes
{
    public const string System = "System";
    public const string Forms = "Forms";
    public const string Cases = "Cases";
}
