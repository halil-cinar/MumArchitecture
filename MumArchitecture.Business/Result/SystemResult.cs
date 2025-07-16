using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Result
{
    public class SystemResult<T>
    {
        public T? Data { get; set; }
        public List<StateMessage> Messages { get; set; }

        public EPriority Status
        {
            get
            {
                int max = 0;
                foreach (var message in Messages)
                {
                    if ((int)message.Priority > max)
                    {
                        max = (int)message.Priority;
                    }
                }
                return (EPriority)max;
            }
        }
        public bool IsSuccess
        {
            get
            {
                return Status == EPriority.None || Status == EPriority.Information || Status == EPriority.Warning;
            }
        }

        public SystemResult()
        {
            Messages = new List<StateMessage>();
        }

        public void AddMessage<T2>(SystemResult<T2> result)
        {
            Messages.AddRange(result.Messages);
        }
        public void AddMessage(string message, EPriority priority = EPriority.Error)
        {
            Messages.Add(new StateMessage
            {
                Message = message,
                Priority = priority
            });
        }
        public void AddDefaultErrorMessage(Exception? ex, string message = "")
        {
            Messages.Add(new StateMessage
            {
                Message = Lang.Value("DefaultErrorMessage") + "  ; " + message,
                Priority = EPriority.SystemError
            });
        }

        public List<string> GetMessages()
        {
            return Messages?.Select(x => x.Message??"")?.ToList()??new List<string>();
        }

        public virtual JsonResult ToJsonResult()
        {
            var result = new Microsoft.AspNetCore.Mvc.JsonResult(new
            {
                Success = IsSuccess,
                //Status = Status,
                Messages = Messages.Select(x => new
                {
                    x.Message,
                    x.Priority
                }),
                Data = Data
            });

            return result;

        }
        public PaggingResult<T2> ToPaggingResult<T2>(Func<T?, List<T2>> func)
        {
            return new PaggingResult<T2>
            {
                Data = func(Data),
                ItemCount = Data is IEnumerable<T2> enumerable ? enumerable.Count() : 0,
                Messages = Messages,
            };
        }
    }
    public class StateMessage
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        public EPriority Priority { get; set; }
    }

    public enum EPriority
    {
        None = 0,
        Information,
        Warning,
        Error,
        SystemError
    }
}
