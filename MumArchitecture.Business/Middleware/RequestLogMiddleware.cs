using Microsoft.AspNetCore.Http;
using MumArchitecture.Business.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Middleware
{
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var request = context.Request;
            var method = request.Method;
            var path = request.Path;
            var queryString = request.QueryString.ToString();
            var headers = string.Join(", ", request.Headers.Select(h => $"{h.Key}: {h.Value}"));
            var ip = (context.Connection.RemoteIpAddress?.ToString()+"/"+context.Connection.RemotePort.ToString()) ?? "Unknown IP";
            
            await _next(context);
            sw.Stop();

            var logText=@$"Time:{DateTime.UtcNow.ToString("G")} |Request: {method} {path}{queryString} |Request Time(seconds): {sw.Elapsed.TotalSeconds} |IP: {ip} | Headers: {headers} ";
            LogManager.LogInfo(logText);
        }
    }
}
