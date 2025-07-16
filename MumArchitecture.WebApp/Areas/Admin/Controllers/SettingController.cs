using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using NToastNotify;

namespace MumArchitecture.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly IToastNotification _toastNotification;

        public SettingController(ISettingService settingService, IToastNotification toastNotification)
        {
            _settingService = settingService;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _settingService.Get(id);
            if (result.IsSuccess && result.Data != null)
            {
                return View(result.Data);
            }
            foreach (var message in result.GetMessages())
            {
                _toastNotification.AddErrorToastMessage(message);
            }
            return View();
        }

        public async Task<IActionResult> Read(string format = "grid")
        {
            var filter = this.QueryConvertFilter<Setting>();
            var result = await _settingService.GetAll(filter);
            if (format == "select")
            {
                return result.ToSelectResult(x => x.Id.ToString(), x => x.Name??"");
            }
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Get(int id)
        {
            var result = await _settingService.Get(id);
            return result.ToJsonResult();
        }

        public async Task<IActionResult> Save(SettingDto dto)
        {
            var result = await _settingService.Save(dto);
            return result.ToJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> Save(int id,string value)
        {
            var getresult = await _settingService.Get(id);
            var setting =(((Setting) getresult.Data ))?? new SettingDto { Id = id, Value = value };
            setting.Value = value;
            var result = await _settingService.Save(setting);
            return result.ToJsonResult();
        }

        
    }
}
