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
            if (HttpMethods.IsPost(context.Request.Method))
            {
                var endpoint = context.GetEndpoint();
                var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

                if (descriptor != null)
                {
                    var parameter = descriptor.MethodInfo.GetParameters()
                                       .FirstOrDefault(p => p.ParameterType.IsClass && p.ParameterType != typeof(string));

                    if (parameter != null)
                    {
                        var modelType = parameter.ParameterType;
                        object? model = null;

                        if (context.Request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true
                            || context.Request.ContentType?.EndsWith("+json", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            context.Request.EnableBuffering();
                            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                            var body = await reader.ReadToEndAsync();
                            context.Request.Body.Position = 0;
                            model = JsonSerializer.Deserialize(body, modelType);
                        }
                        else if (context.Request.HasFormContentType)
                        {
                            var form = await context.Request.ReadFormAsync();
                            var dict = new Dictionary<string, object>();

                            foreach (var kvp in form)
                            {

                                if (kvp.Value.Count == 0) continue;

                                if (kvp.Value.Count == 1)
                                {
                                    var single = kvp.Value[0];
                                    var firstProp = modelType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                                    if (firstProp != null &&
                                        firstProp.PropertyType.IsGenericType &&
                                        firstProp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                                    {
                                        var elemType = firstProp.PropertyType.GetGenericArguments()[0];
                                        var list = kvp.Value.Select(v => Convert.ChangeType(v, elemType)).ToArray();
                                        dict[kvp.Key] = list;
                                    }
                                    else if (firstProp != null && firstProp.PropertyType==typeof(string))
                                    {
                                        dict[kvp.Key] = single.ToString();
                                    }
                                    else
                                    {
                                        dict[kvp.Key] = int.TryParse(single, out var num) ? num : bool.TryParse(single, out var b) ? b : float.TryParse(single, out var f) ? f : single ?? "";
                                    }
                                }
                                else
                                {
                                    var firstProp = modelType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                                    if (firstProp != null &&
                                        firstProp.PropertyType.IsGenericType &&
                                        firstProp.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                                    {
                                        var elemType = firstProp.PropertyType.GetGenericArguments()[0];
                                        var list = kvp.Value.Select(v => Convert.ChangeType(v, elemType)).ToArray();
                                        dict[kvp.Key] = list;
                                    }
                                    else
                                    {
                                        dict[kvp.Key] = kvp.Value.ToArray();
                                    }
                                }
                            }

                            if (!dict.TryGetValue("Id", out var idObj) || string.IsNullOrWhiteSpace(idObj?.ToString()))
                            {
                                dict["Id"] = 0;
                            }

                            var json = JsonSerializer.Serialize(dict);
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            try
                            {
                                model = JsonSerializer.Deserialize(json, modelType, options);
                            }
                            catch (Exception ex)
                            {

                            }
                        }


                        if (model != null)
                        {
                            var validationResults = new List<ValidationResult>();
                            var validationContext = new ValidationContext(model);

                            if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
                            {
                                var errors = validationResults
                                             .GroupBy(e => e.MemberNames.FirstOrDefault() ?? "")
                                             .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                context.Response.ContentType = "application/json";

                                var messages = errors
                                               .SelectMany(e => e.Value.Select(x => new StateMessage
                                               {
                                                   Message = x,
                                                   Priority = EPriority.Error
                                               }))
                                               .ToList();

                                var result = new SystemResult<object> { Messages = messages };
                                await context.Response.WriteAsync(JsonSerializer.Serialize(result,new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy=JsonNamingPolicy.CamelCase
                                }));
                                return;
                            }
                        }
                    }
                }
            }

            await _next(context);
        }



    }
}
