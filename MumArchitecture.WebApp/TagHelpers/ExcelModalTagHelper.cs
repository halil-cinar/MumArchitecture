using Microsoft.AspNetCore.Razor.TagHelpers;
using MumArchitecture.Domain;
using System;
using System.Text;

namespace MumArchitecture.WebApp.TagHelpers
{
    [HtmlTargetElement("excel-modal")]
    public class ExcelModalTagHelper : TagHelper
    {
        public string ModalId { get; set; }
        public string DownloadUrl { get; set; }
        public string UploadUrl { get; set; }
        public string SampleUrl { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var id = string.IsNullOrWhiteSpace(ModalId) ? $"excelModal_{Guid.NewGuid():N}" : ModalId;
            var hasDownload = !string.IsNullOrWhiteSpace(DownloadUrl);
            var hasUpload = !string.IsNullOrWhiteSpace(UploadUrl);

            var sb = new StringBuilder();

            sb.AppendLine($@"<div class=""modal fade"" id=""{id}_modal"" tabindex=""-1"" aria-hidden=""true""><div class=""modal-dialog modal-dialog-centered modal-lg""><div class=""modal-content"">");
            sb.AppendLine($@"<div class=""modal-header""><h5 class=""modal-title"">Excel İşlemleri</h5><button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button></div>");
            sb.AppendLine($@"<style>#{id}_modal .modal-dialog{{max-width:800px;height:90vh}}#{id}_modal .modal-content{{height:100%}}#{id}_modal .modal-body{{flex:1 1 auto;overflow-y:auto}}#{id}_modal .modal-body table thead{{background:linear-gradient(to right,#ff9900,#ffae42);color:#fff}}</style>");
            sb.AppendLine(@"<div class=""modal-body"">");

            if (hasDownload || hasUpload)
            {
                sb.AppendLine(@"<ul class=""nav nav-tabs nav-fill"" role=""tablist"">");
                if (hasDownload)
                    sb.AppendLine($@"<li class=""nav-item"" role=""presentation""><button class=""nav-link active"" id=""{id}_download_tab"" data-bs-toggle=""tab"" data-bs-target=""#{id}_download"" type=""button"" role=""tab"">Excel İndir</button></li>");
                if (hasUpload)
                    sb.AppendLine($@"<li class=""nav-item"" role=""presentation""><button class=""nav-link {(hasDownload ? "" : "active")}"" id=""{id}_upload_tab"" data-bs-toggle=""tab"" data-bs-target=""#{id}_upload"" type=""button"" role=""tab"">Excel Yükle</button></li>");
                sb.AppendLine("</ul>");
            }

            sb.AppendLine(@"<div class=""tab-content pt-3"">");

            if (hasDownload)
                sb.AppendLine($@"<div class=""tab-pane fade show active"" id=""{id}_download"" role=""tabpanel""><button id=""{id}_downloadBtn"" class=""btn btn-primary"">İndir</button></div>");

            if (hasUpload)
            {
                sb.AppendLine($@"<div class=""tab-pane fade {(hasDownload ? "" : "show active")}"" id=""{id}_upload"" role=""tabpanel"">");
                sb.AppendLine($@"<div class=""d-flex mb-3""><input id=""{id}_fileInput"" type=""file"" accept="".xlsx,.xls"" class=""form-control"">");
                if (!string.IsNullOrWhiteSpace(SampleUrl))
                    sb.AppendLine($@"<button id=""{id}_sampleBtn"" class=""btn btn-outline-primary ms-2"">Örnek Dosya İndir</button>");
                sb.AppendLine("</div>");
                sb.AppendLine($@"<div id=""{id}_preview"" class=""table-responsive mb-3""></div>");
                sb.AppendLine($@"<div class=""d-flex gap-2""><button id=""{id}_confirmBtn"" class=""btn btn-primary"" disabled>Onayla ve Devam Et</button><button class=""btn btn-outline-secondary"" data-bs-dismiss=""modal"">Vazgeç</button></div>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div></div></div></div>");

            sb.AppendLine(@"<script>(function(){");
            if (hasDownload)
                sb.AppendLine($@"var d=document.getElementById('{id}_downloadBtn');if(d)d.addEventListener('click',function(){{window.location.href='{DownloadUrl}';}});");
            if (!string.IsNullOrWhiteSpace(SampleUrl))
                sb.AppendLine($@"var s=document.getElementById('{id}_sampleBtn');if(s)s.addEventListener('click',function(){{window.location.href='{SampleUrl}';}});");
            if (hasUpload)
            {
                sb.AppendLine($@"var i=document.getElementById('{id}_fileInput');var p=document.getElementById('{id}_preview');var c=document.getElementById('{id}_confirmBtn');");
                sb.AppendLine(@"if(i){i.addEventListener('change',async function(e){var f=e.target.files[0];if(!f)return;var a=await f.arrayBuffer();var w=XLSX.read(a);var sh=w.Sheets[w.SheetNames[0]];var r=XLSX.utils.sheet_to_json(sh,{header:1});var h='<table class=""table table-bordered table-sm""><thead><tr>';r[0].forEach(function(t){h+='<th>'+t+'</th>';});h+='</tr></thead><tbody>';r.slice(1).forEach(function(rr){h+='<tr>';rr.forEach(function(x){h+='<td>'+(x??'')+'</td>';});h+='</tr>';});h+='</tbody></table>';p.innerHTML=h;c.disabled=false;});}");
                sb.AppendLine($@"if(c){{c.addEventListener('click',async function(){{var f=i.files[0];if(!f)return;var fd=new FormData();fd.append('file',f);var res=await fetch('{UploadUrl}',{{method:'POST',body:fd}});if(!res.ok)return;var j=await res.json();var d=j.Data||j.data||j;var b=d.File||d.file;var n=d.Name||d.name||'result.xlsx';var t=d.ContentType||d.contentType||'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';if(Array.isArray(b)){{b=new Uint8Array(b);}}else{{b=Uint8Array.from(atob(b),function(x){{return x.charCodeAt(0);}});}}var w2=XLSX.read(b.buffer,{{type:'array'}});var sh2=w2.Sheets[w2.SheetNames[0]];var r2=XLSX.utils.sheet_to_json(sh2,{{header:1}});var h='<a class=""btn btn-success mb-2"" download=""'+n+'"" href=""'+URL.createObjectURL(new Blob([b],{{type:t}}))+'"">{Lang.Value("Sonuç Dosyasını İndir")}</a>';h+='<table class=""table table-bordered table-sm""><thead><tr>';r2[0].forEach(function(t){{h+='<th>'+t+'</th>';}});h+='</tr></thead><tbody>';r2.slice(1).forEach(function(rr){{h+='<tr>';rr.forEach(function(x){{h+='<td>'+(x??'')+'</td>';}});h+='</tr>';}});h+='</tbody></table>';p.innerHTML=h;c.disabled=true;}});}}");
            }
            sb.AppendLine("})();</script>");

            output.TagName = null;
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
