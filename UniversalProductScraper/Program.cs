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
            #region unical
            //var node = GetNode("https://www.sparheld.de/gutscheine/discountlens");
            //File.WriteAllText("data\\node.json", JsonConvert.SerializeObject(node, new JsonSerializerSettings()
            //{
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //}));
            //var tables = converter.Convert(node);
            //File.WriteAllText("data\\tables.json", JsonConvert.SerializeObject(tables));

            //return;
            //Console.WriteLine();

            //var firstGraph = testGraphConverter.Test();
            //var secondGraph = testGraphConverter.Test(GetNode("https://www.sparheld.de/gutscheine/levis"));
            //ar testData = firstGraph.Vertices.Where(x => x.Selector == "div.voucherCard.box.voucherCard--default");
            //var unical = secondGraph.Vertices.Where(x => firstGraph.Vertices.All(a => !a.Equals(x))).Select(x => x.ParentNode).Distinct().ToList().GroupBy(x => x.Selector);
            #endregion

            var server = new WebServer().WithCors().WithWebApi("/", x => x.WithController<TableResource>());
            await server.RunAsync();

            while (true) await Task.Delay(1000);
        }

        private static ITree<BaseNode> GetNode(string url)
        {
            ScrapingService scrapingService = new ScrapingService();
            var doc = scrapingService.LoadPage(url);
            return new Scraper().ScrapNode(new DomMapper().ParseDocumentMap(doc));
        }
    }

    class TableResource : WebApiController
    {
        private readonly ScrapingService scrapingService = new ScrapingService();


        [Route(HttpVerbs.Post, "/tables/add/")]
        public IEnumerable<Table> AddLink([QueryField]string link) => this.scrapingService.ScrapPage(link);

        [Route(HttpVerbs.Post, "/tables/clear/")]
        public IEnumerable<Table> Clear()
        {
            this.scrapingService.Clear();
            return new List<Table>();
        }
    }
}
