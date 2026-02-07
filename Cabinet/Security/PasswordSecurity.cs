using System.Security.Cryptography;
using System.Text;

namespace Cabinet.Security
{
    public static class PasswordSecurity
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;
        private const string Prefix = "PBKDF2";

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }

            Span<byte> salt = stackalloc byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string storedValue, string providedPassword, out bool needsRehash)
        {
            needsRehash = false;

            if (string.IsNullOrWhiteSpace(storedValue) || string.IsNullOrEmpty(providedPassword))
            {
                return false;
            }

            var parts = storedValue.Split('$');
            if (parts.Length == 4 && string.Equals(parts[0], Prefix, StringComparison.Ordinal))
            {
                if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
                {
                    return false;
                }

                byte[] salt;
                byte[] expectedHash;
                try
                {
                    salt = Convert.FromBase64String(parts[2]);
                    expectedHash = Convert.FromBase64String(parts[3]);
                }
                catch (FormatException)
                {
                    return false;
                }

                var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(providedPassword),
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(expectedHash, computedHash);
            }

            // Legacy plain-text compatibility path. If it matches, caller should rehash.
            if (string.Equals(storedValue, providedPassword, StringComparison.Ordinal))
            {
                needsRehash = true;
                return true;
            }

            return false;
        }
    }
}
