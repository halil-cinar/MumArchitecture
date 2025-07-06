using Microsoft.AspNetCore.Mvc;

namespace MumArchitecture.WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    public class MailBoxController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Detail()
        {
            return View();
        }
        public IActionResult Compose()
        {
            return View();
        }

    }
}
