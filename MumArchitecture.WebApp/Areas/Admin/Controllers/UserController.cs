using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;

namespace MumArchitecture.WebApp.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
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
    }
}
