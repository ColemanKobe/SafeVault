using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SafeVault.Models;
using SafeVault.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.RegularExpressions;

namespace SafeVault.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    return RedirectToAction("Index", "Home");
                }

                // Enhanced input validation and sanitization
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Additional security validation
                if (!IsValidInput(model.Username) || !IsValidInput(model.Email))
                {
                    ModelState.AddModelError("", "Invalid input detected.");
                    return View(model);
                }

                // Sanitize inputs
                model.Username = SanitizeInput(model.Username);
                model.Email = SanitizeInput(model.Email);

                var user = await _authService.RegisterUserAsync(model);
                
                if (user != null)
                {
                    // Create claims for the registered user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("UserId", user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    _logger.LogInformation("User registered and logged in successfully");
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError("", "Registration failed.");
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                // Handle specific registration errors without exposing sensitive details
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                // Log detailed error but show generic message to user
                _logger.LogError(ex, "Unexpected error during registration");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        {
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    return RedirectToAction("Index", "Home");
                }

                ViewData["ReturnUrl"] = returnUrl;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Enhanced input validation
                if (!IsValidInput(model.UsernameOrEmail))
                {
                    ModelState.AddModelError("", "Invalid input detected.");
                    return View(model);
                }

                // Sanitize input
                model.UsernameOrEmail = SanitizeInput(model.UsernameOrEmail);

                var user = await _authService.LoginAsync(model);
                
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("UserId", user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(24)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    _logger.LogInformation("User logged in successfully");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError("", "Invalid username/email or password.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Logout()
        {
            // GET request for logout - show confirmation or redirect to home
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? confirm)
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User logged out successfully");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Enhanced input validation to prevent injection attacks
        /// </summary>
        private bool IsValidInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Check for length limits
            if (input.Length > 255)
                return false;

            // Check for potentially malicious patterns
            var maliciousPatterns = new[]
            {
                @"<script[^>]*>.*?</script>", // Script tags
                @"javascript:", // JavaScript protocol
                @"on\w+\s*=", // Event handlers
                @"eval\s*\(", // Eval function
                @"expression\s*\(", // CSS expressions
                @"vbscript:", // VBScript protocol
                @"data:text/html", // Data URI with HTML
                @"<iframe[^>]*>.*?</iframe>", // Iframe tags
                @"<object[^>]*>.*?</object>", // Object tags
                @"<embed[^>]*>.*?</embed>", // Embed tags
                @"<link[^>]*>", // Link tags
                @"<meta[^>]*>", // Meta tags
                @"<style[^>]*>.*?</style>", // Style tags
                @"--", // SQL comment
                @"/\*", // SQL comment start
                @"\*/", // SQL comment end
                @";\s*(drop|delete|insert|update|create|alter|exec|execute)", // SQL commands
                @"union\s+select", // SQL union
                @"1\s*=\s*1", // SQL injection pattern
                @"'\s*or\s*'", // SQL injection pattern
                @"\|\|", // SQL concatenation
                @"&&", // Logical AND
                @"\.\./" // Directory traversal
            };

            foreach (var pattern in maliciousPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Enhanced input sanitization with length limits
        /// </summary>
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Trim and limit length
            input = input.Trim();
            if (input.Length > 255)
                input = input.Substring(0, 255);

            // Remove potentially dangerous characters
            input = input.Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("'", "&#x27;")
                        .Replace("&", "&amp;")
                        .Replace("/", "&#x2F;")
                        .Replace("\\", "&#x5C;")
                        .Replace("`", "&#x60;");

            // Remove null characters and other control characters
            input = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

            return input;
        }
    }
}
