using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Domain;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Text.Encodings.Web;

namespace MumArchitecture.WebApp.Areas.Admin.Controllers
{
    public class LanguageController : Controller
    {
        public IActionResult Index()
        {
            var languages = AppSettings.instance?.LocalizationLangs?.Split(",").ToList();
               
            return View(languages);
        }

        [HttpGet]
        public IActionResult GetLanguageValues(string lang)
        {
            var values = Lang.GetValues(lang);
            return Json(values);
        }

        [HttpPost]
        public IActionResult SaveLanguageValues([FromBody] List<LangValue> values, string lang)
        {
            if (values == null || !values.Any())
                return BadRequest();

            var path = Path.Combine(AppContext.BaseDirectory, "Resources", $"lang-{lang}.json");
            System.Text.Json.JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder= JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(values, options);
            System.IO.File.WriteAllText(path, json, System.Text.Encoding.Unicode);
            
            Lang.LoadStrings(); // Reload the language values
            
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> TranslateToLanguage(string lang)
        {
            if (lang == "tr")
                return BadRequest(Lang.Value("Cannot translate to Turkish as it's the source language"));

            try
            {
                await Lang.TranslateAllValuesToLanguage(lang);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public static DateTime GetLocalDatetime(DateTime date, string? lang = null)
        {
            // Implementation of GetLocalDatetime method
            // This is a placeholder and should be implemented based on your requirements
            return date;
        }
    }
}