using Microsoft.AspNetCore.Http;
using MumArchitecture.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Middleware
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string AllowedOrigin;

        public SecurityMiddleware(RequestDelegate next)
        {
            _next = next;
            AllowedOrigin = AppSettings.instance?.AllowedOrigin ?? "";
        }


        public async Task InvokeAsync(HttpContext context)
        {
            // User-Agent kontrolü
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(userAgent) || userAgent.Contains("Postman") || userAgent.Contains("curl"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("User-Agent engellendi.");
                return;
            }

            // Origin/Referer kontrolü
            var origin = context.Request.Headers["Origin"].ToString();
            var referer = context.Request.Headers["Referer"].ToString();
            if ((!string.IsNullOrEmpty(origin) && !origin.StartsWith(AllowedOrigin)) ||
                (!string.IsNullOrEmpty(referer) && !referer.StartsWith(AllowedOrigin)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Geçersiz kaynak (origin/referer).");
                return; 
            }
            await _next(context);
        }
    }
}
