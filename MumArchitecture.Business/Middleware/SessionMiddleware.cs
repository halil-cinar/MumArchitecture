using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuthenticationService _authenticationService;
        private static object createSessionLockKey= new object();
        private static object getSessionLockKey= new object();

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
            _authenticationService = AppSettings.instance!.serviceProvider!.GetRequiredService<IAuthenticationService>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = _authenticationService.AuthToken;
            var path = context.Request.Path;
            bool isStaticFile = Path.HasExtension(path);
            if (!isStaticFile)
            {
                if (string.IsNullOrEmpty(token))
                {
                    lock (createSessionLockKey)
                    {
                        var session =  _authenticationService.CreateSession().Result;
                        if (session.IsSuccess)
                        {
                            context.Response.Cookies.Append("Authorization", session.Data?.Token ?? "");
                        }
                    }
                }
                else
                {
                    lock (getSessionLockKey)
                    {
                        var session = _authenticationService.GetSession(token).Result;
                        if (session == null || session?.ExpiresAt < DateTime.UtcNow)
                        {
                            context.Response.Cookies.Delete("Authorization");
                            context.Response.Redirect("/");
                            return;
                        }
                        else
                        {
                            context.Items["UserSession"] = session;
                        }
                    }
                }
            }
            await _next(context);
        }
    }
}
