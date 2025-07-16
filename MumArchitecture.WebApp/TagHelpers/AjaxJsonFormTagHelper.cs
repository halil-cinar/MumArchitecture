using Microsoft.AspNetCore.Razor.TagHelpers;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using System.Text;

namespace MumArchitecture.WebApp.TagHelpers
{
    [HtmlTargetElement("ajax-json-form")]
    public class AjaxJsonFormTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-name")]
        public string Name { get; set; } = "AjaxJsonForm";

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; } = "";

        [HtmlAttributeName("asp-method")]
        public string Method { get; set; } = "post";

        [HtmlAttributeName("asp-settings")]
        public List<SettingType> Settings { get; set; } = new();

        [HtmlAttributeName("asp-value")]
        public string? JsonValue { get; set; }

        [HtmlAttributeName("asp-entity-id")]
        public string? EntityId { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "form";
            output.Attributes.SetAttribute("id", Name);
            output.Attributes.SetAttribute("action", Action);
            output.Attributes.SetAttribute("method", Method);
            output.Attributes.SetAttribute("enctype", "multipart/form-data");
            if (JsonValue != null) output.Attributes.SetAttribute("data-value", JsonValue);
            if (EntityId != null) output.Attributes.SetAttribute("data-entity-id", EntityId);

            var sb = new StringBuilder();
            RenderSettings(Settings, sb, "");
            sb.AppendLine("<div class=\"mt-3\"><button type=\"submit\" class=\"btn btn-primary\">Save</button></div>");
            output.Content.SetHtmlContent(sb.ToString());
            output.PostElement.SetHtmlContent($"<script>$(function(){{AjaxJsonForm.init('#{Name}');}});</script>");
        }

        private void RenderSettings(IEnumerable<SettingType> settings, StringBuilder sb, string prefix)
        {
            foreach (var s in settings)
            {
                var path = string.IsNullOrEmpty(prefix) ? s.Name : $"{prefix}.{s.Name}";
                switch (s.Type)
                {
                    case ESetting.TEXT:
                        sb.AppendLine($"<div class=\"mb-3\"><label class=\"form-label\">{s.Name}</label><input type=\"text\" class=\"form-control\" name=\"{path}\" /></div>");
                        break;
                    case ESetting.NUMBER:
                        sb.AppendLine($"<div class=\"mb-3\"><label class=\"form-label\">{s.Name}</label><input type=\"number\" class=\"form-control\" name=\"{path}\" step=\"any\" /></div>");
                        break;
                    case ESetting.BOOL:
                        sb.AppendLine($"<div class=\"form-check mb-3\"><input type=\"checkbox\" class=\"form-check-input\" id=\"{path}\" name=\"{path}\" /><label class=\"form-check-label\" for=\"{path}\">{s.Name}</label></div>");
                        break;
                    case ESetting.FILE:
                        sb.AppendLine($"<div class=\"mb-3\"><label class=\"form-label\">{s.Name}</label><input type=\"file\" class=\"form-control\" name=\"{path}\" /></div>");
                        break;
                    case ESetting.HTML:
                        sb.AppendLine($"<div class=\"mb-3\"><label class=\"form-label\">{s.Name}</label><textarea class=\"form-control richhtml\" name=\"{path}\" rows=\"5\"></textarea></div>");
                        break;
                    case ESetting.ARRAY:
                        var id = Guid.NewGuid().ToString("N");
                        sb.AppendLine($"<div class=\"mb-3\"><label class=\"form-label d-block\">{s.Name}</label><div id=\"array-{id}\" class=\"array-container\"></div><button type=\"button\" class=\"btn btn-sm btn-outline-secondary array-add\" data-target=\"array-{id}\" data-name=\"{path}\">+</button></div>");
                        sb.AppendLine($"<template id=\"tpl-{id}\">");
                        RenderSettings(s.ElementType ?? new List<SettingType>(), sb, $"{path}[__index__]");
                        sb.AppendLine("<button type=\"button\" class=\"btn btn-sm btn-outline-danger array-remove\">-</button></template>");
                        break;
                }
            }
        }
    }
}
