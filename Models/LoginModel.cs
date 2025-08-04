using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username or Email is required")]
        [StringLength(100, ErrorMessage = "Username/Email cannot exceed 100 characters")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
