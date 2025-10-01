using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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
            var env = provider.GetRequiredService<IHostEnvironment>(); // gives ContentRootPath
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<EmailTemplateProvider>>();
            var cfg = provider.GetRequiredService<IOptions<SmtpConfiguration>>().Value;

            // Use ContentRootPath so it points at the project folder in dev, deployed content root in prod
            return new EmailTemplateProvider(
                logger,
                basePath: env.ContentRootPath,
                physicalDirectory: cfg.TemplateDirectory);
        });

        services.AddSingleton<ITemplateRenderer, EmailTemplateRenderer>();
        services.AddTransient<IEmailService, SmtpEmailService>();

        return services;
    }
}
