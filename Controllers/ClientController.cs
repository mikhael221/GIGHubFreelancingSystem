using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freelancing.Controllers
{
    public class ClientController : Controller
    {
        private readonly ILogger<ClientController> _logger;

        public ClientController(ILogger<ClientController> logger)
        {
            _logger = logger;
        }
        public IActionResult ClientLogin()
        {
            return View();
        }

        public IActionResult ClientRegister()
        {
            return View();
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }
        public IActionResult Feed()
        {
            return View();
        }
    }
}
