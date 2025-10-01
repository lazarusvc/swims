using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SWIMS.Models.Email;

namespace SWIMS.Services.Email;

public static class EmailingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SWIMS emailing stack (SMTP + template rendering).
    /// </summary>
    public static IServiceCollection AddSwimsEmailing(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<SmtpConfiguration>()
            .Bind(config.GetSection("Emailing:Smtp"))
            .Validate(cfg =>
            {
                // Dev pickup OR Host must be provided
                return !(string.IsNullOrWhiteSpace(cfg.DevPickupDirectory) && string.IsNullOrWhiteSpace(cfg.Host));
            }, "Either DevPickupDirectory (for local dev) OR Smtp.Host must be set.");

        services.AddSingleton(provider =>
        {
            var env = provider.GetRequiredService<IHostEnvironment>();
            var cfg = provider.GetRequiredService<IOptions<SmtpConfiguration>>().Value;
            // Use ContentRootPath so it points at the project folder (dev) or deployed content root (prod)
            return new EmailTemplateProvider(env.ContentRootPath, cfg.TemplateDirectory);
        });

        services.AddSingleton<ITemplateRenderer, EmailTemplateRenderer>();
        services.AddTransient<IEmailService, SmtpEmailService>();

        return services;
    }
}
