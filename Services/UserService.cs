using Microsoft.EntityFrameworkCore;
using SafeVault.Data;
using SafeVault.Models;

namespace SafeVault.Services
{
    public interface IUserService
    {
        Task<bool> IsUsernameUniqueAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<User> CreateUserAsync(string username, string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> ToggleUserStatusAsync(int userId);
        Task<bool> UpdateUserRoleAsync(int userId, string role);
    }

    public class UserService : IUserService
    {
        private readonly SafeVaultDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(SafeVaultDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            try
            {
                return !await _context.Users.AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username uniqueness for {Username}", username);
                throw;
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            try
            {
                return !await _context.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email uniqueness for {Email}", email);
                throw;
            }
        }

        public async Task<User> CreateUserAsync(string username, string email)
        {
            try
            {
                // Double-check uniqueness
                var isUsernameUnique = await IsUsernameUniqueAsync(username);
                var isEmailUnique = await IsEmailUniqueAsync(email);

                if (!isUsernameUnique)
                {
                    throw new InvalidOperationException($"Username '{username}' is already taken.");
                }

                if (!isEmailUnique)
                {
                    throw new InvalidOperationException($"Email '{email}' is already registered.");
                }

                var user = new User
                {
                    Username = username,
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User created successfully: {Username} ({Email})", username, email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username} ({Email})", username, email);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID {UserId}", id);
                throw;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username {Username}", username);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users
                    .OrderBy(u => u.Username)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                throw;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} status toggled to {Status}", userId, user.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string role)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                user.Role = role;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} role updated to {Role}", userId, role);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user {UserId}", userId);
                return false;
            }
        }
    }
}
