using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Domain;
using System.Text.Json;

namespace MumArchitecture.WebApp.Controllers
{
    public class MediaController : Controller
    {
        private readonly IMediaService _mediaService;
        private readonly IAuthenticationService _authenticationService;

        public MediaController(IMediaService mediaService, IAuthenticationService authenticationService)
        {
            _mediaService = mediaService;
            _authenticationService = authenticationService;
        }

        [HttpGet("/{lang}/localization.js")]
        public IActionResult Language(string lang)
        {
            var values= Lang.GetValues(lang);
            var dict= new Dictionary<string, string>();
            foreach (var item in values)
            {
                dict.Add(item.Key, item.Value??"");
            }
            var json = JsonSerializer.Serialize(dict);
            return Content($"var localization = {json};", "application/javascript; charset=utf-16");
        }

        public async Task<IActionResult> Index(string key)
        {
            var media = await _mediaService.Get("/Media?key=" + key);
            if (!media.IsSuccess || media.Data == null)
            {
                return NotFound();
            }
            return File(media.Data!.File!, media.Data!.ContentType!, media.Data.Name);
        }

        public async Task<IActionResult> Index(IFormFile file)
        {
            //todo: login durumu ontroll edilecek 
            var media = await _mediaService.Save(new Domain.Dtos.MediaDto
            {
                File = file,
                SavedUserId=_authenticationService.AuthUserId??0,
            });
            if (!media.IsSuccess || media.Data == null)
            {
                return NotFound();
            }
            return media.ToJsonResult();
        }


    }
}
