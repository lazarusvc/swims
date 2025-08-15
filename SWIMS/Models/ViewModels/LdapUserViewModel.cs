// -------------------------------------------------------------------
// File:    LdapUser.cs
// Author:  N/A
// Created: N/A
// Purpose: Represents an LDAP user with their credentials and group memberships.
// Dependencies:
//   - System.Collections.Generic
//   - System.Linq
// -------------------------------------------------------------------

namespace SWIMS.Models.ViewModels
{
    /// <summary>
    /// Model representing a user retrieved from an LDAP directory.
    /// </summary>
    public class LdapUserViewModel
    {
        /// <summary>
        /// The LDAP username (e.g., sAMAccountName) of the user.
        /// </summary>
        // Provide defaults so these never stay null
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The Distinguished Name (DN) of the user in the LDAP directory.
        /// </summary>
        public string DistinguishedName { get; set; } = string.Empty;

        /// <summary>
        /// Display name as reported by LDAP (e.g., <c>displayName</c> or <c>cn</c>).
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// The user's email address as reported by LDAP (e.g., the <c>mail</c> attribute).
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// The user's given name (LDAP <c>givenName</c> attribute).
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// The user's surname (LDAP <c>sn</c> attribute).
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// A collection of group names (or DNs) that the user belongs to.
        /// </summary>
        public IEnumerable<string> Groups { get; set; } = Enumerable.Empty<string>();
    }
}
