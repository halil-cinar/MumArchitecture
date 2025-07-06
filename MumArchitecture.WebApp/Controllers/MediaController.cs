using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Domain;
using System.Text.Json;

namespace MumArchitecture.WebApp.Controllers
{
    public class MediaController : Controller
    {
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
    }
}
