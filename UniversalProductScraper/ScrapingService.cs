namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Net;

    using AngleSharp;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    using Boilerpipe.Net.Extractors;

    using Extentions.RestSharp;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Polly;
    using Polly.Retry;

    using RestSharp;

    using ServiceStack;

    using Swan.Formatters;

    using UniversalProductScraper.Graph;

    public class SortedBaseNodeTree
    {
        public SortedBaseNodeTree(ITree<BaseNode> tree, Dictionary<string, string> properties)
        {
            this.Properties = properties;
            this.Key = TableKey.Create(tree?.Parent?.Item?.Selector, tree?.Item?.Selector, this.Properties);
            this.Tree = tree;
        }

        public SortedBaseNodeTree(ITree<BaseNode> tree)
        {
            this.Key = TableKey.Create(tree?.Parent?.Item?.Selector, tree?.Item?.Selector, tree?.Item?.GetProperties());
            this.Tree = tree;
        }
        public Dictionary<string, string> Properties { get; set; }
        public ITree<BaseNode> Tree { get; set; }
        public TableKey Key { get; set; }
        public static IEnumerable<SortedBaseNodeTree> TreeMarkup(IEnumerable<ITree<BaseNode>> voidChildren) => voidChildren.Select(x => new SortedBaseNodeTree(x, x.Item.GetProperties()));

    }
    class ScrapingService
    {
        private TreeConverter converter = new TreeConverter();

        public ScrapingService()
        {
            this.Policy = Polly.Policy
                .HandleResult<IRestResponse>(
                    (response) => (response.StatusCode == 0 || response.StatusCode == HttpStatusCode.TooManyRequests)
                                  && response.ResponseStatus != ResponseStatus.TimedOut).WaitAndRetry(
                    2,
                    retryAttempt => TimeSpan.FromSeconds(2));

        }

        public RetryPolicy<IRestResponse> Policy { get; set; }

        public void Clear()
        {
            //this.converter = new Converter();
        }

        public ExpandoObject ScrapPage(string url)
        {
            var page = this.RequestPage(url).Content;
            var doc = this.LoadPage(page);
            var mapper = new DomMapper();
            var documentMap = mapper.ParseDocumentMap(doc);
            File.WriteAllText("data\\map.json", JsonConvert.SerializeObject(documentMap, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            var data = new Scraper().ScrapNode(documentMap);

            this.Trees = data.Select(x => new SortedBaseNodeTree(x));

            this.PreProcessingScrapedData();

            var text = CommonExtractors.ArticleExtractor.GetText(page);
            var objects = this.converter.Convert(data);
            return (ExpandoObject)objects;
        }

        public IEnumerable<SortedBaseNodeTree> Trees { get; set; }



        public void PreProcessingScrapedData()
        {
            var groupedTrees = this.Trees.GroupBy(x => x.Key);
            foreach (var groupedTree in groupedTrees)
            {
                if (groupedTree.Key.ToString()?.Contains("1658942392") ?? false)
                {

                }

                if (groupedTree.All(x => x.Tree.Children.Count == 0))
                {

                }

                if (groupedTree.All(x => x.Tree.Children.Count == 1))
                {
                    foreach (var tree in groupedTree)
                    {
                        var child = tree?.Tree?.Children?.FirstOrDefault();
                        if (child?.Parent != null)
                        {
                            child.Parent.Children = child.Children;
                            child.Parent.Item = child.Item;
                        }
                    }
                }
            }
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
            req.AddHeader("Content-InfoNodeType", "text/html; charset=utf-8");
            var resp = rc.ExecuteWitHeaders(req, this.Policy);
            return resp;
        }
    }
}