namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using AngleSharp;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    using Boilerpipe.Net.Extractors;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using RestSharp;

    using ServiceStack;

    using Swan.Formatters;

    using UniversalProductScraper.Graph;

    class ScrapingService
    {
        private TreeConverter converter = new TreeConverter();


        public void Clear()
        {
            //this.converter = new Converter();
        }

        public JObject ScrapPage(string url)
        {
            var page = this.RequestPage(url).Content;
            var doc = this.LoadPage(page);
            var mapper = new DomMapper();
            var documentMap = mapper.ParseDocumentMap(doc);
            var data = new Scraper().ScrapNode(documentMap);

            var text = CommonExtractors.ArticleExtractor.GetText(page);
            var objects = this.converter.Convert(data);
            return objects;
        }

        public IHtmlDocument LoadPage(string text)
        {
            var config = Configuration.Default.WithDefaultLoader().WithCss().WithJs();
            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var doc = parser.ParseDocument(text);

            return doc;
        }

        public IRestResponse RequestPage(string uri)
        {
            var isCreate = Uri.TryCreate(uri, UriKind.Absolute, out var target);
            if (!isCreate)
                throw new ArgumentException("uri is invalid");
            var rc = new RestClient();
            var req = new RestRequest(target);
            req.AddHeader("Content-Type", "text/html; charset=utf-8");
            var resp = rc.Execute(req);
            return resp;
        }
    }
}