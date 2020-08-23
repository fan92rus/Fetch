using SimhashLib;
using UniversalProductScraper.Graph;

namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EmbedIO;
    using EmbedIO.Routing;
    using EmbedIO.WebApi;
    using Newtonsoft.Json;
    using Unity;
    using Unity.Lifetime;
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
            Container.RegisterType<IWebLoader, RequestWebLoader>();
            Container.RegisterType(typeof(ITreeBuilder<>), typeof(TreeBuilder<>));
        }
    }
    class Program
    {
        public static async Task Main(string[] args)
        {
            var server = new WebServer().WithCors().WithWebApi("/", x => x.WithController<TableResource>());
            await server.RunAsync();
        }
    }

    class TableResource : WebApiController
    {
        private readonly ScrapingService scrapingService = DI.Container.Resolve<ScrapingService>();

        [Route(HttpVerbs.Post, "/tables/add/")]
        public string AddLink([QueryField] string link)
        {
            return JsonConvert.SerializeObject(this.scrapingService.ScrapPage(link));
        }
    }

    class DataStructure
    {
        public DataStructure(string property) => this.Property = property;
        public DataStructure()
        { }

        public string Property { get; set; }
        public ICollection<DataStructure> Children { get; set; }
    }
}
