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
                AllPageCount= AllPageCount,
                ItemCount = ItemCount,
                AllCount = AllCount,
            });
            
            return result;

        }
    }
}
