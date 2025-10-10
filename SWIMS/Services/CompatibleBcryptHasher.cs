using Microsoft.AspNetCore.Identity;
using SWIMS.Models;

namespace SWIMS.Services
{
    /// <summary>
    /// Accepts bcrypt ($2a/$2b/$2y) and legacy Identity PBKDF2 (AQAAAA...) hashes.
    /// PBKDF2 success returns SuccessRehashNeeded so Identity upgrades the hash to bcrypt.
    /// </summary>
    public sealed class CompatibleBcryptHasher : IPasswordHasher<SwUser>
    {
        // If you want to standardize on a specific cost (you recently used 12):
        private const int WorkFactor = 12;

        private readonly PasswordHasher<SwUser> _pbkdf2 = new(); // Identity default PBKDF2

        public string HashPassword(SwUser user, string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);

        public PasswordVerificationResult VerifyHashedPassword(
            SwUser user,
            string hashedPassword,
            string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return PasswordVerificationResult.Failed;

            // Common bcrypt prefixes
            if (hashedPassword.StartsWith("$2a$")
             || hashedPassword.StartsWith("$2b$")
             || hashedPassword.StartsWith("$2y$"))
            {
                return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword)
                    ? PasswordVerificationResult.Success
                    : PasswordVerificationResult.Failed;
            }

            // Identity PBKDF2 (AQAAAA...) fallback
            if (hashedPassword.StartsWith("AQAAAA"))
            {
                var res = _pbkdf2.VerifyHashedPassword(user, hashedPassword, providedPassword);
                return res == PasswordVerificationResult.Success
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : res;
            }

            // Unknown format: last-resort PBKDF2 attempt
            var fallback = _pbkdf2.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return fallback == PasswordVerificationResult.Success
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;
        }
    }
}
