using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Models;

namespace SafeVault.Controllers
{
    [Authorize(Roles = Roles.User)]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("documents")]
        public IActionResult Documents()
        {
            // This would typically load user's documents from a service
            return View();
        }

        [HttpGet("upload")]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost("upload")]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(IFormFile file)
        {
            // Document upload logic would go here
            TempData["Success"] = "Document uploaded successfully!";
            return RedirectToAction("Documents");
        }

        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return View();
        }
    }
}
