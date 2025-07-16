using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Web;

namespace MumArchitecture.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Read()
        {
            var filter = this.QueryConvertFilter<User>();
            var result = await _userService.GetAll(filter);
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Get(int id)
        {
            var result = await _userService.Get(id);
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Save(UserDto dto)
        {
            var result = await _userService.Save(dto);
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.ChangeActive(id);
            return View("Index");
        }

        // Controller or minimal API endpoint
        [HttpGet("api/reports/random")]
        public JsonResult RandomChartData()
        {
            var rnd = new Random();

            var payload = new ChartPayload
            {
                Labels = Enumerable.Range(1, 12).Select(i => $"Ay {i}").ToList(),
                Datasets = new[]
                {
            new ChartDataset
            {
                Label = "Seri A",
                Data = Enumerable.Range(0, 12).Select(_ => (double)rnd.Next(20, 100)).ToList()
            },
            new ChartDataset
            {
                Label = "Seri B",
                Data = Enumerable.Range(0, 12).Select(_ => (double) rnd.Next(20, 100)).ToList()
            },
            new ChartDataset
            {
                Label = "Seri c",
                Data = Enumerable.Range(0, 12).Select(_ => (double) rnd.Next(20, 100)).ToList()
            },
            new ChartDataset
            {
                Label = "Seri d",
                Data = Enumerable.Range(0, 12).Select(_ => (double) rnd.Next(20, 100)).ToList()
            }
        }
            };

            return new ChartResult { Data = payload }.ToJsonResult();
        }

    }
}
