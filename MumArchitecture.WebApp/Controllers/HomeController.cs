using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.WebApp.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace MumArchitecture.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthenticationService _authenticationService;

        public HomeController(ILogger<HomeController> logger, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
        }

        //[EnableCaching(80)] 
        public async Task<IActionResult> Index()
        {
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
