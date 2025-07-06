using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MumArchitecture.Business.Extensions
{

    public static class ObjectExtensions
    {
        public static Dictionary<string, object?> ToDictionary<T>(this T source, bool includeNulls = true)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            return source.GetType()
                         .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.CanRead)
                         .Select(p => new { p.Name, Value = p.GetValue(source) })
                         .Where(x => includeNulls || x.Value is not null)
                         .ToDictionary(x => x.Name, x => x.Value);
        }

        public static string ToLocalizationString(this DateTime source, string format, string? language = null)
        {
            var cultureInfo = string.IsNullOrEmpty(language) ? CultureInfo.CurrentCulture : new CultureInfo(language);
            var timeZone = GetTimeZoneForCulture(cultureInfo);
            var localized = TimeZoneInfo.ConvertTimeFromUtc(source.ToUniversalTime(), timeZone);
            return localized.ToString(format, cultureInfo);
        }

        private static TimeZoneInfo GetTimeZoneForCulture(CultureInfo culture)
        {
            if (CultureTimeZones.TryGetValue(culture.Name, out var tzId))
                return TimeZoneInfo.FindSystemTimeZoneById(tzId);

            return TimeZoneInfo.Local;
        }

        public static T ToObject<T>(this IDictionary<string, object?> source) where T : new()
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            var obj = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanWrite);

            foreach (var prop in props)
            {
                if (!source.TryGetValue(prop.Name, out var value) || value is null) continue;

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var converted = ConvertValue(value, targetType);
                prop.SetValue(obj, converted, null);
            }

            return obj;
        }

        private static object? ConvertValue(object value, Type targetType)
        {
            if (value is string str && string.IsNullOrWhiteSpace(str))
            {
                if (!IsNullable(targetType) && targetType == typeof(string))
                {
                    return "";
                }
                return null;
            }

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = targetType.GenericTypeArguments[0];
                IEnumerable<object> items;

                if (value is string s)
                {
                    var trimmed = s.Trim();
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(trimmed);
                        items = json.ValueKind == System.Text.Json.JsonValueKind.Array
                            ? json.EnumerateArray().Select(e => (object)(e.ValueKind == System.Text.Json.JsonValueKind.String ? e.GetString()! : e.ToString()))
                            : Array.Empty<object>();
                    }
                    else
                    {
                        items = trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => (object)x.Trim());
                    }
                }
                else if (value is System.Collections.IEnumerable enumerable && value is not string)
                {
                    items = enumerable.Cast<object>();
                }
                else
                {
                    items = new[] { value };
                }

                var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
                foreach (var item in items)
                {
                    list.Add(ConvertValue(item, elementType));
                }
                return list;
            }

            if (targetType.IsEnum) return Enum.Parse(targetType, value.ToString()!, true);
            if (targetType == typeof(Guid)) return Guid.Parse(value.ToString()!);
            if (targetType == typeof(DateTimeOffset)) return DateTimeOffset.Parse(value.ToString()!);
            if (value is IConvertible) return Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
            if (targetType.IsAssignableFrom(value.GetType())) return value;
            return null;
        }
        private static bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }


        private static readonly Dictionary<string, string> CultureTimeZones = new(StringComparer.OrdinalIgnoreCase)
{
    { "af-NA", "Namibia Standard Time" },
    { "af-ZA", "South Africa Standard Time" },
    { "am-ET", "E. Africa Standard Time" },
    { "ar-AE", "Arabian Standard Time" },
    { "ar-BH", "Arab Standard Time" },
    { "ar-DZ", "W. Central Africa Standard Time" },
    { "ar-EG", "Egypt Standard Time" },
    { "ar-IQ", "Arab Standard Time" },
    { "ar-JO", "Jordan Standard Time" },
    { "ar-KM", "E. Africa Standard Time" },
    { "ar-KW", "Arab Standard Time" },
    { "ar-LB", "Middle East Standard Time" },
    { "ar-LY", "Libya Standard Time" },
    { "ar-MA", "Morocco Standard Time" },
    { "ar-MR", "Greenwich Standard Time" },
    { "ar-OM", "Arabian Standard Time" },
    { "ar-QA", "Arab Standard Time" },
    { "ar-SA", "Arab Standard Time" },
    { "ar-SD", "E. Africa Standard Time" },
    { "ar-SY", "Syria Standard Time" },
    { "ar-TN", "W. Central Africa Standard Time" },
    { "ar-YE", "Arabian Standard Time" },
    { "az-Latn-AZ", "Azerbaijan Standard Time" },
    { "be-BY", "Belarus Standard Time" },
    { "bg-BG", "FLE Standard Time" },
    { "bn-BD", "Bangladesh Standard Time" },
    { "bs-BA", "Central Europe Standard Time" },
    { "ca-AD", "W. Europe Standard Time" },
    { "ca-ES", "Romance Standard Time" },
    { "cs-CZ", "Central Europe Standard Time" },
    { "da-DK", "Romance Standard Time" },
    { "de-AT", "W. Europe Standard Time" },
    { "de-CH", "W. Europe Standard Time" },
    { "de-DE", "W. Europe Standard Time" },
    { "de-LU", "W. Europe Standard Time" },
    { "dv-MV", "West Asia Standard Time" },
    { "dz-BT", "Bangladesh Standard Time" },
    { "el-CY", "GTB Standard Time" },
    { "el-GR", "GTB Standard Time" },
    { "en-AG", "SA Western Standard Time" },
    { "en-AU", "AUS Eastern Standard Time" },
    { "en-BB", "SA Western Standard Time" },
    { "en-BS", "Eastern Standard Time" },
    { "en-BW", "South Africa Standard Time" },
    { "en-BZ", "Central America Standard Time" },
    { "en-CA", "Eastern Standard Time" },
    { "en-DM", "SA Western Standard Time" },
    { "en-FJ", "Fiji Standard Time" },
    { "en-FM", "West Pacific Standard Time" },
    { "en-GD", "SA Western Standard Time" },
    { "en-GH", "Greenwich Standard Time" },
    { "en-GM", "Greenwich Standard Time" },
    { "en-GY", "SA Western Standard Time" },
    { "en-IE", "GMT Standard Time" },
    { "en-IN", "India Standard Time" },
    { "en-JM", "Eastern Standard Time" },
    { "en-KI", "Line Islands Standard Time" },
    { "en-KN", "SA Western Standard Time" },
    { "en-LC", "SA Western Standard Time" },
    { "en-MH", "UTC+12" },
    { "en-MU", "Mauritius Standard Time" },
    { "en-MX", "Central Standard Time (Mexico)" },
    { "en-NG", "W. Central Africa Standard Time" },
    { "en-NR", "UTC+12" },
    { "en-NZ", "New Zealand Standard Time" },
    { "en-PG", "West Pacific Standard Time" },
    { "en-PH", "Singapore Standard Time" },
    { "en-PW", "Tokyo Standard Time" },
    { "en-SB", "Solomon Standard Time" },
    { "en-SC", "Mauritius Standard Time" },
    { "en-SG", "Singapore Standard Time" },
    { "en-SL", "Greenwich Standard Time" },
    { "en-SZ", "South Africa Standard Time" },
    { "en-TT", "SA Western Standard Time" },
    { "en-TV", "UTC+12" },
    { "en-TZ", "E. Africa Standard Time" },
    { "en-UG", "E. Africa Standard Time" },
    { "en-US", "Eastern Standard Time" },
    { "en-VC", "SA Western Standard Time" },
    { "en-VU", "Central Pacific Standard Time" },
    { "en-WS", "Samoa Standard Time" },
    { "en-ZA", "South Africa Standard Time" },
    { "en-ZM", "South Africa Standard Time" },
    { "en-ZW", "South Africa Standard Time" },
    { "es-AR", "Argentina Standard Time" },
    { "es-BO", "SA Western Standard Time" },
    { "es-CL", "Pacific SA Standard Time" },
    { "es-CO", "SA Pacific Standard Time" },
    { "es-CR", "Central America Standard Time" },
    { "es-CU", "Cuba Standard Time" },
    { "es-DO", "SA Western Standard Time" },
    { "es-EC", "SA Pacific Standard Time" },
    { "es-ES", "Central Europe Standard Time" },
    { "es-GQ", "W. Central Africa Standard Time" },
    { "es-GT", "Central America Standard Time" },
    { "es-HN", "Central America Standard Time" },
    { "es-MX", "Central Standard Time (Mexico)" },
    { "es-NI", "Central America Standard Time" },
    { "es-PA", "SA Pacific Standard Time" },
    { "es-PE", "SA Pacific Standard Time" },
    { "es-PY", "Paraguay Standard Time" },
    { "es-SV", "Central America Standard Time" },
    { "es-UY", "Montevideo Standard Time" },
    { "es-VE", "Venezuela Standard Time" },
    { "et-EE", "FLE Standard Time" },
    { "fa-IR", "Iran Standard Time" },
    { "fi-FI", "FLE Standard Time" },
    { "fr-BE", "Romance Standard Time" },
    { "fr-BF", "Greenwich Standard Time" },
    { "fr-BI", "South Africa Standard Time" },
    { "fr-BJ", "W. Central Africa Standard Time" },
    { "fr-CF", "W. Central Africa Standard Time" },
    { "fr-CG", "W. Central Africa Standard Time" },
    { "fr-CH", "W. Europe Standard Time" },
    { "fr-CI", "Greenwich Standard Time" },
    { "fr-CM", "W. Central Africa Standard Time" },
    { "fr-DJ", "E. Africa Standard Time" },
    { "fr-FR", "Romance Standard Time" },
    { "fr-GA", "W. Central Africa Standard Time" },
    { "fr-GN", "Greenwich Standard Time" },
    { "fr-HT", "Eastern Standard Time" },
    { "fr-LU", "W. Europe Standard Time" },
    { "fr-MC", "W. Europe Standard Time" },
    { "fr-MG", "E. Africa Standard Time" },
    { "fr-ML", "Greenwich Standard Time" },
    { "fr-NE", "W. Central Africa Standard Time" },
    { "fr-RW", "South Africa Standard Time" },
    { "fr-SC", "Mauritius Standard Time" },
    { "fr-SN", "Greenwich Standard Time" },
    { "fr-TD", "W. Central Africa Standard Time" },
    { "fr-TG", "Greenwich Standard Time" },
    { "he-IL", "Israel Standard Time" },
    { "hi-IN", "India Standard Time" },
    { "hr-HR", "Central Europe Standard Time" },
    { "hu-HU", "Central Europe Standard Time" },
    { "hy-AM", "Armenian Standard Time" },
    { "id-ID", "SE Asia Standard Time" },
    { "is-IS", "Greenwich Standard Time" },
    { "it-IT", "W. Europe Standard Time" },
    { "it-SM", "W. Europe Standard Time" },
    { "it-VA", "W. Europe Standard Time" },
    { "ja-JP", "Tokyo Standard Time" },
    { "ka-GE", "Georgian Standard Time" },
    { "kk-KZ", "Central Asia Standard Time" },
    { "km-KH", "SE Asia Standard Time" },
    { "ko-KP", "North Korea Standard Time" },
    { "ko-KR", "Korea Standard Time" },
    { "ky-KG", "Kyrgyzstan Standard Time" },
    { "lo-LA", "SE Asia Standard Time" },
    { "lt-LT", "FLE Standard Time" },
    { "lv-LV", "FLE Standard Time" },
    { "mk-MK", "Central Europe Standard Time" },
    { "mn-MN", "Ulaanbaatar Standard Time" },
    { "ms-BN", "Singapore Standard Time" },
    { "ms-MY", "Singapore Standard Time" },
    { "mt-MT", "W. Europe Standard Time" },
    { "my-MM", "Myanmar Standard Time" },
    { "nb-NO", "Romance Standard Time" },
    { "ne-NP", "Nepal Standard Time" },
    { "nl-NL", "W. Europe Standard Time" },
    { "nl-SR", "SA Eastern Standard Time" },
    { "pa-IN", "India Standard Time" },
    { "pl-PL", "Central Europe Standard Time" },
    { "pt-AO", "W. Central Africa Standard Time" },
    { "pt-BR", "E. South America Standard Time" },
    { "pt-CV", "Cape Verde Standard Time" },
    { "pt-GW", "Greenwich Standard Time" },
    { "pt-MZ", "South Africa Standard Time" },
    { "pt-PT", "GMT Standard Time" },
    { "pt-ST", "Greenwich Standard Time" },
    { "pt-TL", "Tokyo Standard Time" },
    { "ro-MD", "GTB Standard Time" },
    { "ro-RO", "GTB Standard Time" },
    { "ru-RU", "Russian Standard Time" },
    { "rw-RW", "South Africa Standard Time" },
    { "si-LK", "Sri Lanka Standard Time" },
    { "sk-SK", "Central Europe Standard Time" },
    { "sl-SI", "Central Europe Standard Time" },
    { "so-SO", "E. Africa Standard Time" },
    { "sq-AL", "Central Europe Standard Time" },
    { "sr-ME", "Central Europe Standard Time" },
    { "sr-RS", "Central Europe Standard Time" },
    { "sv-SE", "W. Europe Standard Time" },
    { "sw-KE", "E. Africa Standard Time" },
    { "sw-TZ", "E. Africa Standard Time" },
    { "ta-IN", "India Standard Time" },
    { "ta-LK", "Sri Lanka Standard Time" },
    { "te-IN", "India Standard Time" },
    { "tg-TJ", "West Asia Standard Time" },
    { "th-TH", "SE Asia Standard Time" },
    { "tk-TM", "Turkmenistan Standard Time" },
    { "tr-TR", "Turkey Standard Time" },
    { "uk-UA", "FLE Standard Time" },
    { "ur-PK", "Pakistan Standard Time" },
    { "uz-UZ", "West Asia Standard Time" },
    { "vi-VN", "SE Asia Standard Time" },
    { "zh-CN", "China Standard Time" },
    { "zh-HK", "Hong Kong Standard Time" },
    { "zh-MO", "China Standard Time" },
    { "zh-SG", "Singapore Standard Time" },
    { "zh-TW", "Taipei Standard Time" },
    { "zu-ZA", "South Africa Standard Time" }
};
    }
}
