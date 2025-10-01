using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SWIMS.Services.Email;

/// <summary>
/// Minimal {{Token}} replacement against public properties of the provided model.
/// Swap this with Scriban/Razor later by re-implementing ITemplateRenderer.
/// </summary>
public sealed class EmailTemplateRenderer : ITemplateRenderer
{
    private static readonly Regex Token = new(@"\{\{\s*(?<name>[A-Za-z0-9_.]+)\s*\}\}", RegexOptions.Compiled);
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Cache = new();

    private readonly EmailTemplateProvider _provider;

    public EmailTemplateRenderer(EmailTemplateProvider provider) => _provider = provider;

    public Task<EmailTemplate> RenderAsync(string templateKey, object model)
    {
        if (!_provider.TryGet(templateKey, out var tpl))
        {
            var available = _provider.Keys;
            var dir = _provider.DirectoryPath;
            var list = available is { Count: > 0 } ? string.Join(", ", available) : "(none)";
            throw new InvalidOperationException(
                $"Email template '{templateKey}' not found. " +
                $"Directory scanned: '{dir}'. " +
                $"Available keys: {list}. " +
                $"If you use a relative path, it's relative to the app ContentRoot (usually the project folder in dev).");
        }

        var subject = ReplaceTokens(tpl.Subject, model);
        var html = ReplaceTokens(tpl.Html, model);
        return Task.FromResult(new EmailTemplate { Key = templateKey, Subject = subject, HtmlBody = html });
    }

    private static string ReplaceTokens(string input, object model)
    {
        if (model is null) return input;
        var props = Cache.GetOrAdd(model.GetType(), t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public));

        return Token.Replace(input, m =>
        {
            var name = m.Groups["name"].Value;
            foreach (var p in props)
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                    return p.GetValue(model)?.ToString() ?? string.Empty;
            }
            return m.Value; // leave token if no match
        });
    }
}
