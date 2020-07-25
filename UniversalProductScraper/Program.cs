using System;

namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    using EmbedIO;
    using EmbedIO.Actions;
    using EmbedIO.Cors;
    using EmbedIO.Files;
    using EmbedIO.Routing;
    using EmbedIO.Utilities;
    using EmbedIO.WebApi;

    using Newtonsoft.Json;
    using RestSharp;
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

    class ScrapingService
    {
        public readonly Converter Converter = new Converter();
        public void ScrapPage(string url)
        {
            var doc = this.LoadPage(url);
            var mapper = new DomMapper();
            var documentMap = mapper.ParseDocumentMap(doc);
            var data = new Scraper().ScrapNode(documentMap);
            File.WriteAllText("testDAta.txt", JsonConvert.SerializeObject(data));
            this.Converter.Convert(data);
        }

        private IHtmlDocument LoadPage(string uri)
        {
            var isCreate = Uri.TryCreate(uri, UriKind.Absolute, out var target);
            if (!isCreate)
                throw new ArgumentException("uri is invalid");

            var rc = new RestClient();

            var resp = rc.Execute(new RestRequest(target));

            var config = Configuration.Default.WithDefaultLoader().WithCss().WithJs();

            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var doc = parser.ParseDocument(resp.Content);

            return doc;
        }
    }

    class TableResource : WebApiController
    {
        private readonly ScrapingService scrapingService = new ScrapingService();

        [Route(HttpVerbs.Any, "/tables")]
        public IEnumerable<Table> GetTables()
        {
            return this.scrapingService.Converter.GetTables();
        }

        [Route(HttpVerbs.Post, "/tables/add/")]
        public IEnumerable<Table> AddLink([QueryField]string link)
        {
            this.scrapingService.ScrapPage(link);
            return this.GetTables();
        }
    }
}
