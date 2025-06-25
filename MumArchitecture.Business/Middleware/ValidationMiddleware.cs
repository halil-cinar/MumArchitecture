using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MumArchitecture.Business.Result;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post)
            {
                var endpoint = context.GetEndpoint();
                var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (descriptor != null)
                {
                    var parameter = descriptor.MethodInfo.GetParameters().FirstOrDefault(p => p.ParameterType.IsClass && p.ParameterType != typeof(string));
                    if (parameter != null)
                    {
                        var modelType = parameter.ParameterType;
                        context.Request.EnableBuffering();
                        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                        var body = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;

                        try
                        {
                            var model = JsonSerializer.Deserialize(body, modelType);
                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(model);

                            if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                            {
                                var errors = validationResults
                                    .GroupBy(e => e.MemberNames.FirstOrDefault() ?? "")
                                    .ToDictionary(
                                        g => g.Key,
                                        g => g.Select(e => e.ErrorMessage).ToArray()
                                    );

                                context.Response.StatusCode = StatusCodes.Status204NoContent;
                                context.Response.ContentType = "application/json";
                                var messages=new List<StateMessage>();
                                foreach ( var error in errors)
                                {
                                    messages.AddRange(error.Value.Select(x => new StateMessage() { Message=x,Priority=EPriority.Error}));
                                }
                                var result = new SystemResult<object>()
                                {
                                    Messages=messages,

                                };
                                await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                                return;
                            }
                        }
                        catch (JsonException)
                        {
                            context.Response.StatusCode = StatusCodes.Status204NoContent;
                            context.Response.ContentType = "application/json";
                            var result = new SystemResult<object>()
                            {
                                Messages = new List<StateMessage>() { new StateMessage { Message="Geçersiz JSON formatı.",Priority=EPriority.Error} }
                            };
                            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }


    }
}
