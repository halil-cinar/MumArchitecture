using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace MumArchitecture.Business.Result
{
    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public IEnumerable<double> Data { get; set; } = new List<double>();
    }

    public class ChartPayload
    {
        public IEnumerable<string> Labels { get; set; }= new List<string>();
        public IEnumerable<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();
    }

    public class ChartResult : SystemResult<ChartPayload>
    {
        public object ToChartJsObject()
        {
            return new
            {
                labels = Data?.Labels ?? Enumerable.Empty<string>(),
                datasets = Data?.Datasets.Select(d => new
                {
                    label = d.Label,
                    data = d.Data
                }) ?? Enumerable.Empty<object>()
            };
        }

        public override JsonResult ToJsonResult()
        {
            return new JsonResult(new
            {
                success = IsSuccess,
                messages = Messages.Select(x => new { x.Message, x.Priority }),
                data = ToChartJsObject()
            });
        }
    }
}
