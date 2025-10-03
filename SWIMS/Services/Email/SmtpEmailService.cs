using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using SWIMS.Models.Email;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Email;

public interface IEmailService
{
    Task SendAsync(
        EmailAddress to, string subject, string htmlBody,
        EmailAddress? from = null,
        IEnumerable<EmailAddress>? cc = null,
        IEnumerable<EmailAddress>? bcc = null,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken ct = default);

    Task SendTemplateAsync(
        string templateKey, EmailAddress to, object model,
        EmailAddress? from = null,
        IEnumerable<EmailAddress>? cc = null,
        IEnumerable<EmailAddress>? bcc = null,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken ct = default);
}

public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpConfiguration _cfg;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly ITemplateRenderer _renderer;

    public SmtpEmailService(IOptions<SmtpConfiguration> cfg, ILogger<SmtpEmailService> logger, ITemplateRenderer renderer)
    {
        _cfg = cfg.Value;
        _logger = logger;
        _renderer = renderer;
    }

    public async Task SendAsync(
        EmailAddress to, string subject, string htmlBody,
        EmailAddress? from = null,
        IEnumerable<EmailAddress>? cc = null,
        IEnumerable<EmailAddress>? bcc = null,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken ct = default)
    {
        using var message = BuildMailMessage(to, subject, htmlBody, from, cc, bcc, attachments);
        await SendCoreAsync(message, ct);
    }

    public async Task SendTemplateAsync(
        string templateKey, EmailAddress to, object model,
        EmailAddress? from = null,
        IEnumerable<EmailAddress>? cc = null,
        IEnumerable<EmailAddress>? bcc = null,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken ct = default)
    {
        var rendered = await _renderer.RenderAsync(templateKey, model);
        using var message = BuildMailMessage(to, rendered.Subject, rendered.HtmlBody, from, cc, bcc, attachments);
        await SendCoreAsync(message, ct);
    }

    private MailMessage BuildMailMessage(
        EmailAddress to, string subject, string htmlBody,
        EmailAddress? from,
        IEnumerable<EmailAddress>? cc,
        IEnumerable<EmailAddress>? bcc,
        IEnumerable<EmailAttachment>? attachments)
    {
        var msg = new MailMessage
        {
            From = new MailAddress(
                from?.Address ?? _cfg.DefaultFromAddress ?? _cfg.Username ?? "no-reply@localhost",
                from?.DisplayName ?? _cfg.DefaultFromName ?? "SWIMS"),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        msg.To.Add(new MailAddress(to.Address, to.DisplayName));
        if (cc != null) foreach (var a in cc) msg.CC.Add(new MailAddress(a.Address, a.DisplayName));
        if (bcc != null) foreach (var a in bcc) msg.Bcc.Add(new MailAddress(a.Address, a.DisplayName));
        if (attachments != null)
        {
            foreach (var att in attachments)
            {
                var stream = new MemoryStream(att.Content, writable: false);
                msg.Attachments.Add(new Attachment(stream, att.FileName, att.ContentType));
            }
        }
        return msg;
    }

    private async Task SendCoreAsync(MailMessage message, CancellationToken ct)
    {
        // Dev pickup mode: resolve to ABSOLUTE path before using SmtpClient
        if (!string.IsNullOrWhiteSpace(_cfg.DevPickupDirectory))
        {
            var pickupConfigured = _cfg.DevPickupDirectory!;
            var pickupAbsolute = System.IO.Path.IsPathRooted(pickupConfigured)
                ? pickupConfigured
                : System.IO.Path.GetFullPath(System.IO.Path.Combine(System.AppContext.BaseDirectory, pickupConfigured));

            System.IO.Directory.CreateDirectory(pickupAbsolute);

            using var client = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = pickupAbsolute
            };

            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Email written to pickup dir: {Dir} | To: {To} | Subj: {Subject}",
                pickupAbsolute, string.Join(",", message.To.Select(a => a.Address)), message.Subject);
            return;
        }

        // Network send via SMTP
        using var smtp = new SmtpClient(_cfg.Host!, _cfg.Port)
        {
            // Required for BOTH STARTTLS (587) and implicit SSL (465)
            EnableSsl = _cfg.UseSsl || _cfg.UseStartTls,
            // Always use the explicit credentials we provide (never machine/Windows creds)
            UseDefaultCredentials = false,
            Credentials = string.IsNullOrWhiteSpace(_cfg.Username)
                ? System.Net.CredentialCache.DefaultNetworkCredentials // still false path, but keep for completeness
                : new System.Net.NetworkCredential(_cfg.Username, _cfg.Password)
        };

        // STARTTLS TargetName is useful for Office 365 tenants; skip for consumer Outlook host
        if (_cfg.UseStartTls && !_cfg.Host!.Equals("smtp-mail.outlook.com", StringComparison.OrdinalIgnoreCase))
        {
            smtp.TargetName = "STARTTLS/" + _cfg.Host;
        }

        await smtp.SendMailAsync(message, ct);
        _logger.LogInformation("Email sent via {Host}:{Port} to {To} | Subject: {Subject}",
            _cfg.Host, _cfg.Port, string.Join(",", message.To.Select(a => a.Address)), message.Subject);

    }
}
