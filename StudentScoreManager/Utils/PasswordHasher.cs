using System;
using BCrypt.Net;

namespace StudentScoreManager.Utils
{
    public static class PasswordHasher
    {
        private const int WorkFactor = 12;

        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                throw new ArgumentNullException(nameof(plainPassword),
                    "Password cannot be null or empty.");
            }

            try
            {
                return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to hash password.", ex);
            }
        }

        public static bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                throw new ArgumentNullException(nameof(plainPassword),
                    "Password cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                throw new ArgumentNullException(nameof(hashedPassword),
                    "Hashed password cannot be null or empty.");
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool NeedsRehash(string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return true;
            }

            try
            {
                return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor);
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
