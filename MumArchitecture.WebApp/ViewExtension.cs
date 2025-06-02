using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MumArchitecture.Business.Result;
using MumArchitecture.Domain;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Domain.Abstract;


namespace MumArchitecture.Web
{
    public static class ViewExtension
    {
        public static string GetStateMessages(this RazorPage razorPage, string sep = "<br/>")
        {
            razorPage.TempData.TryGetValue("StateMessage", out object? message);
            return message?.ToString()??"";
        }
        public static string GetSuccessMessage(this RazorPage razorPage)
        {
            razorPage.TempData.TryGetValue("SuccessMessage", out object? message);
            return message?.ToString() ?? "";
        }

        public static bool HasError(this RazorPage razorPage)
        {
            razorPage.TempData.TryGetValue("StateMessage", out object? message);
            if (message != null && ((List<StateMessage>)message).Where(x => x.Priority == EPriority.Error || x.Priority == EPriority.SystemError).Count() > 0)
            {
                return true;
            }
            return false;
        }
        public static void AddStateMessage(this Controller controller, StateMessage message)
        {
            controller.TempData.TryAdd("StateMessage", JsonSerializer.Serialize(  new List<StateMessage> { message }));
        }
        public static void AddStateMessage(this Controller controller, List<StateMessage> message)
        {
            controller.TempData.TryAdd("StateMessage", JsonSerializer.Serialize(message));
        }
        public static void AddStateMessageInModalState(this Controller controller)
        {
            var errorMessages = controller.ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => new StateMessage { Message = error.ErrorMessage, Priority = EPriority.Error })
                .ToList();

            controller.TempData.TryAdd("StateMessage", JsonSerializer.Serialize(errorMessages));
        }

        public static void AddSuccessMessage(this Controller controller, string message)
        {
            controller.TempData.TryAdd("SuccessMessage", message);
        }
        public static bool HasRedirect(this RazorPage razorPage)
        {
            return razorPage.ViewContext.ViewBag.Redirect != null;
        }

        public static string GetRedirectUrl(this RazorPage razorPage)
        {
            return razorPage.ViewContext.ViewBag.Redirect ?? "";
        }

        public static bool UserCan(this RazorPage razorPage, string methodName)
        {
            var context = razorPage.ViewContext.HttpContext;
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return false;
            }
            var tokenStr = authorizationHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
            var authenticationService = AppSettings.instance?.serviceProvider?.GetRequiredService<IAuthenticationService>() ?? throw new Exception();
            var task = authenticationService.GetUserMethods(tokenStr);
            task.Wait();

            if (!task.Result.IsSuccess)
            {
                return false;
            }
            var methods = task.Result.Data;
            if (methods == null || !methods.Any(m => m.Name == methodName))
            {
                return false;
            }

            return true;
        }

        public static void SetRedirect(this Controller controller, string? redirectUrl)
        {
            controller.ViewBag.Redirect = redirectUrl;
        }

        public static Filter<T> QueryConvertFilter<T>(this Controller controller)
            where T : Entity, new()
        {
            var filter=Filter<T>.ConvertFilter(controller.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));
            return filter;  
        }

        public static JsonResult ErrorJson(this Controller controller)
        {
            var errorMessages = controller.ModelState.Values
               .SelectMany(state => state.Errors)
               .Select(error => new StateMessage { Message = error.ErrorMessage, Priority = EPriority.Error })
               .ToList();
            var result = new JsonResult(new
            {
                Success = false,
                Messages = errorMessages.Select(x => new
                {
                    x.Message,
                    x.Priority
                }),
            });
            result.ContentType = "application/json";
            return result;
        }
    }
}
