using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Services;
using MumArchitecture.Domain;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Extensions
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]

    public class AuthorizationAttribute :Attribute, IAuthorizationFilter
    {
        private readonly string _methodName;
        private readonly IAuthenticationService _authenticationService;
        public AuthorizationAttribute(string methodName)
        {
            _methodName = methodName;
            _authenticationService = AppSettings.instance?.serviceProvider?.GetService<IAuthenticationService>()??throw new Exception();
            ;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var authorizationHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                var tokenStr = authorizationHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();

                _authenticationService.GetUserMethods(tokenStr).ContinueWith(task =>
                {
                    if (!task.Result.IsSuccess)
                    {
                        context.Result = new UnauthorizedResult();
                        return;
                    }
                    var methods = task.Result.Data;
                    if (methods == null || !methods.Any(m => m.Name == _methodName))
                    {
                        context.Result = new UnauthorizedResult();
                        return;
                    }
                });
                ;
            }catch(Exception ex)
            {
                ;
            }
            return;
        }
    }
}
