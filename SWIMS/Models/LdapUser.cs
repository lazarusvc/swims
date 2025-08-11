// -------------------------------------------------------------------
// File:    LdapUser.cs
// Author:  N/A
// Created: N/A
// Purpose: Represents an LDAP user with their credentials and group memberships.
// Dependencies:
//   - System.Collections.Generic
//   - System.Linq
// -------------------------------------------------------------------

namespace SWIMS.Models
{
    /// <summary>
    /// Model representing a user retrieved from an LDAP directory.
    /// </summary>
    public class LdapUser
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
        /// A collection of group names (or DNs) that the user belongs to.
        /// </summary>
        public IEnumerable<string> Groups { get; set; } = Enumerable.Empty<string>();
    }
}
