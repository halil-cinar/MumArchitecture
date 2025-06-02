using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MumArchitecture.Business.Notification;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Abstract
{
    public abstract class ServiceBase<TEntity>
        where TEntity : Entity, new()
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        //protected IRequestAccessor? RequestAccessor { get; private set; }
        protected IRepository<TEntity> Repository { get; private set; }
        protected HttpContext? HttpContext { get; private set; }
        private readonly IConfiguration _configuration;
        protected readonly int AuthUserId;
        protected ServiceBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            //RequestAccessor = serviceProvider.GetService<IRequestAccessor>();
            Repository = serviceProvider.GetRequiredService<IRepository<TEntity>>();
            HttpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();

            AuthUserId = GetUserIdFromJWTToken(HttpContext?.Request?.Headers?["Authorization"].FirstOrDefault() ?? "");
        }
        private int GetUserIdFromJWTToken(string token)
        {
            return 1;
        //Todo:
            if (string.IsNullOrEmpty(token))
                return -1;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppSettings.instance!.JwtSecret!);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,       // İsteğe bağlı: Üretici doğrulaması yapılacaksa true yapılabilir.
                ValidateAudience = false,     // İsteğe bağlı: Hedef kitle doğrulaması yapılacaksa true yapılabilir.
                //ValidateLifetime = true,      // Token süresi kontrol edilsin.
                ClockSkew = TimeSpan.Zero     // Fazladan zaman farkı tanımlanmayacak.
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return -1;

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return -1;
            }

            return userId;
        }

    }
}
