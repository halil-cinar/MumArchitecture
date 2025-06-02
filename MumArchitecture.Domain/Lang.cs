using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using GTranslatorAPI;
using Microsoft.AspNetCore.Hosting;
using NodaTime;

namespace MumArchitecture.Domain
{
    public class Lang
    {
        private static object _langvaluesLock=new object();
        private static CultureInfo? culture = null;
        private static Dictionary<string, List<LangValue>> langValues = new Dictionary<string, List<LangValue>>();
        public static string Value(string key, string? lang = null)
        {
            if (string.IsNullOrEmpty(lang))
            {
                culture = Thread.CurrentThread.CurrentUICulture ?? CultureInfo.GetCultureInfo("en");
                lang = culture.Name.Split("-")[0];
            }
            else
            {
                culture = CultureInfo.GetCultureInfo(lang) ?? CultureInfo.GetCultureInfo("en");
            }

            if (langValues.Count == 0)
            {
                LoadStrings();
            }

            lock (_langvaluesLock)
            {
                var value = langValues.GetValueOrDefault(lang)?.FirstOrDefault(x => x.Key == key);
                // ?? langValues[lang]?.FirstOrDefault(x => x.Key == culture.Name);
                if (value == null)
                {
                    var a = new Lang();
                    a.SaveTranslateLangText("en", lang, key, key).Wait();
                    LoadStrings();
                    value = langValues[lang]?.FirstOrDefault(x => x.Key == key)
                    ?? langValues[lang]?.FirstOrDefault(x => x.Key == culture.Name);
                }

                return value?.Value?.ToString() ?? key;
            }
        }
        public static List<LangValue> GetValues(string? lang = null)
        {
            if (string.IsNullOrEmpty(lang))
            {
                culture = Thread.CurrentThread.CurrentUICulture ?? CultureInfo.GetCultureInfo("en");
                lang = culture.Name.Split("-")[0];
            }
            else
            {
                culture = CultureInfo.GetCultureInfo(lang) ?? CultureInfo.GetCultureInfo("en");
            }

            if (langValues.Count == 0)
            {
                LoadStrings();
            }


            var values = langValues.GetValueOrDefault(lang);
         
            return values ?? new List<LangValue>();
        }

        public static void LoadStrings()
        {
            lock (_langvaluesLock)
            {
                langValues.Clear();
                //var path = Path.Combine(AppContext.BaseDirectory, "../", "../", "../", "../", "SocialNetwork.Core", "Localization", "Resources");
                var path = Path.Combine(AppContext.BaseDirectory, "Resources");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var languageFiles = Directory.GetFiles(path, "lang-*.json");
                foreach (var languageFile in languageFiles)
                {
                    langValues.Add(Path.GetFileNameWithoutExtension(languageFile)[5..], GetLangValues(languageFile));
                }
            }
        }

        private static List<LangValue> GetLangValues(string path)
        {
            var all = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<List<LangValue>>(all);
            if (data != null)
            {
                return data;
            }
            return new List<LangValue>();
        }


        public async Task<string> SaveTranslateLangText(string lang, string tolang, string key, string value)
        {
            var text = await GetTranslate(Enum.Parse<Languages>(lang), Enum.Parse<Languages>(tolang), value);
            if (string.IsNullOrEmpty(text))
            {
                return value;
            }
            // var path = Path.Combine(AppContext.BaseDirectory, "../", "../", "../", "../", "SocialNetwork.Core", "Localization", "Resources", "lang-" + tolang + ".json");
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", "lang-" + tolang + ".json");
            //if (!File.Exists(path))
            //{
            //    var str = "[]";
            //    var file = File.Create(path);
            //    file.Write(str.ToCharArray().Select(x => (byte)x).ToArray(), 0, str.Length);
            //    file.Close();
            //}
            if (!File.Exists(path))
            {
                var str = "[]";
                using (var file = new StreamWriter(path, false, Encoding.Unicode))
                {
                    file.Write(str);
                }
            }
            var values = JsonSerializer.Deserialize<List<LangValue>>(File.ReadAllText(path)) ?? new List<LangValue>();
            values.Add(new LangValue { Key = value, Value = text });
            System.Text.Json.JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllText(path, JsonSerializer.Serialize(values,options), Encoding.Unicode);
            return text;
        }


        public async Task<string> GetTranslate(Languages lang, Languages toLang, string value)
        {
            try
            {
                var translator = new GTranslatorAPIClient();
                var result = await translator.TranslateAsync(lang, toLang, value);
                return result.TranslatedText;
            }
            catch
            {
                return "";
            }

        }

        public static DateTime GetLocalDatetime(DateTime date, string? lang = null)
        {
            if (string.IsNullOrEmpty(lang))
            {
                culture = Thread.CurrentThread.CurrentUICulture ?? CultureInfo.GetCultureInfo("en");
                lang = culture.Name;
            }
            else
            {
                culture = CultureInfo.GetCultureInfo(lang) ?? CultureInfo.GetCultureInfo("en");
            }

            return date.Add(GetTimeDifferenceByCulture(culture.Name, DateTime.Now));

        }

        private static TimeSpan GetTimeDifferenceByCulture(string cultureCode, DateTime regionalDateTime)
        {
            var timeZoneProvider = DateTimeZoneProviders.Tzdb;
            DateTimeZone timeZone = timeZoneProvider.GetZoneOrNull(cultureCode) ?? DateTimeZone.Utc;

            var instant = Instant.FromDateTimeUtc(regionalDateTime.ToUniversalTime());
            var zonedDateTime = new ZonedDateTime(instant, timeZone);
            var offset = zonedDateTime.Offset;

            return offset.ToTimeSpan();
        }

        public static async Task TranslateAllValuesToLanguage(string targetLang)
        {
            // Clear existing values for target language
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", $"lang-{targetLang}.json");
            if (File.Exists(path))
            {
                File.WriteAllText(path, "[]", Encoding.Unicode);
            }

            var turkishValues = GetValues("tr");
            var lang = new Lang();
            
            foreach (var value in turkishValues)
            {
                var translatedText = await lang.GetTranslate(Languages.tr, Enum.Parse<Languages>(targetLang), value.Value);
                if (!string.IsNullOrEmpty(translatedText))
                {
                    await lang.SaveTranslateLangText("tr", targetLang, value.Key, value.Value);
                }
            }
            
            LoadStrings(); // Reload all language values after translation
        }

    }

    public class LangValue
    {
        public string Key { get; set; }
        public string? Value { get; set; }
    }

}
