using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MumArchitecture.Web.TagHelpers
{
    [HtmlTargetElement("column", ParentTag = "grid")]
    public class ColumnTagHelper : TagHelper
    {
        public string For { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public bool Orderable { get; set; } = true;
        public string Template { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var cols = (List<ColumnDefinition>)context.Items["grid_columns"];
            cols.Add(new ColumnDefinition
            {
                For = For,
                Field = Field,
                Orderable = Orderable,
                Template = Template,
                Title = Title,
                Style = Style
            });
            output.SuppressOutput();
        }
    }
}
