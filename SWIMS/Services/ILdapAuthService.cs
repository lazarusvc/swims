// -------------------------------------------------------------------
// File:    ILdapAuthService.cs
// Author:  N/A
// Created: N/A
// Purpose: Defines the contract for LDAP authentication operations.
// Dependencies:
//   - SWIMS.Models.LdapUser
// -------------------------------------------------------------------

using System.Threading.Tasks;
using SWIMS.Models;

namespace SWIMS.Services
{
    /// <summary>
    /// Defines the contract for services that authenticate users against an LDAP directory.
    /// </summary>
    public interface ILdapAuthService
    {
        /// <summary>
        /// Attempts to authenticate a user against the configured LDAP directory.
        /// </summary>
        /// <param name="username">
        /// The LDAP username (sAMAccountName) of the user to authenticate.
        /// </param>
        /// <param name="password">
        /// The plaintext password to verify against the LDAP store.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that resolves to an <see cref="LdapUser"/> instance
        /// if authentication succeeds; otherwise, <c>null</c>.
        /// </returns>
        Task<LdapUser?> AuthenticateAsync(string username, string password);
    }
}
