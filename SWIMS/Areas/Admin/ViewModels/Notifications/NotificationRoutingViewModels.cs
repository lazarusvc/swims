namespace SWIMS.Areas.Admin.ViewModels.Notifications;

public class UserOptionViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
}

public class RoleOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class PermissionOptionViewModel
{
    /// <summary>
    /// Permission key value (e.g. "Approvals.Level2")
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// Constant name for readability (e.g. "Approvals_L2")
    /// </summary>
    public string Name { get; set; } = "";
}

public class NotificationRouteEditViewModel
{
    public int? Id { get; set; }

    public string EventKey { get; set; } = "";

    // module bucket: "Forms", "Cases", "System", "Approvals", etc.
    public string Type { get; set; } = "System";

    // Populated from SWIMS.Models.Notifications.NotificationTypes (reflection).
    public List<string> AllTypes { get; set; } = new();

    public bool IsEnabled { get; set; } = true;

    public string? Description { get; set; }

    public List<RoleOptionViewModel> AllRoles { get; set; } = new();
    public List<int> SelectedRoleIds { get; set; } = new();

    public List<PermissionOptionViewModel> AllPermissions { get; set; } = new();
    public List<string> SelectedPermissionKeys { get; set; } = new();

    public List<UserOptionViewModel> SelectedUsers { get; set; } = new();
    public List<int> SelectedUserIds { get; set; } = new();
}