using System.Security.Cryptography;

namespace SafeVault.Services
{
    public interface IPasswordHashService
    {
        string GenerateSalt();
        string HashPassword(string password, string salt);
        bool VerifyPassword(string password, string salt, string hash);
    }

    public class PasswordHashService : IPasswordHashService
    {
        private const int SaltSize = 32;
        private const int BCryptWorkFactor = 12; // BCrypt work factor (2^12 = 4096 rounds)

        public string GenerateSalt()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var saltBytes = new byte[SaltSize];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }

        public string HashPassword(string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

            // Combine password with additional salt for maximum security
            var combinedInput = password + salt;
            
            // Use BCrypt with custom work factor for hashing
            // BCrypt includes its own salt, but we add our own for extra security
            var bcryptHash = BCrypt.Net.BCrypt.HashPassword(combinedInput, BCryptWorkFactor);
            
            return bcryptHash;
        }

        public bool VerifyPassword(string password, string salt, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                // Combine password with salt (same as during hashing)
                var combinedInput = password + salt;
                
                // Use BCrypt's built-in verification
                return BCrypt.Net.BCrypt.Verify(combinedInput, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}
