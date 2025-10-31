namespace SWIMS.Services.Email;

public sealed class EmailTemplate
{
    public required string Key { get; init; }
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
}
