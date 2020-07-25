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

    class Program
    {
        public static List<Table> Tables { get; set; }
        public static async Task Main(string[] args)
        {
            var doc = LoadPage("http://fanserial.net/");

            var mapper = new DomMapper();
            var scraper = new Scraper();


            var documentMap = mapper.ParseDocumentMap(doc);

            File.WriteAllText("test.json", documentMap.ToSafeJson());

            var data = scraper.ScrapNode(documentMap);

            Converter converter = new Converter();
            var tables = converter.Convert(data);
            Tables = tables.Where(x => x.Properties.Any()).ToList();
            File.WriteAllText("maaped.json", JsonConvert.SerializeObject(data));
            var server = new WebServer().WithCors().WithWebApi("/", x => x.WithController<TableResource>());

            await server.RunAsync();

            while (true) await Task.Delay(1000);
        }

        private static IHtmlDocument LoadPage(string uri)
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
        [Route(HttpVerbs.Any, "/tables")]
        public List<Table> Tables()
        {
            return Program.Tables;
        }
    }
}
