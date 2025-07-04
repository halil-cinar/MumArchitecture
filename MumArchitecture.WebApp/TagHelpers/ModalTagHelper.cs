using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Threading.Tasks;

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
            var child = await output.GetChildContentAsync();
            var sb = new StringBuilder();

            sb.AppendLine($"<div class=\"modal fade {Class}\" id=\"{Id}_modal\" tabindex=\"-1\" aria-labelledby=\"{Id}_label\" aria-hidden=\"true\" data-get-url=\"{GetUrl}\" data-save-url=\"{Url}\">");
            sb.AppendLine($"  <div class=\"modal-dialog modal-{Size} modal-dialog-scrollable\">");
            sb.AppendLine("    <div class=\"modal-content\">");
            sb.AppendLine("      <div class=\"modal-header\">");
            sb.AppendLine($"        <h5 class=\"modal-title\" id=\"{Id}_label\">{Title}</h5>");
            sb.AppendLine("        <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\" aria-label=\"Close\"></button>");
            sb.AppendLine("      </div>");
            sb.AppendLine("      <div class=\"modal-body\">");
            sb.AppendLine(child.GetContent());
            sb.AppendLine("      </div>");
            sb.AppendLine("      <div class=\"modal-footer\">");
            sb.AppendLine("        <button type=\"button\" class=\"btn btn-secondary\" data-bs-dismiss=\"modal\">Close</button>");
            if (!string.IsNullOrWhiteSpace(Url))
                sb.AppendLine("        <button type=\"button\" class=\"btn btn-primary\" data-modal-save>Save</button>");
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");

            sb.AppendLine($@"<script>
(function(){{
    var modal = $('#{Id}_modal');
    modal.on('show.bs.modal', function () {{
        var url = $(this).data('get-url');
        if(url) $(this).find('.modal-body').load(url);
    }});
    modal.on('click','[data-modal-save]', function (e) {{
        e.preventDefault();
        var saveUrl = modal.data('save-url');
        var form    = modal.find('form');
        if(!saveUrl || !form.length) return;
        $.ajax({{
            url: saveUrl,
            type: 'POST',
            data: form.serialize()
        }}).done(function (r) {{
            if(r === false || (r.success !== undefined && !r.success)) {{
                modal.find('.modal-body').html(r);
            }} else {{
                modal.modal('hide');
            }}
        }});
    }});
    modal.on('hidden.bs.modal', function () {{
        $(this).find('.modal-body').empty();
        if('{Onclose}') {{
            var fn = window['{Onclose}'];
            if(typeof fn === 'function') fn();
        }}
    }});
}})();
</script>");

            output.TagName = null;
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
