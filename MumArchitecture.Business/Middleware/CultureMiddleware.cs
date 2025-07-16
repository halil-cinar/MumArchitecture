using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using MumArchitecture.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Middleware
{
    public class CultureMiddleware
    {
        private readonly RequestDelegate _next;

        public CultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.GetDisplayUrl().Contains("read", StringComparison.OrdinalIgnoreCase))
            {
                ;
            }
            var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
            var cultureCode = !string.IsNullOrEmpty(acceptLanguage) ? acceptLanguage.Split(',')[0] : AppSettings.instance!.DefaultCulture!;

            var culture = new CultureInfo(cultureCode);
            if (!AppSettings.instance!.LocalizationLangs!.ToLower()!.Split(",").Contains(culture.TwoLetterISOLanguageName.ToLower()))
            {
                culture = new CultureInfo(AppSettings.instance.DefaultCulture!);
            }
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Lang.LoadStrings();
            await _next(context);
        }
    }

}
