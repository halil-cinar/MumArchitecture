using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

namespace MumArchitecture.WebApp.TagHelpers
{
    [HtmlTargetElement("chart")]
    public class ChartTagHelper : TagHelper
    {
        public string Url { get; set; }
        public string Type { get; set; } = "bar";
        public string Colors { get; set; }
        public bool Horizontal { get; set; } = false;
        public int Width { get; set; } = 600;
        public int Height { get; set; } = 300;
        public string ExtraOptions { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var id = $"chart-{Guid.NewGuid():N}";
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "chart-wrapper position-relative");
            var html = $@"
<canvas id=""{id}"" width=""{Width}"" height=""{Height}""></canvas>
<div id=""{id}-mask"" class=""chart-mask position-absolute top-0 start-0 w-100 h-100 d-flex justify-content-center align-items-center"">Loading…</div>
<script>
window.chartRegistry = window.chartRegistry || [];
window.chartRegistry.push({{
    id: '{id}',
    url: '{Url}',
    type: '{Type}',
    colors: '{Colors ?? string.Empty}',
    horizontal: {(Horizontal ? "true" : "false")},
    options: {(!string.IsNullOrWhiteSpace(ExtraOptions) ? ExtraOptions : "null")}
}});
</script>";
            output.Content.SetHtmlContent(html);
        }
    }
}
