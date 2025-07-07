using NodaTime.Calendars;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using MumArchitecture.Domain;
namespace MumArchitecture.WebApp.TagHelpers
{
    /*
        Hidden,
        Select,
        Select2,
        MultiSelect,
        MultiSelect2,
        Html,
        Float,
        Int,
        Text,
        TextArea,
        File,
        Switch,
        RadioSwitch,
        Datetime,
        Date,
        Custom
    */

    [HtmlTargetElement("form-input", ParentTag = "ajax-form")]
    public class FormInputTagHelper : TagHelper
    {
        public string Name { get; set; }
        public string Label { get; set; } = "";
        public string Placeholder { get; set; } = "";
        public string Type { get; set; } = "text";
        public bool Required { get; set; } = false;
        public bool Multiple { get; set; } = false;
        public IEnumerable<SelectListItem> Items { get; set; } 
        public string Value { get; set; } = "";
        public string CustomHtml { get; set; } = "";
        public string GetUrl { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            //var colWidth = context.Items.TryGetValue("ColumnWidth", out var wObj) ? (int)wObj : 12;
            output.PreContent.AppendHtml(@"<div class=""column-item"">");
            output.PostContent.AppendHtml("</div>");
            var fieldType = Type?.ToLowerInvariant() ?? "text";
            switch (fieldType)
            {
                case "hidden":
                    output.Content.SetHtmlContent(@$"<input type=""hidden"" name=""{Name}"" value=""{Value}"" />");
                    break;
                case "select":
                case "select2":
                case "multiselect":
                case "multiselect2":
                    var isMultiple = fieldType.StartsWith("multi") || Multiple;
                    var selectClass = (fieldType == "select2" || fieldType == "multiselect2") ? "select2" : "";
                    var multipleAttr = isMultiple ? " multiple" : "";
                    var options = string.Join("", Items?.Select(i => $@"<option value=""{i.Value}""{(i.Selected ? " selected" : "")}>{i.Text}</option>")??new List<string>() { !isMultiple?$"<option value=\"\" selected>{Lang.Value("Select")}</option>":"" });
                    output.Content.SetHtmlContent($@"
<div class=""form-floating mb-3"">
<select class=""form-select {selectClass}"" name=""{Name}"" id=""{Name}""{multipleAttr} placeholder=""{Placeholder}"">
{options}
</select>
<label for=""{Name}"">{Label}</label>
</div>");
                    break;
                case "html":
                    output.Content.SetHtmlContent($@"<div class=""mb-3""><textarea class=""summernote"" name=""{Name}"" id=""{Name}"">{Value}</textarea></div>");
                    break;
                case "textarea":
                    output.Content.SetHtmlContent($@"
<div class=""form-floating mb-3"">
<textarea class=""form-control"" name=""{Name}"" id=""{Name}"" placeholder=""{Placeholder}"">{Value}</textarea>
<label for=""{Name}"">{Label}</label>
</div>");
                    break;
                case "file":
                    var multiAttr = Multiple ? " multiple" : "";
                    var existing = string.IsNullOrWhiteSpace(Value) ? "" : $" data-existing=\"{Value}\"";
                    output.Content.SetHtmlContent($@"
<label for=""{Name}"" class=""form-label fw-semibold mb-1"">{Label}</label>
<input class=""filepond"" type=""file"" name=""{Name}"" id=""{Name}""{multiAttr}{existing} />
");
                    break;
                case "switch":
                    var checkedAttr = (Value == "true" || Value == "True") ? "checked" : "";
                    output.Content.SetHtmlContent($@"
<div class=""form-check form-switch mb-3"">
<input class=""form-check-input"" type=""checkbox"" role=""switch"" id=""{Name}"" name=""{Name}"" {checkedAttr}>
<label class=""form-check-label"" for=""{Name}"">{Label}</label>
</div>");
                    break;
                case "radioswitch":
                    output.Content.SetHtmlContent($@"<div id=""{Name}_group"" class=""mb-3""></div>");
                    break;
                case "datetime":
                    output.Content.SetHtmlContent($@"
<div class=""form-floating mb-3"">
<input type=""text"" class=""form-control datetime-picker"" name=""{Name}"" id=""{Name}"" value=""{Value}"" placeholder=""{Placeholder}"">
<label for=""{Name}"">{Label}</label>
</div>");
                    break;
                case "date":
                    output.Content.SetHtmlContent($@"
<div class=""form-floating mb-3"">
<input type=""text"" class=""form-control date-picker"" name=""{Name}"" id=""{Name}"" value=""{Value}"" placeholder=""{Placeholder}"">
<label for=""{Name}"">{Label}</label>
</div>");
                    break;
                case "float":
                case "int":
                case "text":
                default:
                    var inputTypeTxt = fieldType == "float" || fieldType == "int" ? "number" : "text";
                    var inputMode = fieldType == "float" ? "decimal" : (fieldType == "int" ? "numeric" : "text");
                    output.Content.SetHtmlContent($@"
<div class=""form-floating mb-3"">
<input type=""{inputTypeTxt}"" inputmode=""{inputMode}"" class=""form-control"" name=""{Name}"" id=""{Name}"" value=""{Value}"" placeholder=""{Placeholder}"">
<label for=""{Name}"">{Label}</label>
</div>");
                    break;
                case "custom":
                    output.Content.SetHtmlContent(CustomHtml);
                    break;
            }
            if (!string.IsNullOrWhiteSpace(GetUrl) && (fieldType == "select" || fieldType == "select2" || fieldType == "multiselect" || fieldType == "multiselect2"))
            {
                output.PostContent.AppendHtml($@"
<script>
$.getJSON('{GetUrl}', null)
    .done(function(data) {{
        if (!data.success) {{
            throw new Error((data.messages||data.Messages).map(function(m){{return m.Message||m.message;}}).join(', ')||'Bir hata oluştu');
        }}
        var $select = $('#{Name}');
        $select.find('option:not([value])').remove();
        $.each(data.data, function(val, text) {{
            $select.append(new Option(text, val));
        }});
        if ($select.hasClass('select2')) $select.trigger('change');
    }})
    .fail(function(_, status, err){{console.error('Veri çekme hatası:', status, err);}});
</script>");
            }
            if (!string.IsNullOrWhiteSpace(GetUrl) && fieldType == "radioswitch")
            {
                output.PostContent.AppendHtml($@"
<script>
$.getJSON('{GetUrl}', null)
    .done(function(data) {{
        if (!data.success) {{
            throw new Error((data.messages||data.Messages).map(function(m){{return m.Message||m.message;}}).join(', ')||'Bir hata oluştu');
        }}
        var $group = $('#{Name}_group');
        $.each(data.data, function(val, text) {{
            var id = '{Name}_' + val;
            var html = '<div class=""form-check form-switch""><input class=""form-check-input"" type=""radio"" name=""{Name}"" id=""'+id+'"" value=""'+val+'""><label class=""form-check-label"" for=""'+id+'"">'+text+'</label></div>';
            $group.append(html);
            }}
        }});
    }}
}})
    .fail(function(_, status, err){{ {{ console.error('Veri çekme hatası:', status, err); }} }});
</script> ");
            }
        }
    }
}
