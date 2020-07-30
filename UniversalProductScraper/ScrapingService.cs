namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using AngleSharp;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    using Newtonsoft.Json;

    using RestSharp;

    using UniversalProductScraper.Graph;

    class ScrapingService
    {
        private Converter converter = new Converter();


        public void Clear()
        {
            this.converter = new Converter();
        }

        public IEnumerable<Table> ScrapPage(string url)
        {
            var doc = this.LoadPage(url);
            var mapper = new DomMapper();
            var documentMap = mapper.ParseDocumentMap(doc);
            var data = new Scraper().ScrapNode(documentMap);

            File.WriteAllText("data\\node.json", JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            this.converter.Convert(data);
            return this.converter.GetTables();
        }

        public IHtmlDocument LoadPage(string uri)
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