using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Models;
using SafeVault.Services;

namespace SafeVault.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IUserService userService, ILogger<AdminController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["Error"] = "Error loading admin dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list");
                TempData["Error"] = "Error loading users list.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("toggle-user-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var result = await _userService.ToggleUserStatusAsync(id);
                if (result)
                {
                    TempData["Success"] = "User status updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update user status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status for user {UserId}", id);
                TempData["Error"] = "Error updating user status.";
            }

            return RedirectToAction("Users");
        }

        [HttpPost("update-user-role/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(int id, string role)
        {
            try
            {
                if (role != Roles.Admin && role != Roles.User)
                {
                    TempData["Error"] = "Invalid role specified.";
                    return RedirectToAction("Users");
                }

                var result = await _userService.UpdateUserRoleAsync(id, role);
                if (result)
                {
                    TempData["Success"] = "User role updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update user role.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user {UserId} to {Role}", id, role);
                TempData["Error"] = "Error updating user role.";
            }

            return RedirectToAction("Users");
        }
    }
}
