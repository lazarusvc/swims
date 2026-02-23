namespace SWIMS.Services.Notifications;

public sealed class NotificationEmailOptions
{
    public bool ImmediateEnabled { get; set; } = false;
    public List<string> ImmediateAllowEventKeys { get; set; } = new();
}