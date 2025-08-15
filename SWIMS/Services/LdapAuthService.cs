// -------------------------------------------------------------------
// File:    LdapAuthService.cs
// Author:  N/A
// Created: N/A
// Purpose: Authenticates users against an LDAP directory using configured settings.
// Dependencies:
//   - IConfiguration (for LDAP settings)
//   - ILdapAuthService (interface)
//   - LdapUser (model)
//   - System.DirectoryServices.Protocols
// Notes:
//   Hardened implementation: Negotiate auth by default, StartTLS is opt-in, clear logging,
//   timeouts and referral control, optional dual username format attempts.
// -------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SWIMS.Services
{
    /// <summary>
    /// LDAP authentication service.
    /// </summary>
    /// <remarks>
    /// This implementation prefers <see cref="AuthType.Negotiate"/> (Kerberos/NTLM)
    /// over simple bind, makes StartTLS opt-in via configuration, and logs detailed
    /// diagnostics on failures without throwing to the caller.
    /// </remarks>
    public class LdapAuthService : ILdapAuthService
    {
        private readonly ILogger<LdapAuthService> _logger;

        private readonly string _server;
        private readonly int _port;
        private readonly bool _useSsl;

        // NEW: Opt-in toggle to upgrade clear LDAP on 389 to TLS via StartTLS.
        private readonly bool _useStartTls;

        // NEW: Try both UPN and DOMAIN\user when user types only "john".
        private readonly bool _tryBothFormats;

        // NEW: Optional explicit NetBIOS domain for DOMAIN\user attempts (e.g., "ORLEINDUSTRIES").
        private readonly string? _netbiosDomain;

        private readonly string _upnSuffix;
        private readonly string _baseDn;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAuthService"/> class.
        /// </summary>
        /// <param name="config">Application configuration used to hydrate LDAP settings.</param>
        /// <param name="logger">Logger for emitting diagnostic messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when required LDAP settings are missing.</exception>
        public LdapAuthService(IConfiguration config, ILogger<LdapAuthService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Read and validate required settings.
            _server = config["Ldap:Server"]
                      ?? throw new ArgumentNullException(nameof(config), "Ldap:Server missing in config");

            if (!int.TryParse(config["Ldap:Port"], out _port))
                throw new ArgumentNullException(nameof(config), "Ldap:Port missing in config");

            _useSsl = config.GetValue("Ldap:UseSsl", false);
            _useStartTls = config.GetValue("Ldap:UseStartTls", false);
            _tryBothFormats = config.GetValue("Ldap:TryBothFormats", true);
            _netbiosDomain = config["Ldap:NetbiosDomain"];

            _upnSuffix = config["Ldap:UpnSuffix"]
                         ?? throw new ArgumentNullException(nameof(config), "Ldap:UpnSuffix missing in config");

            _baseDn = config["Ldap:BaseDN"]
                      ?? throw new ArgumentNullException(nameof(config), "Ldap:BaseDN missing in config");
        }

        /// <summary>
        /// Attempts to authenticate a user against LDAP and, on success,
        /// returns a minimal <see cref="LdapUser"/> populated from directory attributes.
        /// </summary>
        /// <param name="username">The username supplied by the user (short name, UPN, or DOMAIN\user).</param>
        /// <param name="password">The plaintext password supplied by the user.</param>
        /// <returns>
        /// A task producing a <see cref="LdapUser"/> when authentication succeeds; otherwise <c>null</c>.
        /// </returns>
        public Task<LdapUser?> AuthenticateAsync(string username, string password)
        {
            try
            {
                // 1) Log the exact endpoint/mode for immediate visibility during troubleshooting.
                _logger.LogInformation("LDAP connect {Server}:{Port} SSL={Ssl} StartTLS={StartTls}",
                    _server, _port, _useSsl, _useStartTls);

                var identifier = new LdapDirectoryIdentifier(_server, _port);

                using var connection = new LdapConnection(identifier)
                {
                    // Prefer Negotiate (Kerberos/NTLM) over Basic (simple bind)
                    AuthType = AuthType.Negotiate
                };

                // 2) Enforce LDAP v3 and configure SSL/StartTLS
                connection.SessionOptions.ProtocolVersion = 3;
                connection.SessionOptions.SecureSocketLayer = _useSsl; // LDAPS if true
                connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
                connection.Timeout = TimeSpan.FromSeconds(10);

                if (_useStartTls)
                {
                    // Only attempt StartTLS when explicitly enabled via config.
                    connection.SessionOptions.StartTransportLayerSecurity(null);
                }

                // 3) Build candidate bind names
                string login = username?.Trim() ?? string.Empty;
                var candidates = new List<string>();

                // If the user already supplied UPN or DOMAIN\user, try exactly that first.
                if (login.Contains("@") || login.Contains("\\"))
                {
                    candidates.Add(login);
                }
                else
                {
                    // Try UPN form first
                    candidates.Add($"{login}@{_upnSuffix}");

                    // Optionally also try DOMAIN\user
                    if (_tryBothFormats)
                    {
                        var netbios = !string.IsNullOrWhiteSpace(_netbiosDomain)
                                      ? _netbiosDomain
                                      : (_upnSuffix.Split('.').FirstOrDefault() ?? string.Empty).ToUpperInvariant();

                        if (!string.IsNullOrWhiteSpace(netbios))
                            candidates.Add($"{netbios}\\{login}");
                    }
                }

                // 4) Attempt to bind with each candidate
                LdapException? last = null;
                string? boundAs = null;

                foreach (var candidate in candidates)
                {
                    try
                    {
                        _logger.LogInformation("LDAP bind attempt as {BindUser}", candidate);
                        connection.Bind(new NetworkCredential(candidate, password));
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
                    _logger.LogError(last, "LDAP bind failed for all candidates. LastMsg={Msg}",
                        last?.ServerErrorMessage);
                    return Task.FromResult<LdapUser?>(null);
                }

                // 5) Determine sAMAccountName for the search
                string samAccountName = boundAs.Contains("@")
                    ? boundAs.Split('@')[0]
                    : (boundAs.Contains("\\") ? boundAs.Split('\\').Last() : login);

                var filter = $"(sAMAccountName={samAccountName})";
                var request = new SearchRequest(
                    _baseDn,
                    filter,
                    SearchScope.Subtree,
                    new[] { "distinguishedName", "memberOf", "sAMAccountName", "cn", "mail", "givenName", "sn", "userPrincipalName", "displayName" });

                var response = (SearchResponse)connection.SendRequest(request);
                _logger.LogInformation("LDAP search returned {Count} entries for {Sam}", response.Entries.Count, samAccountName);

                if (response.Entries.Count == 0)
                {
                    _logger.LogWarning("LDAP: no entries found for {Sam}", samAccountName);
                    return Task.FromResult<LdapUser?>(null);
                }

                var entry = response.Entries[0];

                var groups = entry.Attributes["memberOf"]
                             ?.GetValues(typeof(string))
                             .Cast<string>()
                             .ToList() ?? new List<string>();

                // extract profile attributes and choose a real email 
                string? givenName = entry.Attributes["givenName"]?[0]?.ToString();
                string? sn = entry.Attributes["sn"]?[0]?.ToString();
                string? mail = entry.Attributes["mail"]?[0]?.ToString();
                string? upn = entry.Attributes["userPrincipalName"]?[0]?.ToString();
                string? cn = entry.Attributes["cn"]?[0]?.ToString();
                string? displayName = entry.Attributes["displayName"]?[0]?.ToString();

                // canonical sAMAccountName from entry if available, else use the search key
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

                var user = new LdapUser
                {
                    Username = samFromEntry,
                    DistinguishedName = entry.DistinguishedName,
                    DisplayName = effectiveDisplayName,
                    Email = effectiveEmail,      // <— real email
                    FirstName = givenName,       // <— LDAP givenName
                    LastName = sn,               // <— LDAP sn (surname)
                    Groups = groups
                };

                _logger.LogInformation("LDAP user resolved: {User}", user.Username);
                return Task.FromResult<LdapUser?>(user);
            }
            catch (LdapException ex)
            {
                // Surface the exact directory error to logs for quick triage (e.g., 81, 49/52e, etc.)
                _logger.LogError(ex, "LDAP bind/search failed. Code={Code}; ServerMsg={Msg}",
                    ex.ErrorCode, ex.ServerErrorMessage);
                return Task.FromResult<LdapUser?>(null);
            }
        }
    }
}
