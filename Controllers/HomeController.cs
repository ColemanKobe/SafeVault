using Microsoft.AspNetCore.Mvc;

namespace SafeVault.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("/")]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // Redirect based on user role
                if (User.IsInRole(Models.Roles.Admin))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (User.IsInRole(Models.Roles.User))
                {
                    return RedirectToAction("Index", "User");
                }
            }
            
            return View();
        }

        [HttpGet]
        [Route("/Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [Route("/Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
