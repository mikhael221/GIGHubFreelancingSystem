using Microsoft.AspNetCore.Mvc;

namespace Freelancing.Controllers
{
    public class ManageProjectClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
