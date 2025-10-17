namespace SWIMS.Services.Email;

public sealed class GraphMailConfiguration
{
    public string TenantId { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public string? ClientSecret { get; init; }    // Use Key Vault or user-secrets in dev
    public string SenderUser { get; init; } = default!; // mailbox UPN or ID
    public bool SaveToSentItems { get; init; } = true;

    public string? DefaultFromAddress { get; init; }
    public string? DefaultFromName { get; init; }
    public string? TemplateDirectory { get; init; }
}
