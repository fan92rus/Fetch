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
            Container.RegisterType<IWebLoader, RequestWebLoader>(new ContainerControlledLifetimeManager());
            Container.RegisterType<TreeConverter>(new ContainerControlledLifetimeManager());

        }
    }
    class Program
    {
        public static async Task Main(string[] args)
        {
            DataStructure structure = new DataStructure("main")
                                          {
                                              Children = new List<DataStructure>()
                                                             {
                                                                 new DataStructure("film")
                                                                     {
                                                                         Children = new List<DataStructure>()
                                                                                        {
                                                                                            new DataStructure(
                                                                                                "Raiting"),
                                                                                            new DataStructure("Reviews")
                                                                                        }
                                                                     },
                                                                 new DataStructure("menu")
                                                             }
                                          };

            var json = JsonConvert.SerializeObject(structure);

            var server = new WebServer().WithCors().WithWebApi("/", x => x.WithController<TableResource>());
            await server.RunAsync();
        }
    }

    class TableResource : WebApiController
    {
        private readonly ScrapingService scrapingService = DI.Container.Resolve<ScrapingService>();

        [Route(HttpVerbs.Post, "/tables/add/")]
        public string AddLink([QueryField]string link)
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
