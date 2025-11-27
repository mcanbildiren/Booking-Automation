using Microsoft.AspNetCore.Mvc;

namespace HairdresserAdmin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}

