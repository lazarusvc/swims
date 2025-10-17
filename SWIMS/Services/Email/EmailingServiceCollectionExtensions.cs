using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SWIMS.Services.Email;

public static class EmailingServiceCollectionExtensions
{
    public static IServiceCollection AddSwimsEmailing(this IServiceCollection services, IConfiguration config)
    {
        var emailing = config.GetSection("Emailing");
        var mode = emailing.GetValue<string>("Mode")?.Trim() ?? "Smtp";

        // --- SMTP profiles (keep your current behavior) ---
        services.AddOptions<SmtpProfilesOptions>()
            .Bind(emailing.GetSection("SmtpProfiles"))
            .ValidateDataAnnotations();

        // Resolve the active SMTP profile (so we can reuse its TemplateDirectory if Graph omits one)
        services.AddSingleton<IOptions<SmtpConfiguration>>(sp =>
        {
            var profiles = sp.GetRequiredService<IOptions<SmtpProfilesOptions>>().Value;
            // Legacy fallback to Emailing:Smtp if Profiles not present (keeps backward compatibility)
            var legacy = emailing.GetSection("Smtp").Get<SmtpConfiguration>();

            if (profiles?.Profiles == null || profiles.Profiles.Count == 0)
                return Options.Create(legacy ?? new SmtpConfiguration());

            var activeKey = profiles.ActiveProfile ?? profiles.Profiles.Keys.First();
            if (!profiles.Profiles.TryGetValue(activeKey, out var active))
                throw new InvalidOperationException($"Emailing:SmtpProfiles.ActiveProfile '{activeKey}' not found.");

            return Options.Create(active);
        });

        // --- Template provider (works for both modes) ---
        services.AddSingleton(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            var logger = sp.GetRequiredService<ILogger<EmailTemplateProvider>>();

            var smtpCfg = sp.GetRequiredService<IOptions<SmtpConfiguration>>().Value;
            var graphDir = emailing.GetSection("Graph:TemplateDirectory").Get<string>();

            // Prefer Graph template dir if in Graph mode; otherwise use SMTP profile's TemplateDirectory; default to "Templates/Email"
            var templateDir =
                mode.Equals("Graph", StringComparison.OrdinalIgnoreCase) ? (graphDir ?? smtpCfg.TemplateDirectory) :
                smtpCfg.TemplateDirectory;

            return new EmailTemplateProvider(logger, env.ContentRootPath, templateDir ?? "Templates/Email");
        });

        services.AddSingleton<ITemplateRenderer, EmailTemplateRenderer>();

        // --- Provider selection ---
        if (mode.Equals("Graph", StringComparison.OrdinalIgnoreCase))
        {
            services.AddOptions<GraphMailConfiguration>()
                .Bind(emailing.GetSection("Graph"))
                .ValidateDataAnnotations();

            services.AddTransient<IEmailService, GraphEmailService>();
        }
        else
        {
            services.AddTransient<IEmailService, SmtpEmailService>();
        }

        // Keep your smoke test registration (you already had this)
        services.AddHostedService<StartupEmailSmokeTest>();

        return services;
    }
}
