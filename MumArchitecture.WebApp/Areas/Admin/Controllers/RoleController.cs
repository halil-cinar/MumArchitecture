using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;

namespace MumArchitecture.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Read(string format="grid")
        {
           
            var filter = this.QueryConvertFilter<Role>();
            filter.AddIncludes(x => x.Methods);
            var result = await _roleService.GetAll(filter);
            if (format == "select")
            {
                return result.ToPaggingResult<RoleListDto>(x=>x??new List<RoleListDto>()).ToSelectResult(x => x.Id.ToString(), x => x.Name??"");
            }
            return result.ToJsonResult();
        }
        public async Task<IActionResult> ReadMethod(string format="grid")
        {
           
            var result = await _roleService.GetAllMethods();
            if (format == "select")
            {
                return result.ToPaggingResult<MethodListDto>(x=>x??new List<MethodListDto>()).ToSelectResult(x => x.Id.ToString(), x => x.Name??"");
            }
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Get(int id)
        {
            var result = await _roleService.Get(id);
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Save(RoleDto dto)
        {
            var result = await _roleService.Save(dto);
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleService.Delete(id);
            return View("Index");
        }
    }
}
