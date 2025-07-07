using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MumArchitecture.WebApp.TagHelpers
{
    [HtmlTargetElement("ajax-form", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AjaxFormTagHelper : TagHelper
    {
        private readonly IAntiforgery _antiforgery;
        private readonly IHttpContextAccessor _contextAccessor;

        public AjaxFormTagHelper(IAntiforgery antiforgery, IHttpContextAccessor contextAccessor)
        {
            _antiforgery = antiforgery;
            _contextAccessor = contextAccessor;
        }

        public string Method { get; set; } = "POST";
        public string Action { get; set; } = "";
        public string OnSubmit { get; set; } = "";
        public bool UseAntiForgeryToken { get; set; } = true;
        public string GetUrl { get; set; } = "";
        public int Columns { get; set; } = 1;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
           
            output.TagName = "form";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "ajax-form");
            output.Attributes.SetAttribute("method", Method);
            output.Attributes.SetAttribute("action", Action);
            if (!string.IsNullOrWhiteSpace(OnSubmit)) output.Attributes.SetAttribute("data-onsubmit", OnSubmit);
            if (!string.IsNullOrWhiteSpace(GetUrl)) output.Attributes.SetAttribute("data-get-url", GetUrl);
            if (UseAntiForgeryToken)
            {
                var tokens = _antiforgery.GetAndStoreTokens(_contextAccessor.HttpContext!);
                output.PreContent.AppendHtml($"<input type=\"hidden\" name=\"{tokens.FormFieldName}\" value=\"{tokens.RequestToken}\" />");
            }
            //var colWidth = 12 / (Columns < 1 ? 1 : Columns);
            //context.Items["ColumnWidth"] = colWidth;
            context.Items["Columns"] = Columns;

            // Tüm alanları tek .row içine yerleştir
            output.PreContent.AppendHtml($@"<div class=""form-columns"" style=""column-count:{Columns};"">");
            output.PostContent.AppendHtml("</div>");
            output.PostContent.AppendHtml(@"<div class=""mt-4 d-flex justify-content-end gap-2"">
        <button type=""reset"" class=""btn btn-secondary"">Temizle</button>
        <button type=""submit"" class=""btn btn-primary"">Kaydet</button>
    </div>");
        }
    }
}
