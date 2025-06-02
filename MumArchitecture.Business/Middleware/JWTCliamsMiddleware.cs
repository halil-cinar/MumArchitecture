using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using MumArchitecture.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Middleware
{
    public class JWTCliamsMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public JWTCliamsMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                context.Items["UserClaims"] = GetUserClaims(token);
            }
            await _requestDelegate(context);
        }

        private UserClaim? GetUserClaims(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);

                return new UserClaim
                {
                    UserID = Convert.ToInt32(claims.ContainsKey("UserID") ? claims["UserID"] : "0"),
                    User = claims.ContainsKey("UserID") ? JsonConvert.DeserializeObject<UserDto>(claims.GetValueOrDefault("User") ?? "") : null
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
