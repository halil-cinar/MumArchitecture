using Microsoft.AspNetCore.Http;
using MumArchitecture.Business.Abstract;
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

        public SessionMiddleware(RequestDelegate next, IAuthenticationService authenticationService)
        {
            _next = next;
            _authenticationService = authenticationService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = _authenticationService.AuthToken;
            if (string.IsNullOrEmpty(token))
            {
                var session = await _authenticationService.CreateSession();
                if (session.IsSuccess)
                {
                    context.Response.Cookies.Append("Authorization", session.Data?.Token ?? "");
                }
            }
            else
            {
                var session = await _authenticationService.GetSession(token);
                if (session == null || session?.ExpiresAt < DateTime.UtcNow)
                {
                    context.Response.Cookies.Delete("Authorization");
                    context.Response.Redirect("/");
                    return;
                }
            }
            await _next(context);
        }
    }
}
