namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EmbedIO;
    using EmbedIO.Actions;
    using EmbedIO.Cors;
    using EmbedIO.Files;
    using EmbedIO.Routing;
    using EmbedIO.Utilities;
    using EmbedIO.WebApi;

    using ServiceStack;

    using UniversalProductScraper.Models;

    class Program
    {

        public static async Task Main(string[] args)
        {
            var server = new WebServer().WithCors().WithWebApi("/", x => x.WithController<TableResource>());
            await server.RunAsync();

            while (true) await Task.Delay(1000);
        }
    }

    class TableResource : WebApiController
    {
        private readonly ScrapingService scrapingService = new ScrapingService();

        [Route(HttpVerbs.Any, "/tables")]
        public IEnumerable<Table> GetTables()
        {
            return this.scrapingService.GetTables();
        }

        [Route(HttpVerbs.Post, "/tables/add/")]
        public IEnumerable<Table> AddLink([QueryField]string link)
        {
            this.scrapingService.ScrapPage(link);
            return this.GetTables();
        }


        [Route(HttpVerbs.Post, "/tables/clear/")]
        public IEnumerable<Table> Clear()
        {
            this.scrapingService.Clear();
            return this.GetTables();
        }
    }
}
