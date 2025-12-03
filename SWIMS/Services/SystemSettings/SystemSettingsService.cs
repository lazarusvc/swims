using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SWIMS.Services.SystemSettings
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly IConfiguration _config;
        private readonly IConfigurationRoot? _configRoot;
        private readonly IHostEnvironment _env;
        private readonly ILogger<SystemSettingsService> _logger;

        // Friendly names & descriptions for known top-level sections
        private static readonly Dictionary<string, (string Display, string? Description)> SectionMetadata =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["App"] = (
                    "Application",
                    "Core application metadata and global behavior toggles."
                ),
                ["ConnectionStrings"] = (
                    "Database Connections",
                    "Connection strings for the main database and related services."
                ),
                ["Emailing"] = (
                    "Email & SMTP",
                    "Outbound email mode, SMTP profiles, and notification behavior."
                ),
                ["Reporting"] = (
                    "Reporting",
                    "Reporting engine configuration and integrations."
                ),
                ["Support"] = (
                    "Support & Helpdesk",
                    "Support contact details and helpdesk configuration."
                ),
                ["Authentication"] = (
                    "Authentication",
                    "Authentication and security-related settings."
                )
            };

        public SystemSettingsService(
            IConfiguration config,
            IHostEnvironment env,
            ILogger<SystemSettingsService> logger)
        {
            _config = config;
            _configRoot = config as IConfigurationRoot;
            _env = env;
            _logger = logger;
        }

        public Task<SystemSettingsOverview> GetOverviewAsync(CancellationToken ct = default)
        {
            var overview = new SystemSettingsOverview
            {
                ActiveEnvironment = _env.EnvironmentName
            };

            // Use all JSON providers to determine "from appsettings" status.
            var appSettingsKeys = GetJsonProviderTopLevelKeys();

            foreach (var section in _config.GetChildren())
            {
                // Skip noisy/infra sections by default
                if (string.Equals(section.Key, "Logging", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(section.Key, "AllowedHosts", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fromAppSettings = appSettingsKeys.Contains(section.Key);
                var hasValues = HasAnyValues(section);
                var meta = GetSectionMetadata(section.Key);

                overview.Sections.Add(new SystemSettingsSectionSummary
                {
                    Key = section.Key,
                    DisplayName = meta.Display,
                    Description = meta.Description,
                    IsFromAppSettings = fromAppSettings,
                    IsConfigured = hasValues
                });
            }

            overview.Sections = overview.Sections
                .OrderBy(s => s.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Task.FromResult(overview);
        }

        public Task<SystemSettingsSection> GetSectionAsync(string key, string? environment, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Section key is required.", nameof(key));

            var sectionConfig = _config.GetSection(key);
            var meta = GetSectionMetadata(key);

            var section = new SystemSettingsSection
            {
                Key = key,
                DisplayName = meta.Display,
                Description = meta.Description,
                EnvironmentName = string.IsNullOrWhiteSpace(environment) ? _env.EnvironmentName : environment
            };

            var items = new List<SystemSettingsItem>();
            AddItemsRecursive(sectionConfig, items);

            section.Items = items
                .OrderBy(i => i.KeyPath, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Task.FromResult(section);
        }

        public async Task SaveSectionAsync(SystemSettingsSection section, string? environment, CancellationToken ct = default)
        {
            if (section is null) throw new ArgumentNullException(nameof(section));

            // Guard: only allow JSON writes in Development for now.
            if (!_env.IsDevelopment())
            {
                _logger.LogWarning("Attempted to save system settings in non-development environment. Operation blocked.");
                throw new InvalidOperationException("Editing configuration is currently only enabled in the Development environment.");
            }

            var envName = string.IsNullOrWhiteSpace(environment)
                ? _env.EnvironmentName
                : environment;

            if (string.IsNullOrWhiteSpace(envName))
            {
                envName = "Development";
            }

            var (filePath, isFallbackToBase) = ResolveConfigFilePath(envName);

            JsonObject rootObj;
            string? header = null;

            if (filePath is not null && File.Exists(filePath))
            {
                var text = await File.ReadAllTextAsync(filePath, ct);
                if (string.IsNullOrWhiteSpace(text))
                {
                    rootObj = new JsonObject();
                }
                else
                {
                    try
                    {
                        rootObj = ParseJsonWithHeader(text, out header);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to parse JSON configuration file {FilePath}", filePath);
                        var msg =
                            $"Could not parse configuration file '{Path.GetFileName(filePath)}'. " +
                            "Please ensure it is valid JSON (no inline comments, trailing commas, or partial content) " +
                            "before editing via this UI. Top-of-file comments are supported.";
                        throw new InvalidOperationException(msg);
                    }
                }
            }
            else
            {
                // File does not exist yet – we will create it at the resolved path.
                if (filePath is null)
                {
                    var contentRoot = GetContentRoot();
                    filePath = Path.Combine(contentRoot, $"appsettings.{envName}.json");
                }
                rootObj = new JsonObject();
            }

            // backup before overwriting
            if (File.Exists(filePath!))
            {
                var backupPath = filePath! + "." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".bak";
                File.Copy(filePath!, backupPath, overwrite: false);
            }

            foreach (var item in section.Items)
            {
                ApplyItem(rootObj, item);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var bodyJson = rootObj.ToJsonString(options);
            string final;

            if (!string.IsNullOrEmpty(header))
            {
                // Ensure header ends with a newline
                if (!header.EndsWith(Environment.NewLine))
                {
                    header += Environment.NewLine;
                }
                final = header + bodyJson;
            }
            else
            {
                final = bodyJson;
            }

            await File.WriteAllTextAsync(filePath!, final, ct);

            _logger.LogInformation(
                "System settings section {SectionKey} saved to {FilePath}. (Fallback to base: {Fallback})",
                section.Key, filePath, isFallbackToBase);

            _configRoot?.Reload();
        }

        // ----------------- helpers -----------------

        /// <summary>
        /// Uses all JsonConfigurationProvider instances in the current configuration root
        /// to determine which top-level keys come from JSON config files.
        /// This covers appsettings.json, appsettings.{Environment}.json, and any other JSON files
        /// you have wired in the host builder.
        /// </summary>
        private HashSet<string> GetJsonProviderTopLevelKeys()
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // If we weren't constructed with a root, we can't introspect providers.
            if (_configRoot is null)
                return result;

            foreach (var provider in _configRoot.Providers)
            {
                var providerType = provider.GetType();
                var fullName = providerType.FullName ?? string.Empty;

                // Only look at JSON-based providers (appsettings.json, appsettings.Development.json, etc.)
                if (!fullName.Contains("JsonConfigurationProvider", StringComparison.OrdinalIgnoreCase))
                    continue;

                IDictionary<string, string>? data = null;

                // Try private backing field first (older implementations)
                var dataField = providerType.GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic);
                if (dataField?.GetValue(provider) is IDictionary<string, string> fieldData)
                {
                    data = fieldData;
                }
                else
                {
                    // Fallback: protected/internal Data property (newer implementations)
                    var dataProp = providerType.GetProperty(
                        "Data",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    if (dataProp?.GetValue(provider) is IDictionary<string, string> propData)
                    {
                        data = propData;
                    }
                }

                if (data is null)
                    continue;

                foreach (var key in data.Keys)
                {
                    // Keys are flattened like "Emailing:Smtp:Host" → we only care about "Emailing"
                    var top = key
                        .Split(':', StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(top))
                    {
                        result.Add(top);
                    }
                }
            }

            return result;
        }


        private (string? FilePath, bool IsFallbackToBase) ResolveConfigFilePath(string envName)
        {
            var contentRoot = GetContentRoot();

            var envFile = envName.Equals("Production", StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(contentRoot, "appsettings.Production.json")
                : Path.Combine(contentRoot, $"appsettings.{envName}.json");

            var baseFile = Path.Combine(contentRoot, "appsettings.json");

            if (File.Exists(envFile))
            {
                return (envFile, false);
            }

            if (File.Exists(baseFile))
            {
                return (baseFile, true);
            }

            // No file yet – we'll create env-specific file later.
            return (envFile, false);
        }

        private string GetContentRoot()
        {
            if (_env is Microsoft.Extensions.Hosting.IHostEnvironment hostEnv &&
                !string.IsNullOrWhiteSpace(hostEnv.ContentRootPath))
            {
                return hostEnv.ContentRootPath;
            }

            return AppContext.BaseDirectory;
        }

        /// <summary>
        /// Parses a JSON string that may have a top-of-file header (e.g., comments)
        /// before the first '{'. The header is returned as-is; the JSON body is parsed.
        /// </summary>
        private static JsonObject ParseJsonWithHeader(string text, out string? header)
        {
            header = null;
            if (string.IsNullOrWhiteSpace(text))
                return new JsonObject();

            var firstBrace = text.IndexOf('{');
            if (firstBrace > 0)
            {
                // Treat everything before the first '{' as header (comments, etc.)
                header = text.Substring(0, firstBrace);
                var jsonPart = text.Substring(firstBrace);

                var node = JsonNode.Parse(jsonPart);
                if (node is JsonObject objFromWithHeader)
                    return objFromWithHeader;

                return new JsonObject();
            }

            // No header, parse the whole string
            var rootNode = JsonNode.Parse(text);
            return rootNode as JsonObject ?? new JsonObject();
        }

        private static bool HasAnyValues(IConfiguration section)
        {
            foreach (var child in section.GetChildren())
            {
                if (!string.IsNullOrEmpty(child.Value))
                    return true;

                if (child.GetChildren().Any())
                {
                    if (HasAnyValues(child)) return true;
                }
            }
            return false;
        }

        private static void AddItemsRecursive(IConfiguration section, IList<SystemSettingsItem> items)
        {
            foreach (var child in section.GetChildren())
            {
                var grandchildren = child.GetChildren().ToList();
                if (grandchildren.Any())
                {
                    AddItemsRecursive(child, items);
                }
                else
                {
                    var path = child.Path; // e.g. "Emailing:Smtp:Host"
                    var value = child.Value;
                    var type = InferType(value);
                    var isSecret = IsSecretKey(path);

                    items.Add(new SystemSettingsItem
                    {
                        KeyPath = path,
                        DisplayName = child.Key,
                        Description = null,
                        Value = value,
                        DataType = type,
                        IsSecret = isSecret,
                        IsRequired = false
                    });
                }
            }
        }

        private static string InferType(string? value)
        {
            if (value is null) return "string";

            if (bool.TryParse(value, out _)) return "bool";
            if (int.TryParse(value, out _)) return "int";
            if (long.TryParse(value, out _)) return "long";
            if (double.TryParse(value, out _)) return "double";

            return "string";
        }

        private static bool IsSecretKey(string keyPath)
        {
            var last = keyPath.Split(':', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
            last = last.ToLowerInvariant();

            return last.Contains("password")
                   || last.Contains("secret")
                   || last.Contains("apikey")
                   || last.Contains("api-key")
                   || last.EndsWith("key")
                   || last.Contains("token");
        }

        private static void ApplyItem(JsonObject root, SystemSettingsItem item)
        {
            if (string.IsNullOrWhiteSpace(item.KeyPath))
                return;

            var segments = item.KeyPath.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0) return;

            JsonObject current = root;

            for (int i = 0; i < segments.Length; i++)
            {
                var seg = segments[i];
                var isLast = i == segments.Length - 1;

                if (isLast)
                {
                    current[seg] = item.Value is null ? null : JsonValue.Create(item.Value);
                }
                else
                {
                    if (current[seg] is JsonObject childObj)
                    {
                        current = childObj;
                    }
                    else
                    {
                        var newObj = new JsonObject();
                        current[seg] = newObj;
                        current = newObj;
                    }
                }
            }
        }

        private static (string Display, string? Description) GetSectionMetadata(string key)
        {
            if (SectionMetadata.TryGetValue(key, out var meta))
                return meta;

            return (key, null);
        }
    }
}
