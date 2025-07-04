using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MumArchitecture.Domain;
using System.Text;

namespace MumArchitecture.Web.TagHelpers
{
    [HtmlTargetElement("input-box", ParentTag = "addupdatemodal")]
    public class InputBoxTagHelper : TagHelper
    {
        public string Type { get; set; } = "text";
        public string Class { get; set; } = "form-control";
        public string Placeholder { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Label { get; set; } = null;
        public string Value { get; set; } = "";
        public string InputStyle { get; set; } = "";
        public int Col { get; set; } = 6;
        public bool Required { get; set; } = false;
        public bool Multiple { get; set; } = false;

        public string? Link { get; set; } = null;
        [HtmlAttributeName("items")]
        public IEnumerable<SelectListItem> Items { get; set; } = new List<SelectListItem>();
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(Link))
            {

            }
            var sb = new StringBuilder();
            var modal = (AddUpdateModalTagHelper)context.Items["addupdatemodal_taghelper"];
            var For = modal.Id + "_" + Name;
            sb.AppendLine($"<div class=\"col-md-{Col}\">");
            if (Type != "HIDDEN")
            {
                sb.AppendLine($"<label for=\"{For}\" class=\"form-label\">{Label ?? Name}</label>");
            }
            if (Type == "SELECT")
            {
                sb.AppendLine(@"

<style>
.select2-container{z-index:1060!important}
</style>
");
                sb.AppendLine($"<select class=\"select2\" name=\"{Name}\" id=\"{For}\" {(Multiple ? "multiple" : "")}>");
                sb.AppendLine($"<option disabled {(Multiple ? "" : "selected")} hidden value=\"\">{Lang.Value("Select")}</option>");
                foreach (var item in Items)
                {

                    if (Value == item.Value)
                    {
                        sb.AppendLine($"<option selected {(item.Disabled ? "disabled" : "")} value=\"{item.Value}\">{item.Text}</option>");
                    }
                    else
                    {
                        sb.AppendLine($"<option {(item.Disabled ? "disabled" : "")} {(item.Selected ? "selected" : "")} value=\"{item.Value}\">{item.Text}</option>");
                    }
                }
                sb.AppendLine("</select>");
                if (!string.IsNullOrEmpty(Link))
                {
                    sb.AppendLine($@"
    <script>
        $.getJSON('{Link}', null)
            .done((data) => {{
                if (!data.success) {{
                    throw new Error(
                        (data.messages ?? data.Messages)?.map(m => m.Message ?? m.message).join(', ') || 'Bir hata oluştu'
                    );
                }}

                const $select = $('#{For}');
                $select.find('option:not([disabled])').remove();
                $.each(data.data, (val, text) => {{
                    $select.append(new Option(text, val));
                }});
            }})
            .fail((_, status, err) => console.error('Veri çekme hatası:', status, err));
    </script>
");

                }
            }
            else if (Type == "TEXTAREA")
            {
                sb.AppendLine($"<textarea class=\"{Class}\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" {(Required ? "required" : "")}>{Value}</textarea>");
            }
            else if (Type == "HTML")
            {
                sb.AppendLine($"<textarea class=\"{Class} summernote\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" {(Required ? "required" : "")}>{Value}</textarea>");
            }
            else if (Type == "DATE")
            {
                sb.AppendLine($"<input type=\"text\" class=\"{Class} flatpickr flatpickr-input calendar\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" value=\"{Value}\" {(Required ? "required" : "")} />");
            }
            else if (Type == "INT")
            {
                sb.AppendLine($"<input type=\"number\" step=\"1\" class=\"{Class}\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" value=\"{Value}\" {(Required ? "required" : "")} />");
            }
            else if (Type == "FLOAT")
            {
                sb.AppendLine($"<input type=\"number\" step=\"0.01\" class=\"{Class}\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" value=\"{Value}\" {(Required ? "required" : "")} />");
            }
            else if (Type == "TEXT")
            {
                sb.AppendLine($"<input type=\"text\" class=\"{Class}\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" value=\"{Value}\" {(Required ? "required" : "")} />");
            }
            else if (Type == "CHECKBOX")
            {
                sb.AppendLine($@"
        <div class='form-check form-switch'>
                        <input type ='checkbox' class='form-check-input {Class}' id='{For}' name='{Name}' value='true' {(Value?.ToLower() == "true" ? "checked" : "")} {(Required ? "required" : "")} style='transform:scale(1.4);'/>
        </div>");
            }


            else if (Type == "HIDDEN")
            {
                sb.AppendLine($"<input type=\"hidden\" class=\"{Class}\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" value=\"{Value}\" {(Required ? "required" : "")} />");
            }
            else if (Type == "FILE")
            {
                sb.AppendLine($"<input type=\"file\" class=\"{Class}\" id=\"{For}\" name=\"{Name}\" {(Required ? "required" : "")} " +
                    $"onchange=\"document.getElementById('{For}_filename').innerText = this.files[0]?.name || 'Dosya seçilmedi';\" />" +
                    $" < small id =\"{For}_filename\" class=\"form-text text-muted\">Dosya seçilmedi</small>");
            }
            else
            {
                sb.AppendLine($"<input type=\"text\" class=\"{Class}\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" value=\"{Value}\" {(Required ? "required" : "")} />");
            }

            //sb.AppendLine($"<input type=\"{Type}\" class=\"{Class}\" id=\"{For}\" name=\"{Name}\" placeholder=\"{Placeholder}\" value=\"{Value}\" {(Required ? "required" : "")} />");
            sb.AppendLine("</div>");
            output.TagName = null;
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
