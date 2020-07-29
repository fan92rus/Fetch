namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using EmbedIO;
    using EmbedIO.Actions;
    using EmbedIO.Cors;
    using EmbedIO.Files;
    using EmbedIO.Routing;
    using EmbedIO.Utilities;
    using EmbedIO.WebApi;

    using Newtonsoft.Json;

    using QuickGraph;
    using QuickGraph.Algorithms;

    using ServiceStack;

    using UniversalProductScraper.Graph;
    using UniversalProductScraper.Models;

    class Program
    {

        public static async Task Main(string[] args)
        {
            TestGraphConverter testGraphConverter = new TestGraphConverter();
            Converter converter = new Converter();

            var firstGraph = testGraphConverter.Test(GetNode("https://www.sparheld.de/gutscheine/discountlens"));
            //var secondGraph = testGraphConverter.Test(GetNode("https://www.sparheld.de/gutscheine/levis"));
            var testData = firstGraph.Vertices.Where(x => x.Selector == "div.voucherCard.box.voucherCard--default");

            //var unical = secondGraph.Vertices.Where(x => firstGraph.Vertices.All(a => !a.Equals(x))).Select(x => x.ParentNode).Distinct().ToList().GroupBy(x => x.Selector);
            Console.WriteLine();

            //File.WriteAllText("unicalElements.json", JsonConvert.SerializeObject(unical, Formatting.Indented, new JsonSerializerSettings()
            //{
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //}));


            var server = new WebServer().WithCors().WithWebApi("/", x => x.WithController<TableResource>());
            await server.RunAsync();

            while (true) await Task.Delay(1000);
        }

        private static Node GetNode(string url)
        {
            ScrapingService scrapingService = new ScrapingService();

            var mapper = new DomMapper();
            var doc = scrapingService.LoadPage(url);
            var documentMap = mapper.ParseDocumentMap(doc);
            var data = new Scraper().ScrapNode(documentMap);
            return data;
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
