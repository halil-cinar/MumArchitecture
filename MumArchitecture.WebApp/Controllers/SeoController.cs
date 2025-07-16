using Microsoft.AspNetCore.Mvc;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Domain.ListDtos;
using System.Text;
using System.Xml.Linq;

namespace MumArchitecture.WebApp.Controllers
{
    [ApiController]
    public class SeoController : Controller
    {
        private readonly IMenuService _menuService;

        public SeoController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet("/sitemap.xml")]
        public async Task<IActionResult> Index()
        {
            var menus = await _menuService.GetAll(new Business.Result.Filter<Domain.Entities.Menu>().
                AddFilter(x => x.IsActive && x.IsVisible&&x.Area==Domain.Enums.EArea.Main));
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var urlset = new XElement("urlset",
                new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"));

            foreach (var menu in menus.Data?.OrderBy(m => m.DisplayOrder).ToList()??new List<MenuListDto>())
            {
                var loc = menu.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? menu.Url
                    : $"{baseUrl}/{menu.Url.TrimStart('/')}";
                urlset.Add(new XElement("url", new XElement("loc", loc)));
            }

            var doc = new XDocument(urlset);
            return Content(doc.ToString(SaveOptions.DisableFormatting), "application/xml", Encoding.UTF8);
        }

        [HttpGet("/robots.txt")]
        public IActionResult Robots()
        {
            var host = $"{Request.Scheme}://{Request.Host}";
            var sb = new StringBuilder()
                .AppendLine("User-agent: *")
                .AppendLine("Disallow: /admin/")
                .AppendLine("Disallow: /api/")
                .AppendLine($"Sitemap: {host}/sitemap.xml");

            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }


       
    }
}
