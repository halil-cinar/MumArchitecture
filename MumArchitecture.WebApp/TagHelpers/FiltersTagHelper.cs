using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Web.TagHelpers
{
    [HtmlTargetElement("filters", ParentTag = "grid")]
    public class FiltersTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var gridId = ((GridTagHelper)context.Items["grid_taghelper"]).Id;
            var child = output.GetChildContentAsync().Result;
            var inner = child.GetContent();
            var items = context.Items["grid_gridbuttons"] as List<string>;

            var sb = new StringBuilder();
            sb.AppendLine($"<form id=\"{gridId}FilterForm\">");
            sb.AppendLine("<div class=\"row g-3 align-items-end\">");
            sb.AppendLine(inner);
            sb.AppendLine("</div>");
            sb.AppendLine($"<div class=\"col-md-3 w-100 mt-2 d-flex text-end flex-row-reverse align-self-end\">");
            sb.AppendLine($"<button type=\"button\" class=\"btn btn-orange\" onclick=\"Grid.applyFilters('{gridId}');\">{"Ara"}</button>");
            foreach (var item in items!)
            {
                sb.AppendLine(item);
            }
            sb.AppendLine($"</div>");
            sb.AppendLine("</form>");

            output.TagName = null;
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
