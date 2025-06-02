using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace MumArchitecture.Web.TagHelpers
{
    [HtmlTargetElement("gridbutton", ParentTag = "filters")]
    public class GridButtonTagHelper : TagHelper
    {
        public string Id { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Color { get; set; } = "orange";
        public string OnClick { get; set; } = string.Empty;
        public string Modal { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var gridId = ((GridTagHelper)context.Items["grid_taghelper"]).Id;
            var sb = new StringBuilder();
            var input = "";
            
            Id = Id ?? $"{gridId}_gridbtn_" + DateTime.Now.Ticks.ToString()[0];
            //<li><a class="dropdown-item" href="#" onclick="editOrder(${item['id']})">Düzenle</a></li>
            if (!string.IsNullOrEmpty(Modal))
            {
                input = $"<button type=\"button\" class=\"btn btn-{Color} {Class}\" href=\"#\" id=\"{Id}\" {Data} data-bs-toggle=\"modal\" data-bs-target=\"#{Modal}_modal\">{(string.IsNullOrEmpty(Icon) ? "" : $"<i class=\"{Icon}\"></i>")}{Title}</button>";
            }
            else if (!string.IsNullOrEmpty(Url))
            {
                input = $"<a class=\"btn btn-{Color} {Class}\" href=\"{Url}\" id=\"{Id}\" {Data}>{(string.IsNullOrEmpty(Icon) ? "" : $"<i class=\"{Icon}\"></i>")}{Title}</a>";
            }
            else
            {
                input = $"<a class=\"btn btn-{Color} {Class}\" href=\"javascript:;\" id=\"{Id}\" onclick=\"{OnClick}\" {Data}>{(string.IsNullOrEmpty(Icon) ? "" : $"<i class=\"{Icon}\"></i>")}{Title}</a>";
            }

            sb.AppendLine($"{input}");

            //output.TagName = null;
            //output.Content.SetHtmlContent(sb.ToString());
            var items = context.Items["grid_gridbuttons"] as List<string>;
            items!.Add(sb.ToString());
            context.Items["grid_gridbuttons"] = items;
            output.Content.SetHtmlContent("");
        }
    }
}
