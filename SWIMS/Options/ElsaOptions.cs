namespace SWIMS.Options;

public sealed class ElsaOptions
{
    public string ServerUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";

    public IntegrationOptions Integration { get; set; } = new();

    public sealed class IntegrationOptions
    {
        public string NotificationsKey { get; set; } = "";
    }
}
