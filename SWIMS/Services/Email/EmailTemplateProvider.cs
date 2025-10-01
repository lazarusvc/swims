using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SWIMS.Services.Email;

/// <summary>
/// Loads *.html templates from a physical directory (default: Templates/Emails).
/// First HTML comment can include the subject, e.g.:
/// <!-- Subject: Confirm your email, {{FirstName}} -->
/// Body supports simple {{Token}} replacement.
/// </summary>
public sealed class EmailTemplateProvider
{
    private readonly Dictionary<string, (string Subject, string Html)> _cache;

    public EmailTemplateProvider(string basePath, string? physicalDirectory)
    {
        var dir = ResolveDirectory(basePath, physicalDirectory);
        _cache = LoadAll(dir);
    }

    public bool TryGet(string key, out (string Subject, string Html) template) =>
        _cache.TryGetValue(key, out template);

    private static string ResolveDirectory(string basePath, string? configured)
    {
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (Directory.Exists(configured)) return configured;
            // Resolve relative to application base
            var rel = Path.Combine(basePath, configured);
            if (Directory.Exists(rel)) return rel;
        }
        var defaultDir = Path.Combine(basePath, "Templates", "Emails");
        Directory.CreateDirectory(defaultDir);
        return defaultDir;
    }

    private static Dictionary<string, (string Subject, string Html)> LoadAll(string dir)
    {
        var dict = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.EnumerateFiles(dir, "*.html"))
        {
            var key = Path.GetFileNameWithoutExtension(path);
            var content = File.ReadAllText(path, Encoding.UTF8);
            var m = Regex.Match(content, @"<!--\s*Subject:\s*(.*?)\s*-->", RegexOptions.IgnoreCase);
            var subject = m.Success ? m.Groups[1].Value.Trim() : key;
            dict[key] = (subject, content);
        }
        return dict;
    }
}
