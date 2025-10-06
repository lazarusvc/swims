using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HandlebarsDotNet;

namespace SWIMS.Services.Email;

/// <summary>
/// Handlebars-based renderer for email templates.
/// - Compiles & caches the Subject and HTML from EmailTemplateProvider
/// - Supports standard {{tokens}} and {{#if}} / {{#unless}} blocks
/// - Adds helper: {{nl2br text}} and {{urlencode value}}
/// </summary>
public sealed class EmailTemplateRenderer : ITemplateRenderer
{
    private readonly EmailTemplateProvider _provider;

    // Cache compiled templates by key
    private static readonly ConcurrentDictionary<string, (HandlebarsTemplate<object, object> Subject, HandlebarsTemplate<object, object> Html)> Cache
        = new();

    private static bool _helpersRegistered;

    public EmailTemplateRenderer(EmailTemplateProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        RegisterHelpersOnce();
    }

    public Task<EmailTemplate> RenderAsync(string templateKey, object model)
    {
        if (!_provider.TryGet(templateKey, out var tpl))
        {
            var available = string.Join(", ", _provider.Keys);
            var dir = _provider.DirectoryPath;
            throw new InvalidOperationException(
                $"Email template '{templateKey}' not found. Directory: '{dir}'. Available keys: {available}");
        }

        // Compile (once per key)
        var compiled = Cache.GetOrAdd(templateKey, _ =>
        {
            var subj = Handlebars.Compile(tpl.Subject ?? string.Empty);
            var html = Handlebars.Compile(tpl.Html ?? string.Empty);
            return (subj, html);
        });

        // Render with the anonymous model you pass from your code
        var subject = compiled.Subject(model);
        var html = compiled.Html(model);

        return Task.FromResult(new EmailTemplate
        {
            Key = templateKey,
            Subject = subject,
            HtmlBody = html
        });
    }

    private static void RegisterHelpersOnce()
    {
        if (_helpersRegistered) return;

        // Converts newline characters to <br> with HTML-encoding first.
        Handlebars.RegisterHelper("nl2br", (writer, context, parameters) =>
        {
            var s = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
            s = System.Net.WebUtility.HtmlEncode(s).Replace("\r\n", "<br>").Replace("\n", "<br>");
            writer.WriteSafeString(s);
        });

        // URL-encode a string for use in query strings
        Handlebars.RegisterHelper("urlencode", (writer, context, parameters) =>
        {
            var s = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
            writer.Write(System.Net.WebUtility.UrlEncode(s));
        });

        _helpersRegistered = true;
    }
}
