using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MumArchitecture.WebApp.TagHelpers
{
    [HtmlTargetElement("array-input", ParentTag = "ajax-form")]
    public class ArrayInputTagHelper : TagHelper
    {
        public string Name { get; set; } = "";
        public string Label { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var child = (await output.GetChildContentAsync()).GetContent();
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.PreContent.AppendHtml("<div class=\"column-item\">");
            output.PostContent.AppendHtml("</div>");
            output.Content.SetHtmlContent($@"
<div class=""array-input"">
    {(string.IsNullOrWhiteSpace(Label) ? "" : $"<label class=\"form-label fw-semibold mb-2\">{Label}</label>")}
    <input type=""hidden"" name=""{Name}"" class=""array-input-hidden"" value=""[]"" />
    <div class=""array-items""></div>
    <div class=""array-actions mt-2"">
        <button type=""button"" class=""btn btn-success btn-sm array-add"">+</button>
    </div>
    <template class=""array-item-template"">
        <div class=""array-item d-flex flex-wrap gap-2 align-items-start mb-3"">
            {child}
            <button type=""button"" class=""btn btn-danger btn-sm array-remove"">-</button>
        </div>
    </template>
</div>
<script>
if (!window.arrayInputHelper) {{
    window.arrayInputHelper = (function () {{
        function init(scope) {{
            (scope || document).querySelectorAll('.array-input:not([data-ai-init])').forEach(function (c) {{
                c.dataset.aiInit = '1';
                var hidden = c.querySelector('.array-input-hidden');
                var items = c.querySelector('.array-items');
                var tpl = c.querySelector('template.array-item-template');
                var addBtn = c.querySelector('.array-add');

                function collect() {{
                    var arr = [];
                    items.querySelectorAll('.array-item').forEach(function (it) {{
                        var obj = {{}};
                        it.querySelectorAll('input,select,textarea').forEach(function (inp) {{
                            var key = inp.name || inp.getAttribute('data-field') || '';
                            if (!key) return;
                            var val;
                            if (inp.type === 'checkbox') val = inp.checked;
                            else if (inp.type === 'radio') {{ if (inp.checked) val = inp.value; else return; }}
                            else val = inp.value;
                            obj[key] = val;
                        }});
                        arr.push(obj);
                    }});
                    hidden.value = JSON.stringify(arr);
                }}

                function inflate() {{
                    try {{
                        var arr = JSON.parse(hidden.value || '[]');
                        items.innerHTML = '';
                        arr.forEach(function (o) {{ addItem(o); }});
                        if (!arr.length) addItem();
                    }} catch (e) {{ addItem(); }}
                }}

                function addItem(data) {{
                    var frag = tpl.content.cloneNode(true);
                    var el = frag.querySelector('.array-item');
                    items.appendChild(frag);
                    if (data) {{
                        el.querySelectorAll('input,select,textarea').forEach(function (inp) {{
                            var key = inp.name || inp.getAttribute('data-field') || '';
                            if (!key || !(key in data)) return;
                            var v = data[key];
                            if (inp.type === 'checkbox') inp.checked = v === true || v === 'true' || v == 1;
                            else {{
                                inp.value = v;
                                if (window.jQuery && $(inp).hasClass('select2')) $(inp).trigger('change.select2');
                                if (inp.classList.contains('summernote') && window.jQuery) $(inp).summernote('code', v || '');
                            }}
                        }});
                    }}
                    hook(el);
                }}

                function hook(row) {{
                    row.querySelectorAll('input,select,textarea').forEach(function (inp) {{
                        inp.addEventListener('change', collect);
                        if (inp.classList.contains('summernote') && window.jQuery) $(inp).on('summernote.change', collect);
                    }});
                    var rm = row.querySelector('.array-remove');
                    if (rm) rm.addEventListener('click', function () {{ row.remove(); collect(); }});
                    if (window.ajaxFormHelper && ajaxFormHelper.initAjaxForms) ajaxFormHelper.initAjaxForms(row);
                }}

                addBtn.addEventListener('click', function () {{ addItem(); collect(); }});
                hidden.addEventListener('input', inflate);
                inflate();
                collect();
            }});
        }}
        document.addEventListener('DOMContentLoaded', function () {{ init(document); }});
        return {{ initArrayInputs: init }};
    }})();
}}
</script>");
        }
    }
}
