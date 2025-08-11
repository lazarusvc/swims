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
// -------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using SWIMS.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Threading.Tasks;

namespace SWIMS.Services
{
    /// <summary>
    /// Service for authenticating <see cref="SwUser"/> instances against an LDAP directory.
    /// Implements <see cref="ILdapAuthService"/> and uses <see cref="IConfiguration"/> for settings.
    /// </summary>
    public class LdapAuthService : ILdapAuthService
    {
        private readonly string _server;
        private readonly int _port;
        private readonly bool _useSsl;
        private readonly string _upnSuffix;
        private readonly string _baseDn;

        /// <summary>
        /// Initializes a new instance of <see cref="LdapAuthService"/> using configuration values.
        /// </summary>
        /// <param name="config">
        /// The <see cref="IConfiguration"/> containing LDAP settings:
        /// <c>Ldap:Server</c>, <c>Ldap:Port</c>, <c>Ldap:UseSsl</c>,
        /// <c>Ldap:UpnSuffix</c>, and <c>Ldap:BaseDN</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any required configuration key is missing or invalid.
        /// </exception>
        public LdapAuthService(IConfiguration config)
        {
            _server = config["Ldap:Server"]
                      ?? throw new ArgumentNullException(nameof(config), "Ldap:Server missing in config");
            _port = int.Parse(config["Ldap:Port"]
                               ?? throw new ArgumentNullException(nameof(config), "Ldap:Port missing in config"));

            _useSsl = config.GetValue<bool>("Ldap:UseSsl", true);

            _upnSuffix = config["Ldap:UpnSuffix"]
                        ?? throw new ArgumentNullException(nameof(config), "Ldap:UpnSuffix missing in config");

            _baseDn = config["Ldap:BaseDN"]
                        ?? throw new ArgumentNullException(nameof(config), "Ldap:BaseDN missing in config");
        }

        /// <summary>
        /// Attempts to bind to the LDAP server using the provided credentials.
        /// </summary>
        /// <param name="username">
        /// The LDAP username (sAMAccountName); the configured UPN suffix is appended.
        /// </param>
        /// <param name="password">The plaintext password to authenticate.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> resolving to an <see cref="LdapUser"/>
        /// on successful authentication; otherwise, <c>null</c>.
        /// </returns>
        public Task<LdapUser?> AuthenticateAsync(string username, string password)
        {
            try
            {
                // 1) Establish the connection
                var identifier = new LdapDirectoryIdentifier(_server, _port);
                using var connection = new LdapConnection(identifier)
                {
                    AuthType = AuthType.Basic
                };

                // 2) Enforce LDAP v3 and configure SSL/StartTLS
                connection.SessionOptions.ProtocolVersion = 3;
                connection.SessionOptions.SecureSocketLayer = _useSsl;
                if (!_useSsl)
                {
                    connection.SessionOptions.StartTransportLayerSecurity(null);
                }

                // 3) 
                string bindUsername;

                // Allow UPN, NetBIOS, or short name
                if (username.Contains("@") || username.Contains("\\"))
                {
                    bindUsername = username;
                }
                else
                {
                    bindUsername = $"{username}@{_upnSuffix}";
                }

                // Always extract short form username for sAMAccountName
                var samAccountName = bindUsername.Contains("@")
                    ? bindUsername.Split('@')[0]
                    : bindUsername.Split('\\').Last();


                // 4) Bind (authenticate) with UPN
                var credential = new NetworkCredential(bindUsername, password);
                connection.Bind(credential);

                Console.WriteLine("[LDAP] Bind successful.");

                // 5) Search for the user entry
                var filter = $"(sAMAccountName={samAccountName})";
                var request = new SearchRequest(
                    _baseDn,
                    filter,
                    SearchScope.Subtree,
                    new[] { "distinguishedName", "memberOf", "sAMAccountName", "cn", "mail" });
                var response = (SearchResponse)connection.SendRequest(request);

                Console.WriteLine($"[LDAP] Entries returned: {response.Entries.Count}");



                Console.WriteLine($"[LDAP] Attempting bind with input: {username}");

                Console.WriteLine($"[LDAP] Bind using: {bindUsername}");
                Console.WriteLine($"[LDAP] Search filter: (sAMAccountName={samAccountName})");


                if (response.Entries.Count == 0)
                {
                    Console.WriteLine("[LDAP] No entries found for user.");
                    return Task.FromResult<LdapUser?>(null);
                }

                var entry = response.Entries[0];
                var dn = entry.DistinguishedName;
                var groups = entry.Attributes["memberOf"]
                                 ?.GetValues(typeof(string))
                                 .Cast<string>()
                                 .ToList()
                             ?? new List<string>();

                // 6) Return our LdapUser model
                var samAccount = entry.Attributes["sAMAccountName"]?[0]?.ToString();

                var user = new LdapUser
                {
                    Username = samAccount ?? username, // fallback if attribute missing
                    DistinguishedName = dn,
                    Groups = groups
                };

                Console.WriteLine($"[LDAP] Successfully resolved LDAP user: {user.Username}");
                return Task.FromResult<LdapUser?>(user);

            }
            catch (LdapException)
            {
                return Task.FromResult<LdapUser?>(null);
            }
        }
    }
}
