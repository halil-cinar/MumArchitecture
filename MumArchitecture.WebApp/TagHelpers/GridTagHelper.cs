using System.Text;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MumArchitecture.Web.TagHelpers
{
    [HtmlTargetElement("grid")]
    public class GridTagHelper : TagHelper
    {
        private const string UrlAttr = "url";
        private const string IdAttr = "id";
        private const string ClassAttr = "class";

        [HtmlAttributeName(UrlAttr)]
        public string Url { get; set; }

        [HtmlAttributeName(IdAttr)]
        public string Id { get; set; }

        [HtmlAttributeName(ClassAttr)]
        public string TableClass { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // sütun ve filtre tanımlarını saklayacak koleksiyonları başlat
            context.Items["grid_columns"] = new List<ColumnDefinition>();
            context.Items["grid_rowbuttons"] = new List<string>();
            context.Items["grid_gridbuttons"] = new List<string>();
            context.Items["grid_taghelper"] = this;
            // içeriği al (column ve filters tag’leri burada toplanacak)
            var child = await output.GetChildContentAsync();
            var inner = child.GetContent();

            // wrapper div
            var sb = new StringBuilder();
            sb.AppendLine(inner);

            // tablo başlığı
            sb.AppendLine($"<div class=\"table-responsive mt-4\">");
            sb.AppendLine($"<table id=\"{Id}\" class=\"{TableClass}\"><thead><tr>");
            var cols = (List<ColumnDefinition>)context.Items["grid_columns"];
            foreach (var c in cols)
            {
                var title = c.Title ?? c.For ?? c.Field;
                var style = !string.IsNullOrEmpty(c.Style) ? $" style=\"{c.Style}\"" : "";
                sb.AppendLine($"<th{style}>{title}</th>");
            }
            sb.AppendLine("</tr></thead><tbody></tbody></table></div>");

            // pagination
            sb.AppendLine($"<nav aria-label=\"Sayfalar\"><ul class=\"pagination justify-content-center\" id=\"{Id}_pagination\"></ul></nav>");
            
            // col fors
            var colfors=cols.Select(c=>c.For??c.Field).ToList();
            sb.AppendLine($"<div id=\"{Id}_colfors\" style=\"display:none\" > {string.Join(";", colfors)} </div>");


            //rowbuttons
            var rowButtons = (List<string>)context.Items["grid_rowbuttons"];
            if (rowButtons.Count > 0)
            {
                sb.AppendLine($"<div id=\"{Id}_rowbuttons\" style=\"display:none\" > {string.Join("",rowButtons)} </div>");
            }
            // Grid init script
            sb.AppendLine("<script>");
            sb.AppendLine($"  Grid.init('#{Id}', '{Url}');");
            

            sb.AppendLine("</script>");

            // çıktıyı render et
            output.TagName = null;
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
    public class ColumnDefinition
    {
        public string For { get; set; }
        public string Field { get; set; }
        public bool Orderable { get; set; } = true;
        public string Template { get; set; }
        public string Title { get; set; }
        public string Style { get; set; }
    }
    public class RowButtonDefinition
    {
        public string Id { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Class { get; set; }
        public string OnClick { get; set; }
        public string HREF { get; set; }
    }
}
