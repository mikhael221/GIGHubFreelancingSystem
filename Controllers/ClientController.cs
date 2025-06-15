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
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }
        [Authorize]
        public IActionResult Feed()
        {
            return View();
        }
    }
}
