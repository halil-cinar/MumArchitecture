using MumArchitecture.Domain;
using MumArchitecture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public virtual Microsoft.AspNetCore.Mvc.JsonResult ToJsonResult()
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

    }

    public class StateMessage
    {
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
