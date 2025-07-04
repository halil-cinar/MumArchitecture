using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;

namespace MumArchitecture.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IExcelDataService _excelDataService;

        public MenuController(IMenuService menuService, IExcelDataService excelDataService)
        {
            _menuService = menuService;
            _excelDataService = excelDataService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Read(string format = "grid")
        {

            var filter = this.QueryConvertFilter<Menu>();
            filter.AddIncludes(x => x.Parent);
            var result = await _menuService.GetAll(filter);
            if (format == "select")
            {
                return result.ToSelectResult(x => x.Id.ToString(), x => x.Name);
            }
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Get(int id)
        {
            var result = await _menuService.Get(id);
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Save(MenuDto dto)
        {
            var result = await _menuService.Save(dto);
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _menuService.ChangeActive(id);
            return View("Index");
        }
        public async Task<IActionResult> DownloadExcel()
        {
            var result = await _excelDataService.DownloadExcel<Menu, MenuListDto>(Filter<Menu>.CreateFilter(), x => x, false);
            return File(result.Data.File, result.Data.ContentType, result.Data.Name);
        }

    }
}
