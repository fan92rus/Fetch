using System;
using OpenQA.Selenium;
using UniversalProductScraper.Graph;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using UniversalProductScraper.Loaders;

namespace UniversalProductScraper
{
    static class DI
    {
        public static IServiceProvider ServiceProvider { get; }
        static DI()
        {
            var chromeOprions = new ChromeOptions();
            // chromeOprions.AddArgument("--headless");

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IDomMapper, DomMapper>();
            serviceCollection.AddSingleton<IScraper, Scraper>();
            serviceCollection.AddSingleton<ITreeConverter, TreeDictionaryConverter>();
            serviceCollection.AddSingleton<IWebDriver>(_ => new ChromeDriver(chromeOprions));
            serviceCollection.AddSingleton<IWebLoader, RequestWebLoader>();
            serviceCollection.AddSingleton(typeof(ITreeBuilder<>), typeof(TreeBuilder<>));

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
    class Program
    {
        public static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var ss = DI.ServiceProvider.GetService<ScrapingService>();
            var pageData = ss.ParsePage("https://www.russianfood.com/recipes/recipe.php?rid=149690/");

            var server = new WebServer(c => c.WithUrlPrefix("http://*:5020")).WithCors().WithWebApi("/", x => x.WithController<TableResource>());
        }
    }

    class TableResource : WebApiController
    {
        // Тут стандартный Microsoft DI юзать

        private readonly ScrapingService scrapingService = DI.ServiceProvider.GetService<ScrapingService>();

        [Route(HttpVerbs.Post, "/parse/test")]
        public string AddLink([QueryField] string link) => JsonConvert.SerializeObject(scrapingService.ScrapPage(link));
    }
}
