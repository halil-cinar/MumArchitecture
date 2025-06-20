using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Extensions;
using MumArchitecture.WebApp.Models;
using System.Diagnostics;

namespace MumArchitecture.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [EnableCaching(80)] 
        public IActionResult Index()
        {
            Debug.WriteLine("Index action called at: " + DateTime.Now);
            Thread.Sleep(5000);
            return View();
        }
        [EnableCaching] 
        public IActionResult Get()
        {
            Debug.WriteLine("Index action called at: " + DateTime.Now);
            Thread.Sleep(5000); 
            return Json(new {a="dddd"});
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
