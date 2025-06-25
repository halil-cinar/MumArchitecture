using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Services.Identity;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Logging;
using MumArchitecture.Business.Notification;
using MumArchitecture.Business.Result;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Services
{
    public class AuthenticationService : IAuthenticationService, IAddScope
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRoleService _roleService;
        private readonly IIdentityService _identityService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Session> _sessionRepository;
        public AuthenticationService(IServiceProvider serviceProvider)
        {
            _userRepository = serviceProvider.GetRequiredService<IRepository<User>>();
            _roleService = serviceProvider.GetRequiredService<IRoleService>();
            _identityService = serviceProvider.GetRequiredService<IIdentityService>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            _sessionRepository = serviceProvider.GetRequiredService<IRepository<Session>>();
        }

        public int? AuthUserId
        {
            get
            {
                if (string.IsNullOrEmpty(AuthToken))
                {
                    return null;
                }
                var result = GetUserIdFromJWTToken(AuthToken);
                result.Wait();
                return result.Result;
            }
        }

        public string AuthToken
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Request?.Cookies?["Authorization"] ?? "";
            }
        }

        public bool IsLogin
        {
            get
            {
                return !string.IsNullOrEmpty(AuthToken);
            }
        }

        async Task<SessionListDto?> IAuthenticationService.GetSession(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }
                var session = await _sessionRepository.Get(x => x.Token == token);
                return session;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SystemResult<UserListDto>> GetUser(string token)
        {

            var result = new SystemResult<UserListDto>();
            try
            {

                int? userId = await GetUserIdFromJWTToken(token);
                var user = await _userRepository.Get(x => x.Id == userId);
                if (user == null)
                {
                    throw new UserException(Lang.Value(Messages.RecordNotFound));
                }
                result.Data = user;
                return result;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }

            return result;
        }

        public async Task<SystemResult<List<MethodListDto>>> GetUserMethods(string token)
        {
            var result = new SystemResult<List<MethodListDto>>();
            try
            {
                int? userId = await GetUserIdFromJWTToken(token);
                var methodResult = await _roleService.GetUserMethods(userId ?? 0);
                if (!methodResult.IsSuccess)
                {
                    result.AddMessage(methodResult);
                    return result;
                }
                result.Data = methodResult.Data;
                return result;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }

            return result;
        }

        public async Task<SystemResult<SessionListDto>> Login(IdentityCheckDto identity)
        {
            var result = new SystemResult<SessionListDto>();
            try
            {
                var user = await _userRepository.Get(u => u.Email == identity.Email) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                var identityResult = await _identityService.CheckPassword(identity);
                if (!identityResult.IsSuccess || identityResult.Data == false)
                {
                    throw new UserException(Lang.Value("EmailOrPasswordIsWrong"));
                }
                string tokenString = CreateJWTToken(user);
                var oldSession = await _sessionRepository.Get(x => x.Token == AuthToken);
                // Oluşturulan token bilgilerini SessionListDto içerisine aktarıyoruz.
                var session = new Session
                {

                    Token = tokenString,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection.GetFullIpAddress(),
                    UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers?["User-Agent"] ?? throw new UserException("YouMustUseABrowser"),
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                };
                if (oldSession != null)
                {
                    session.Id = oldSession.Id;
                    await _sessionRepository.Update(session);
                }
                else
                {
                    await _sessionRepository.Add(session);
                }
                result.Data = session;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }

            return result;
        }

        private string CreateJWTToken(User? user)
        {
            // JWT token oluşturulması
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppSettings.instance!.JwtSecret!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, (user?.Id??0).ToString()),
                    // Gerekirse ek claim'ler eklenebilir
                }),
                //Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        public async Task<SystemResult<SessionListDto>> CreateSession()
        {
            var result = new SystemResult<SessionListDto>();
            try
            {
                var jwtToken = CreateJWTToken(null);
                var session = new Session
                {
                    Token = jwtToken,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection.GetFullIpAddress(),
                    UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers?["User-Agent"] ?? throw new UserException("YouMustUseABrowser"),
                    UserId = 0,
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                };
                await _sessionRepository.Add(session);

                result.Data = session;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }

            return result;
        }
        public async Task<SystemResult<Nothing>> LogOut(string token)
        {
            var result = new SystemResult<Nothing>();
            try
            {
                int? userId = await GetUserIdFromJWTToken(token);
                var user = await _userRepository.Get(x => x.Id == userId);
                if (user == null)
                {
                    throw new UserException(Lang.Value(Messages.RecordNotFound));
                }

                var session = await _sessionRepository.Get(x => x.Token == token) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                await _sessionRepository.Delete(x => x.Id == session.Id);

                result.Data = new Nothing();
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }

            return result;
        }

        public async Task<SystemResult<Nothing>> SignUp(UserDto user)
        {
            var result = new SystemResult<Nothing>();
            try
            {
                var userResult = await _userService.Save(user);
                result.AddMessage(userResult);
                result.Data = new Nothing();
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }

            return result;
        }

        private async Task<int?> GetUserIdFromJWTToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return default;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppSettings.instance!.JwtSecret!);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,       // İsteğe bağlı: Üretici doğrulaması yapılacaksa true yapılabilir.
                ValidateAudience = false,     // İsteğe bağlı: Hedef kitle doğrulaması yapılacaksa true yapılabilir.
                ValidateLifetime = false,      // Token süresi kontrol edilsin.
                ClockSkew = TimeSpan.Zero     // Fazladan zaman farkı tanımlanmayacak.
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new Exception("Token içerisinde geçerli bir kullanıcı id bilgisi bulunamadı.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UserException(Lang.Value(Messages.RecordNotFound));
            }
            var session = await _sessionRepository.Get(x => x.Token == token);
            if (session == null)
            {
                throw new UserException(Lang.Value(Messages.RecordNotFound));
            }
            if (session.ExpiresAt.HasValue &&
                (session.ExpiresAt.Value - DateTime.UtcNow).TotalHours > 0 &&
                (session.ExpiresAt.Value - DateTime.UtcNow).TotalHours < 3)
            {
                session.ExpiresAt = DateTime.UtcNow.AddDays(1);
                await _sessionRepository.Update(session);
            }
            return userId == 0 ? null : userId;
        }

        Task<int?> IAuthenticationService.GetUserIdFromJWTToken(string token)
        {
            return GetUserIdFromJWTToken(token);
        }
    }
}
