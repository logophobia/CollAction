﻿using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using CollAction.Services.Sitemap;

namespace CollAction.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISitemapService sitemapService;
        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger, ISitemapService sitemapService)
        {
            this.sitemapService = sitemapService;
            this.logger = logger;
        }

        public IActionResult Index()
            => View();

        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            logger.LogError(exceptionHandlerPathFeature.Error, "An error has occurred at: {0}", exceptionHandlerPathFeature.Path);
            return View();
        }

        public ContentResult Robots()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.Append("Sitemap: https://");
            sb.Append(Url.ActionContext.HttpContext.Request.Host);
            sb.AppendLine(Url.Action("Sitemap"));
            sb.AppendLine("Disallow: /Admin/");
            sb.AppendLine("Disallow: /Account/");
            sb.AppendLine("Disallow: /Manage/");
            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }

        public async Task<ContentResult> Sitemap()
            => Content((await sitemapService.GetSitemap()).ToString(), "text/xml", Encoding.UTF8);
    }
}
