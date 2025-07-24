using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using UniversalProductScraper.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using Unity;
using UniversalProductScraper.Loaders;

namespace UniversalProductScraper
{
    static class DI
    {
        public static IUnityContainer Container { get; }
        static DI()
        {
            var chromeOprions = new ChromeOptions();
            // chromeOprions.AddArgument("--headless");

            Container = new UnityContainer();
            Container.RegisterType<IDomMapper, DomMapper>();
            Container.RegisterType<IScraper, Scraper>();
            Container.RegisterType<ITreeConverter, TreeDictionaryConverter>();
            Container.RegisterInstance<IWebDriver>(new ChromeDriver(chromeOprions));
            Container.RegisterType<IWebLoader, RequestWebLoader>();
            Container.RegisterType(typeof(ITreeBuilder<>), typeof(TreeBuilder<>));
        }
    }
    class Program
    {
        public static async Task Main(string[] args)
        {
            var ss = DI.Container.Resolve<ScrapingService>();
            var pageData = ss.ScrapPage("https://www.russianfood.com/recipes/recipe.php?rid=149690/");
            var prices = pageData.Find(x => x.Selector.Contains("avail"));

            //var server = new WebServer().WithCors().WithWebApi("/", x => x.WithController<TableResource>());
            //await server.RunAsync();
        }
    }

    class TableResource : WebApiController
    {
        // Тут стандартный Microsoft DI юзать

        private readonly ScrapingService scrapingService = DI.Container.Resolve<ScrapingService>();

        [Route(HttpVerbs.Post, "/parse/test")]
        public string AddLink([QueryField] string link) => JsonConvert.SerializeObject(scrapingService.ScrapPage(link));
    }
}
