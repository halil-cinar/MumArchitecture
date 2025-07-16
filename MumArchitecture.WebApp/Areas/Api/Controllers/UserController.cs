using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Web;
using System.Web.Http;

namespace MumArchitecture.WebApp.Areas.Api.Controllers
{
    [Area("Api")]
    [ApiController]
    [Route("[area]/v1/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> GetAll()
        {
            var filter = this.QueryConvertFilter<User>();
            var result = await _userService.GetAll(filter);
            return result.ToJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _userService.Get(id);
            return result.ToJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> Save(UserDto dto)
        {
            var result = await _userService.Save(dto);
            return result.ToJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.ChangeActive(id);
            return result.ToJsonResult();
        }
    }
}
