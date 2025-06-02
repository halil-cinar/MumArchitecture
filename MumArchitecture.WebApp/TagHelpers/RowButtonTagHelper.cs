using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace MumArchitecture.Web.TagHelpers
{
    [HtmlTargetElement("rowbutton", ParentTag = "grid")]
    public class RowButtonTagHelper : TagHelper
    {

        public string Id { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string OnClick { get; set; } = string.Empty;
        public string Modal { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var gridId = ((GridTagHelper)context.Items["grid_taghelper"]).Id;
            var sb = new StringBuilder();
            var input = "";
            Id = Id ?? $"{gridId}_btn_" + DateTime.Now.Ticks.ToString()[0];
            //<li><a class="dropdown-item" href="#" onclick="editOrder(${item['id']})">Düzenle</a></li>
            if (!string.IsNullOrEmpty(Modal))
            {
                input = $"<a class=\"dropdown-item {Class}\" href=\"#\" id=\"{Id}\" data-id=\"[id]\" data-bs-toggle=\"modal\" data-bs-target=\"#{Modal}_modal\">{(string.IsNullOrEmpty(Icon) ? "" : $"<i class=\"{Icon}\"></i>")}{Title}</a>";
            }
            else if (!string.IsNullOrEmpty(Url))
            {
                input = $"<a class=\"dropdown-item {Class}\" href=\"{Url}\" id=\"{Id}\" data-id=\"[id]\">{(string.IsNullOrEmpty(Icon) ? "" : $"<i class=\"{Icon}\"></i>")}{Title}</a>";
            }
            else
            {
                input = $"<a class=\"dropdown-item {Class}\" href=\"javascript:;\" id=\"{Id}\" onclick=\"{OnClick}\" data-id=\"[id]\">{(string.IsNullOrEmpty(Icon)?"": $"<i class=\"{Icon}\"></i>")}{Title}</a>";
            }

            sb.AppendLine($"<li>{input}</li>");

            //output.TagName = null;
            //output.Content.SetHtmlContent(sb.ToString());
            var items = context.Items["grid_rowbuttons"] as List<string>;
            items!.Add(sb.ToString());
            context.Items["grid_rowbuttons"] = items;
            output.SuppressOutput();
            return Task.CompletedTask;
        }
    }
}
