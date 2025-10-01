using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SWIMS.Services.Email;

/// <summary>
/// Loads *.html templates from a physical directory (default: Templates/Emails).
/// First HTML comment can include the subject, e.g.:
/// <!-- Subject: Confirm your email, {{FirstName}} -->
/// Body supports simple {{Token}} replacement.
/// </summary>
public sealed class EmailTemplateProvider
{
    private readonly ILogger<EmailTemplateProvider> _logger;
    private readonly Dictionary<string, (string Subject, string Html)> _cache;
    public string DirectoryPath { get; }

    /// <summary>Expose loaded keys for diagnostics.</summary>
    public IReadOnlyCollection<string> Keys => _cache.Keys;

    public EmailTemplateProvider(ILogger<EmailTemplateProvider> logger, string basePath, string? physicalDirectory)
    {
        _logger = logger;
        DirectoryPath = ResolveDirectory(basePath, physicalDirectory);
        _cache = LoadAll(DirectoryPath);

        if (_cache.Count == 0)
        {
            _logger.LogWarning("EmailTemplateProvider: No templates found in directory: {Dir}. Expected *.html files.", DirectoryPath);
        }
        else
        {
            _logger.LogInformation("EmailTemplateProvider: Loaded {Count} templates from {Dir}. Keys: {Keys}",
                _cache.Count, DirectoryPath, string.Join(", ", _cache.Keys.OrderBy(k => k)));
        }
    }

    public bool TryGet(string key, out (string Subject, string Html) template) =>
        _cache.TryGetValue(key, out template);

    private static string ResolveDirectory(string basePath, string? configured)
    {
        // normalize: handle both / and \ from JSON
        static string Norm(string p) =>
            Path.GetFullPath(p.Replace('\\', Path.DirectorySeparatorChar)
                              .Replace('/', Path.DirectorySeparatorChar));

        if (!string.IsNullOrWhiteSpace(configured))
        {
            // Absolute path?
            if (Path.IsPathRooted(configured))
                return Norm(configured);

            // Relative to content root
            return Norm(Path.Combine(basePath, configured));
        }

        // Default: ContentRoot/Templates/Emails
        return Norm(Path.Combine(basePath, "Templates", "Emails"));
    }


    private static Dictionary<string, (string Subject, string Html)> LoadAll(string dir)
    {
        var dict = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(dir))
            return dict;

        foreach (var path in Directory.EnumerateFiles(dir, "*.html", SearchOption.TopDirectoryOnly))
        {
            var key = Path.GetFileNameWithoutExtension(path);
            var content = File.ReadAllText(path, Encoding.UTF8);

            // Extract subject from first HTML comment line
            var m = Regex.Match(content, @"<!--\s*Subject:\s*(.*?)\s*-->", RegexOptions.IgnoreCase);
            var subject = m.Success ? m.Groups[1].Value.Trim() : key;

            dict[key] = (subject, content);
        }

        return dict;
    }
}
