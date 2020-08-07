namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using EmbedIO;
    using EmbedIO.Actions;
    using EmbedIO.Cors;
    using EmbedIO.Files;
    using EmbedIO.Routing;
    using EmbedIO.Utilities;
    using EmbedIO.WebApi;

    using Newtonsoft.Json;

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

        [Route(HttpVerbs.Post, "/tables/add/")]
        public string AddLink([QueryField]string link)
        {
            return JsonConvert.SerializeObject(this.scrapingService.ScrapPage(link));
        }
    }
}
