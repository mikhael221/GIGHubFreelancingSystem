using Microsoft.AspNetCore.Mvc;

namespace Freelancing.Controllers
{
    public class ManageProjectFreelancerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
