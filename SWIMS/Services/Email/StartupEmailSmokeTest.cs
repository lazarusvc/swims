using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SWIMS.Models.Email;

namespace SWIMS.Services.Email;

/// <summary>
/// Sends a one-time email on application startup when enabled in configuration.
/// Useful for verifying SMTP/pickup + template wiring in dev.
/// </summary>
public sealed class StartupEmailSmokeTest : IHostedService
{
    private readonly ILogger<StartupEmailSmokeTest> _logger;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly IEmailService _emails;

    public StartupEmailSmokeTest(
        ILogger<StartupEmailSmokeTest> logger,
        IConfiguration config,
        IHostEnvironment env,
        IEmailService emails)
    {
        _logger = logger;
        _config = config;
        _env = env;
        _emails = emails;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var section = _config.GetSection("Emailing:StartupTest");
        var enabled = section.GetValue<bool>("Enabled");
        if (!enabled)
        {
            _logger.LogDebug("StartupEmailSmokeTest disabled (Emailing:StartupTest:Enabled=false).");
            return;
        }

        try
        {
            var delaySeconds = section.GetValue<int?>("DelaySeconds") ?? 0;
            if (delaySeconds > 0)
            {
                _logger.LogInformation("StartupEmailSmokeTest delaying {Delay}s before send...", delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }

            // Recipients: semicolon/comma/space-separated
            var toString = section.GetValue<string>("To") ?? "";
            var recipients = toString
                .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new EmailAddress(x.Trim()))
                .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogWarning("StartupEmailSmokeTest enabled but no recipients configured (Emailing:StartupTest:To).");
                return;
            }

            // Prefer template when TemplateKey is provided; fall back to raw subject/body
            var templateKey = section.GetValue<string>("TemplateKey");
            var subject = section.GetValue<string>("Subject") ?? $"SWIMS Startup Email - {_env.EnvironmentName}";
            var htmlBody = section.GetValue<string>("HtmlBody")
                           ?? $"<p>SWIMS started at <strong>{DateTimeOffset.Now:u}</strong> on <em>{Environment.MachineName}</em> ({_env.EnvironmentName}).</p>";

            // Model passed to templates
            var model = new
            {
                Now = DateTimeOffset.Now.ToString("u"),
                Environment = _env.EnvironmentName,
                Hostname = Environment.MachineName,
                // Subject // NEW: available to templates if you want to render it there
            };

            int sent = 0;
            foreach (var to in recipients)
            {
                if (!string.IsNullOrWhiteSpace(templateKey))
                {
                    _logger.LogInformation("StartupEmailSmokeTest using template '{TemplateKey}' to {Recipient}.", templateKey, to.Address);
                    await _emails.SendTemplateAsync(templateKey, to, model, ct: cancellationToken);
                }
                else
                {
                    _logger.LogInformation("StartupEmailSmokeTest using inline body to {Recipient}.", to.Address);
                    await _emails.SendAsync(to, subject, htmlBody, ct: cancellationToken);
                }
                sent++;
            }

            _logger.LogInformation("StartupEmailSmokeTest sent {Count} message(s).", sent);
        }
        catch (OperationCanceledException)
        {
            // ignore on shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartupEmailSmokeTest failed to send startup email.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
