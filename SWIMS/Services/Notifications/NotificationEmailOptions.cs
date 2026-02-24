namespace SWIMS.Services.Notifications;

public sealed class NotificationEmailOptions
{
    public bool ImmediateEnabled { get; set; } = false;

    // Always email (user cannot switch off)
    public List<string> MandatoryEventKeys { get; set; } = new();

    // Optional immediate email (user Email pref governs)
    public List<string> AllowEventKeys { get; set; } = new();
}