// -------------------------------------------------------------------
// File:    BcryptPasswordHasher.cs
// Author:  N/A
// Created: N/A
// Purpose: BCrypt-based implementation of IPasswordHasher<SwUser> for secure password hashing.
// Dependencies:
//   - Microsoft.AspNetCore.Identity.IPasswordHasher<SwUser>
//   - BCrypt.Net
//   - SWIMS.Models.SwUser
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Identity;
using SWIMS.Models;

namespace SWIMS.Services
{
    /// <summary>
    /// Implements <see cref="IPasswordHasher{SwUser}"/> using BCrypt for hashing and verifying passwords.
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher<SwUser>
    {
        /// <summary>
        /// Creates a salted BCrypt hash of the specified <paramref name="password"/>.
        /// </summary>
        /// <param name="user">
        /// The <see cref="SwUser"/> instance (unused in this implementation).
        /// </param>
        /// <param name="password">
        /// The plaintext password to hash.
        /// </param>
        /// <returns>A salted hash of the password.</returns>
        public string HashPassword(SwUser user, string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        /// <summary>
        /// Verifies that the provided plaintext password matches the stored hashed password.
        /// </summary>
        /// <param name="user">
        /// The <see cref="SwUser"/> instance (unused in this implementation).
        /// </param>
        /// <param name="hashedPassword">
        /// The stored BCrypt hash to verify against.
        /// </param>
        /// <param name="providedPassword">
        /// The plaintext password to verify.
        /// </param>
        /// <returns>
        /// <see cref="PasswordVerificationResult.Success"/> if the passwords match;
        /// otherwise, <see cref="PasswordVerificationResult.Failed"/>.
        /// </returns>
        public PasswordVerificationResult VerifyHashedPassword(
            SwUser user,
            string hashedPassword,
            string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}
