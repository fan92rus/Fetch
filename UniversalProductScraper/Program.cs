using System;

namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Html;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    using MoreLinq;
    using MoreLinq.Extensions;

    using Newtonsoft.Json;

    using RestSharp;
    using ServiceStack;
    using UniversalProductScraper.Models;


    class Program
    {
        static async Task Main(string[] args)
        {
            var doc = LoadPage("http://fanserial.net/");

            var mapper = new DomMapper();
            var scraper = new Scraper();


            var documentMap = mapper.ParseDocumentMap(doc);

            File.WriteAllText("test.json", documentMap.ToSafeJson());

            var data = scraper.ScrapNode(documentMap);

            var find = data.FindAll(x => x.Type == Type.Link).ToList()
                .GroupBy(x => x.ParentNode.Selector).Select(x => new
                {
                    Parent = x.Key,
                    Nodes = x.ToList().GroupBy(e => e.Attributes.Count)
                });

            File.WriteAllText("links.json", find.ToSafeJson());
            Converter converter = new Converter();
            converter.Convert(data);

            File.WriteAllText("finalNodes.json", JsonConvert.SerializeObject(data));
            File.WriteAllText("table.json", JsonConvert.SerializeObject(converter.Objects));

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

    class Converter
    {
        public Dictionary<string, string> Names { get; set; } = new Dictionary<string, string>();
        public List<KeyValuePair<string, List<Dictionary<string, string>>>> Objects { get; set; } = new List<KeyValuePair<string, List<Dictionary<string, string>>>>();

        public void Convert(Node node) => this.ConvertNode(node, 0);

        private Dictionary<string, string> ConvertNode(Node node, int id)
        {
            var subElements = node.Nodes.Where(x => (x?.Nodes?.Any() ?? false)).ToList();
            var voidChildren = node.Nodes.Where(x => !(x?.Nodes?.Any() ?? false)).ToList();
            Console.WriteLine(id);
            if (subElements.Any())
            {
                foreach (var group in subElements.GroupBy(x => x.Selector))
                {
                    var collection = @group.Select(this.ConvertNode).Where(data => data != null).ToList();

                    this.Objects.Add(new KeyValuePair<string, List<Dictionary<string, string>>>(group.Key, collection));
                }
            }

            if (voidChildren.Any())
            {
                var collection = new Dictionary<string, string>();

                foreach (var group in voidChildren.GroupBy(x => x.Selector))
                {
                    if (group.Count() == 1)
                    {
                        foreach (var child in group)
                        {
                            var properties = this.GetProperty(child);
                            properties.Add("parent_id", id.ToString());
                            properties.Add("parent_selector", node.Selector);
                            foreach (var property in properties)
                            {
                                if (!collection.ContainsKey(property.Key) && !string.IsNullOrEmpty(property.Value))
                                    collection.Add(property.Key, property.Value);
                            }
                        }
                    }
                    else
                    {
                        var els = new List<Dictionary<string, string>>();

                        foreach (var child in group)
                        {
                            var properties = this.GetProperty(child);
                            properties.Add("parent_id", id.ToString());
                            properties.Add("parent_selector", node.Selector);
                            els.Add(properties);
                        }

                        var targetTable = this.Objects.FirstOrDefault(x => x.Key == group.Key);

                        if (targetTable.Key != null)
                            targetTable.Value.AddRange(els);
                        else
                            this.Objects.Add(new KeyValuePair<string, List<Dictionary<string, string>>>(group.Key, els));

                    }
                }


                return collection;
            }

            return null;
        }

        private Dictionary<string, string> GetProperty(Node gNode)
        {
            Dictionary<string, string> dList = new Dictionary<string, string>();

            foreach (var el in gNode.Attributes)
            {
                var key = el.Key.Replace("-", "_");
                if (!dList.ContainsKey(key))
                    dList.Add(key, el.Value);
            }

            dList.Add(gNode.Selector.Replace("-", "_"), gNode.Text);
            return dList;
        }
    }
}
