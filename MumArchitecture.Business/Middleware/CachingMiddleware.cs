using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using MumArchitecture.Business.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Middleware
{
    public class CachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        public CachingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var cacheAttr = endpoint?.Metadata?.GetMetadata<EnableCachingAttribute>();
            if (cacheAttr == null)
            {
                await _next(context);
                return;
            }

            var cacheKey = GetCacheKey(context);
            if (_cache.TryGetValue(cacheKey, out string cached))
            {
                SetContentType(context, cached);
                await context.Response.WriteAsync(cached);
                return;
            }

            var originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            await _next(context);
            
            if (context.Response.StatusCode == StatusCodes.Status200OK)
            {
                buffer.Position = 0;
                var body = await new StreamReader(buffer).ReadToEndAsync();
                _cache.Set(cacheKey, body, TimeSpan.FromSeconds(cacheAttr.DurationSeconds));
                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody);
            }

            context.Response.Body = originalBody;
        }

        private static void SetContentType(HttpContext ctx, string body)
        {
            if (body.StartsWith("<")) ctx.Response.ContentType = "text/html";
            else if (body.TrimStart().StartsWith("{") || body.TrimStart().StartsWith("[")) ctx.Response.ContentType = "application/json";
            else ctx.Response.ContentType = "text/plain";
        }

        private static string GetCacheKey(HttpContext ctx)
        {
            var p = ctx.Request.Path.ToString().ToLowerInvariant();
            var q = ctx.Request.QueryString.ToString().ToLowerInvariant();
            var auth = ctx.Request.Cookies?["Authorization"]?.ToLowerInvariant();
            var ip= ctx.Connection.RemoteIpAddress?.ToString();
            var acceptlang = CultureInfo.DefaultThreadCurrentCulture?.TwoLetterISOLanguageName??"";//ctx.Request.Headers["Accept-Language"].ToString();
            return $"{acceptlang}{p}{q}{auth}{ip}";
        }
    }
}
