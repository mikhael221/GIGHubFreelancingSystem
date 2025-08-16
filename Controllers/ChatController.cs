using Microsoft.AspNetCore.Mvc;

namespace Freelancing.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
