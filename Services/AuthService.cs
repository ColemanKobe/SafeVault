using SafeVault.Models;
using SafeVault.Data;
using Microsoft.EntityFrameworkCore;

namespace SafeVault.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterUserAsync(RegisterModel model);
        Task<User?> LoginAsync(LoginModel model);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<bool> IsEmailAvailableAsync(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly SafeVaultDbContext _context;
        private readonly IPasswordHashService _passwordHashService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(SafeVaultDbContext context, IPasswordHashService passwordHashService, ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHashService = passwordHashService;
            _logger = logger;
        }

        public async Task<User?> RegisterUserAsync(RegisterModel model)
        {
            try
            {
                // Check if username or email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

                if (existingUser != null)
                {
                    if (existingUser.Username == model.Username)
                        throw new InvalidOperationException("Username is already taken");
                    if (existingUser.Email == model.Email)
                        throw new InvalidOperationException("Email is already registered");
                }

                // Generate salt and hash password
                var salt = _passwordHashService.GenerateSalt();
                var passwordHash = _passwordHashService.HashPassword(model.Password, salt);

                // Create new user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Role = "User", // Always default to User role for security
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {Username} ({Email})", user.Username, user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Username} ({Email})", model.Username, model.Email);
                throw;
            }
        }

        public async Task<User?> LoginAsync(LoginModel model)
        {
            try
            {
                // Find user by username or email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => 
                        (u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail) 
                        && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent user: {UsernameOrEmail}", model.UsernameOrEmail);
                    return null;
                }

                // Verify password
                if (!_passwordHashService.VerifyPassword(model.Password, user.Salt, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user: {Username}", user.Username);
                    return null;
                }

                _logger.LogInformation("Successful login for user: {Username}", user.Username);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for: {UsernameOrEmail}", model.UsernameOrEmail);
                return null;
            }
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
