using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using UniversalProductScraper.Graph;
using UniversalProductScraper.Models;

namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EmbedIO;
    using EmbedIO.Routing;
    using EmbedIO.WebApi;
    using Newtonsoft.Json;
    using Unity;
    using UniversalProductScraper.Loaders;

    static class DI
    {
        public static IUnityContainer Container { get; }
        static DI()
        {
            Container = new UnityContainer();
            Container.RegisterType<IDomMapper, DomMapper>();
            Container.RegisterType<IScraper, Scraper>();
            Container.RegisterType<ITreeConverter, TreeConverter>();
            Container.RegisterInstance<IWebDriver>(new FirefoxDriver());
            Container.RegisterType<IWebLoader, SeleniumLoader>();
            Container.RegisterType(typeof(ITreeBuilder<>), typeof(TreeBuilder<>));
        }
    }
    class Program
    {
        public static async Task Main(string[] args)
        {
            var ss = DI.Container.Resolve<ScrapingService>();
            var pageData = ss.ScrapPage("https://www.dns-shop.ru/product/940ce0cb7d702ff0/videokarta-powercolor-amd-radeon-rx-6700-xt-red-devil-axrx-6700xt-12gbd6-3dheoc/");
            var prices = pageData.Find(x => x.Selector.Contains("avail"));

            //var server = new WebServer().WithCors().WithWebApi("/", x => x.WithController<TableResource>());
            //await server.RunAsync();
        }
    }

    class TableResource : WebApiController
    {
        private readonly ScrapingService scrapingService = DI.Container.Resolve<ScrapingService>();

        [Route(HttpVerbs.Post, "/tables/add/")]
        public string AddLink([QueryField] string link) => JsonConvert.SerializeObject(scrapingService.ScrapPage(link));
    }

    class DataStructure
    {
        public DataStructure(string property)
        {
            Property = property;
        }

        public DataStructure()
        { }

        public string Property { get; set; }
        public ICollection<DataStructure> Children { get; set; }
    }
}
