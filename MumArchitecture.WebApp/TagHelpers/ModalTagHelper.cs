using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace MumArchitecture.Web.TagHelpers
{
    [HtmlTargetElement("modal")]
    public class ModalTagHelper : TagHelper
    {
        public string Id { get; set; } = "";
        public string Class { get; set; } = "";
        public string Title { get; set; } = "";
        public string Size { get; set; } = "lg";
        public string Onclose { get; set; } = "";

        public string Url { get; set; } = "";
        public string GetUrl { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            context.Items["addupdatemodal_taghelper"] = this;
            var sb = new StringBuilder();
            sb.AppendLine($"<div class=\"modal fade\" id=\"{Id}_modal\" tabindex=\"-1\" aria-labelledby=\"{Id}_label\" aria-hidden=\"true\" data-get-url=\"{GetUrl}\" data-save-url=\"{Url}\">");
            sb.AppendLine($"<div class=\"modal-dialog modal-{Size} modal-dialog-scrollable\">");
            sb.AppendLine("<div class=\"modal-content\">");
            sb.AppendLine("<div class=\"modal-header\">");
            sb.AppendLine($"<h5 class=\"modal-title\" id=\"{Id}_label\">{Title}</h5>");
            sb.AppendLine("<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\" aria-label=\"Close\"></button>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"modal-body\">");

            var content = await output.GetChildContentAsync();
            var inner = content.GetContent();

            sb.AppendLine(inner);//inner

            sb.AppendLine(" </div>");
            sb.AppendLine("<div class=\"modal-footer\">");
            sb.AppendLine(" <button type=\"button\" class=\"btn btn-secondary\" data-bs-dismiss=\"modal\">Close</button>");
            sb.AppendLine("</div></div></div>");
            sb.AppendLine("</div>");
            //          sb.AppendLine("<script>");
            //            sb.AppendLine($"  ModalFormManager.init({{modalSelector: '#{Id}_modal',getUrl: '{GetUrl}', saveUrl: '{Url}'}});");


            ////        sb.AppendLine("</script>");

            output.TagName = null;
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
