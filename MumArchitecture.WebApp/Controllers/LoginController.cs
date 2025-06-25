using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Result;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Dtos;

namespace MumArchitecture.WebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IIdentityService _identityService;

        public LoginController(IAuthenticationService authenticationService, IIdentityService identityService)
        {
            _authenticationService = authenticationService;
            _identityService = identityService;
        }

        public IActionResult Index()
        {
            var token = _authenticationService.AuthToken;
            if (!string.IsNullOrEmpty(token))
            {
                return Redirect("/");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(IdentityCheckDto identity)
        {
            var result = await _authenticationService.Login(identity);
            if (!result.IsSuccess)
            {
                return View("Index", identity);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            };

            Response.Cookies.Append("Authorization", result.Data?.Token??"", cookieOptions);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserDto user)
        {
            var result = await _authenticationService.SignUp(user);

            if (!result.IsSuccess)
            {
                return View();
            }

            return RedirectToAction("Index", "Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var result = await _identityService.ForgatPassword(email);
            if (!result.IsSuccess)
            {
                return View();
            }
            return RedirectToAction("Index", "Login");
        }

        public IActionResult ResetPassword(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Key = key;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string key, IdentityCheckDto identity)
        {
            if (string.IsNullOrEmpty(key))
            {
                return RedirectToAction("Index", "Login");
            }

            var result = await _identityService.ForgatPassword(key, identity);
            if (!result.IsSuccess)
            {
                ViewBag.Key = key;
                return View();
            }

            return RedirectToAction("Index", "Login");
        }

        public async Task<IActionResult> LogOut()
        {
            await _authenticationService.LogOut(_authenticationService.AuthToken);
            Response.Cookies.Delete("Authorization");
            return Redirect("/");
        }
    }
}
