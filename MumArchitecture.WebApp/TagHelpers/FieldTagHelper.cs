using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.VisualStudio.Services.WebApi;
using MumArchitecture.Domain;
using System.Text;

namespace MumArchitecture.Web.TagHelpers
{
    [HtmlTargetElement("field", ParentTag = "filters")]
    public class FieldTagHelper : TagHelper
    {
        public string For { get; set; } = string.Empty;
        public string Names { get; set; } = string.Empty;
        public string Type { get; set; } = "TEXT";//SELECT, DATE, HIDDEN
        public int LgSize { get; set; } = 3;
        public string Label { get; set; } = string.Empty;
        [HtmlAttributeName("items")]
        public IEnumerable<SelectListItem> Items { get; set; }
        public string Value { get; set; } = "";
        public string? Link { get; set; } = null;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var id = For ?? Names;
            var label = Label ?? id;
            
            var size = $"col-md-{LgSize} form-floating";
            string input;

            switch (Type.ToUpper())
            {
                case "SELECT":
                    input = $"<select class=\"form-select\" id=\"{id}\" asp-items=\"{Items}\" name=\"{id}\"><option selected>Seçiniz</option>";
                    input+=($"<option selected  value=\"\">{Lang.Value("Select")}</option>");

                    foreach (var item in Items??Enumerable.Empty<SelectListItem>())
                    {
                        if (Value == item.Value)
                        {
                            input += $"<option selected {(item.Disabled?"disabled":"")} value=\"{item.Value}\">{item.Text}</option>";
                        }
                        else
                        {
                            input += $"<option {(item.Disabled?"disabled":"")} {(item.Selected?"selected":"")} value=\"{item.Value}\">{item.Text}</option>";
                        }

                    }
                        
                    input+="</select>";
                    input+=($@"
    <script>
        $.getJSON('{Link}', null)
            .done((data) => {{
                if (!data.success) {{
                    throw new Error(
                        (data.messages ?? data.Messages)?.map(m => m.Message ?? m.message).join(', ') || 'Bir hata oluştu'
                    );
                }}

                const $select = $('#{id}');
                $select.find('option:not([value])').remove();
                $.each(data.data, (val, text) => {{
                    $select.append(new Option(text, val));
                }});
            }})
            .fail((_, status, err) => console.error('Veri çekme hatası:', status, err));
    </script>
");
                    break;
                case "DATE":
                    input = $"<input type=\"date\" class=\"form-control\" placeholder=\"{label}\" id=\"{id}\" name=\"{id}\">";
                    break;
                case "HIDDEN":
                    output.TagName = null;
                    output.Content.SetHtmlContent($"<input type=\"hidden\" id=\"{id}\" value=\"{Value}\" / name=\"{id}\">");
                    return;
                default:
                    input = $"<input type=\"text\" class=\"form-control\" placeholder=\"{label}\" id=\"{id}\" name=\"{id}\">";
                    break;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"<div class=\"{size}\">");
            sb.AppendLine(input);
            sb.AppendLine($"<label for=\"{id}\">{label}</label>");
            sb.AppendLine("</div>");

            output.TagName = null;
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
