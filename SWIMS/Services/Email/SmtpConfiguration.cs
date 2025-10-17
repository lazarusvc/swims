using System.ComponentModel.DataAnnotations;

namespace SWIMS.Services.Email;

public sealed class SmtpConfiguration
{
    [Required] public string? Host { get; init; }
    public int Port { get; init; } = 587;
    public bool UseStartTls { get; init; } = true;
    public bool UseSsl { get; init; } = false;

    [EmailAddress] public string? DefaultFromAddress { get; init; }
    public string? DefaultFromName { get; init; }

    public string? Username { get; init; }
    public string? Password { get; init; }

    /// <summary>
    /// For local dev: write .eml files into this folder instead of sending over the network.
    /// If set, it overrides Host and skips SMTP.
    /// </summary>
    public string? DevPickupDirectory { get; init; }

    /// <summary>
    /// Optional physical directory for HTML templates (default: Templates/Emails).
    /// </summary>
    public string? TemplateDirectory { get; init; }
}
