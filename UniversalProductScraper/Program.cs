using System;

namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;
    using RestSharp;
    using ServiceStack;
    using UniversalProductScraper.Models;


    class Program
    {
        static async Task Main(string[] args)
        {
            var doc = LoadPage("https://www.sparheld.de/gutscheine/hans-natur#co=110931");

            var mapper = new DomMapper();
            var scraper = new Scraper();

            var documentMap = mapper.ParseDocumentMap(doc);

            File.WriteAllText("test.json", documentMap.ToSafeJson());
            File.WriteAllText("finalNodes.json", scraper.ScrapNode(documentMap).ToSafeJson());

            Console.WriteLine("Final!!!");
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
}
