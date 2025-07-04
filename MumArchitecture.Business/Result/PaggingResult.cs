using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Result
{
    public class PaggingResult<T> : SystemResult<T>
    {
        public int CurrentPage { get; set; }
        public int ItemCount { get; set; }
        public int AllCount { get; set; }
        public int AllPageCount { get; set; }
        public new List<T>? Data { get; set; }


        public override Microsoft.AspNetCore.Mvc.JsonResult ToJsonResult()
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
                Data = Data,
                CurrentPage = CurrentPage,
                AllPageCount = AllPageCount,
                ItemCount = ItemCount,
                AllCount = AllCount,
            });

            return result;

        }
        public virtual JsonResult ToSelectResult(Func<T, string> value, Func<T, string> name)
        {
            if (Data == null)
            {
                return new Microsoft.AspNetCore.Mvc.JsonResult(new
                {
                    Success = false,
                    Messages = Messages.Select(x => new
                    {
                        x.Message,
                        x.Priority
                    }),
                    Data = new Dictionary<string, string>()
                });
            }
            var values = Data.Select(value).ToList();
            var names = Data.Select(name).ToList();
            var result = new Microsoft.AspNetCore.Mvc.JsonResult(new
            {
                Success = IsSuccess,
                //Status = Status,
                Messages = Messages.Select(x => new
                {
                    x.Message,
                    x.Priority
                }),
                Data = values.Zip(names, (v, n) => new { Value = v, Name = n }).ToDictionary(x => x.Value, x => x.Name)
            });
            return result;

        }

    }
}
