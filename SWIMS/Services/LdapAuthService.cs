// -------------------------------------------------------------------
// File:    LdapAuthService.cs
// Purpose: Authenticates users against Active Directory/LDAP.
// Notes:   - Adds RootDSE BaseDN auto-discovery (prefers discovery, falls back to configured BaseDN)
//          - Correctly handles DOMAIN\user by splitting into User + Domain in NetworkCredential
//          - Escapes LDAP filters per RFC 4515
//          - Keeps AuthType.Negotiate (Kerberos/NTLM) by default
// -------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SWIMS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SWIMS.Services
{
    public class LdapAuthService : ILdapAuthService
    {
        private readonly ILogger<LdapAuthService> _logger;

        private readonly string _server;
        private readonly int _port;
        private readonly bool _useSsl;
        private readonly bool _useStartTls;
        private readonly bool _tryBothFormats;
        private readonly string? _netbiosDomain;
        private readonly string _upnSuffix;
        private readonly string _baseDn; // optional; may be empty -> discovery

        private string? _discoveredBaseDn; // cached RootDSE result

        public LdapAuthService(IConfiguration config, ILogger<LdapAuthService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _server = config["Ldap:Server"]
                      ?? throw new ArgumentNullException(nameof(config), "Ldap:Server missing in config");

            if (!int.TryParse(config["Ldap:Port"], out _port))
                throw new ArgumentNullException(nameof(config), "Ldap:Port missing in config");

            _useSsl = config.GetValue("Ldap:UseSsl", false);
            _useStartTls = config.GetValue("Ldap:UseStartTls", false);
            _tryBothFormats = config.GetValue("Ldap:TryBothFormats", true);
            _netbiosDomain = config["Ldap:NetbiosDomain"]; // e.g., "GOCD"
            _upnSuffix = config["Ldap:UpnSuffix"]
                         ?? throw new ArgumentNullException(nameof(config), "Ldap:UpnSuffix missing in config");

            // Allow BaseDN to be omitted; we'll try discovery first and only fallback to this if discovery fails.
            _baseDn = config["Ldap:BaseDN"] ?? string.Empty;
        }

        public Task<LdapUserViewModel?> AuthenticateAsync(string username, string password)
        {
            try
            {
                // 1) Connect
                _logger.LogInformation("LDAP connect {Server}:{Port} SSL={Ssl} StartTLS={StartTls}",
                    _server, _port, _useSsl, _useStartTls);

                var identifier = new LdapDirectoryIdentifier(_server, _port);
                using var connection = new LdapConnection(identifier)
                {
                    AuthType = AuthType.Negotiate // prefer Kerberos/NTLM
                };

                connection.SessionOptions.ProtocolVersion = 3;
                connection.SessionOptions.SecureSocketLayer = _useSsl; // LDAPS if true (port 636 typically)
                connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
                connection.Timeout = TimeSpan.FromSeconds(10);

                if (_useStartTls)
                {
                    // Only when explicitly enabled. If the server doesn't support it, this throws -> caught below.
                    connection.SessionOptions.StartTransportLayerSecurity(null);
                }

                // 2) Build candidate identities for bind
                var login = username?.Trim() ?? string.Empty;
                var candidates = new List<string>();

                if (login.Contains("@") || login.Contains("\\"))
                {
                    candidates.Add(login);
                }
                else
                {
                    candidates.Add($"{login}@{_upnSuffix}");

                    if (_tryBothFormats)
                    {
                        var netbios = !string.IsNullOrWhiteSpace(_netbiosDomain)
                                      ? _netbiosDomain
                                      : (_upnSuffix.Split('.').FirstOrDefault() ?? string.Empty).ToUpperInvariant();
                        if (!string.IsNullOrWhiteSpace(netbios))
                            candidates.Add($"{netbios}\\{login}");
                    }
                }

                // 3) Try to bind
                LdapException? last = null;
                string? boundAs = null;

                foreach (var candidate in candidates)
                {
                    try
                    {
                        NetworkCredential cred;
                        if (candidate.Contains("\\"))
                        {
                            var parts = candidate.Split('\\', 2);
                            cred = new NetworkCredential(parts[1], password, parts[0]); // DOMAIN\user -> Domain + User
                        }
                        else
                        {
                            cred = new NetworkCredential(candidate, password); // UPN or plain user
                        }

                        _logger.LogInformation("LDAP bind attempt as {BindUser}", candidate);
                        connection.Bind(cred);
                        boundAs = candidate;
                        _logger.LogInformation("LDAP bind OK as {BindUser}", boundAs);
                        break;
                    }
                    catch (LdapException ex)
                    {
                        last = ex;
                    }
                }

                if (boundAs is null)
                {
                    _logger.LogError(last, "LDAP bind failed for all candidates. LastMsg={Msg}", last?.ServerErrorMessage);
                    return Task.FromResult<LdapUserViewModel?>(null);
                }

                // 4) Figure out the BaseDN (prefer discovery, fallback to configured)
                var baseDn = EnsureBaseDnPreferDiscoveryFirst(connection, _baseDn);

                // 5) Look up the user entry
                string samAccountName = boundAs.Contains("@")
                    ? boundAs.Split('@')[0]
                    : (boundAs.Contains("\\") ? boundAs.Split('\\').Last() : login);

                string upnCandidate = boundAs.Contains("@") ? boundAs : $"{samAccountName}@{_upnSuffix}";

                var filter = $"(&(objectClass=user)(|(sAMAccountName={EscapeLdap(samAccountName)})(userPrincipalName={EscapeLdap(upnCandidate)})))";

                var request = new SearchRequest(
                    baseDn,
                    filter,
                    SearchScope.Subtree,
                    new[] { "distinguishedName", "memberOf", "sAMAccountName", "cn", "mail", "givenName", "sn", "userPrincipalName", "displayName" });

                var response = (SearchResponse)connection.SendRequest(request);
                _logger.LogInformation("LDAP search returned {Count} entries for {Sam}", response.Entries.Count, samAccountName);

                if (response.Entries.Count == 0)
                {
                    _logger.LogWarning("LDAP: no entries found for {Sam}", samAccountName);
                    return Task.FromResult<LdapUserViewModel?>(null);
                }

                var entry = response.Entries[0];

                var groups = entry.Attributes["memberOf"]
                             ?.GetValues(typeof(string))
                             .Cast<string>()
                             .ToList() ?? new List<string>();

                string? givenName = entry.Attributes["givenName"]?[0]?.ToString();
                string? sn = entry.Attributes["sn"]?[0]?.ToString();
                string? mail = entry.Attributes["mail"]?[0]?.ToString();
                string? upn = entry.Attributes["userPrincipalName"]?[0]?.ToString();
                string? cn = entry.Attributes["cn"]?[0]?.ToString();
                string? displayName = entry.Attributes["displayName"]?[0]?.ToString();

                var samFromEntry = entry.Attributes["sAMAccountName"]?[0]?.ToString() ?? samAccountName;

                // Email precedence: mail -> UPN -> sAM@UpnSuffix
                string effectiveEmail =
                    !string.IsNullOrWhiteSpace(mail) ? mail :
                    (!string.IsNullOrWhiteSpace(upn) ? upn : $"{samFromEntry}@{_upnSuffix}");

                // Display name precedence: displayName -> "givenName sn" -> cn -> sAM
                string effectiveDisplayName =
                    !string.IsNullOrWhiteSpace(displayName) ? displayName :
                    (!string.IsNullOrWhiteSpace(givenName) || !string.IsNullOrWhiteSpace(sn)
                        ? $"{givenName} {sn}".Trim()
                        : (!string.IsNullOrWhiteSpace(cn) ? cn : samFromEntry));

                var user = new LdapUserViewModel
                {
                    Username = samFromEntry,
                    DistinguishedName = entry.DistinguishedName,
                    DisplayName = effectiveDisplayName,
                    Email = effectiveEmail,
                    FirstName = givenName,
                    LastName = sn,
                    Groups = groups
                };

                _logger.LogInformation("LDAP user resolved: {User}", user.Username);
                return Task.FromResult<LdapUserViewModel?>(user);
            }
            catch (LdapException ex)
            {
                _logger.LogError(ex, "LDAP bind/search failed. Code={Code}; ServerMsg={Msg}", ex.ErrorCode, ex.ServerErrorMessage);
                return Task.FromResult<LdapUserViewModel?>(null);
            }
        }

        // ---------- Helpers ----------

        /// <summary>
        /// Prefer RootDSE discovery of BaseDN. If discovery fails, fallback to configuredBaseDn (if any).
        /// Caches the discovered value for subsequent requests.
        /// </summary>
        private string EnsureBaseDnPreferDiscoveryFirst(LdapConnection conn, string? configuredBaseDn)
        {
            if (!string.IsNullOrWhiteSpace(_discoveredBaseDn))
                return _discoveredBaseDn!;

            try
            {
                _discoveredBaseDn = DiscoverDefaultNamingContext(conn);
                _logger.LogInformation("LDAP discovered defaultNamingContext: {BaseDn}", _discoveredBaseDn);
                return _discoveredBaseDn!;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(configuredBaseDn))
                {
                    _logger.LogWarning(ex, "RootDSE discovery failed; falling back to configured BaseDN: {BaseDn}", configuredBaseDn);
                    return configuredBaseDn!;
                }

                // No configured fallback; bubble up
                throw;
            }
        }

        /// <summary>
        /// Queries RootDSE for defaultNamingContext.
        /// </summary>
        private static string DiscoverDefaultNamingContext(LdapConnection conn)
        {
            var req = new SearchRequest(
                "", // RootDSE
                "(objectClass=*)",
                SearchScope.Base,
                "defaultNamingContext");

            var resp = (SearchResponse)conn.SendRequest(req);

            if (resp?.Entries?.Count > 0 &&
                resp.Entries[0].Attributes.Contains("defaultNamingContext") &&
                resp.Entries[0].Attributes["defaultNamingContext"].Count > 0)
            {
                return resp.Entries[0].Attributes["defaultNamingContext"][0]!.ToString();
            }

            throw new InvalidOperationException("Could not read defaultNamingContext from RootDSE.");
        }

        /// <summary>
        /// Escapes special characters in LDAP filters per RFC 4515.
        /// </summary>
        private static string EscapeLdap(string value) =>
            (value ?? string.Empty)
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
    }
}