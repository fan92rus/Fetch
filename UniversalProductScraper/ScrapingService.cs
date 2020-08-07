namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;

    using Unity;

    using UniversalProductScraper.Graph;
    using UniversalProductScraper.Loaders;

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
        [Dependency]
        public TreeConverter Converter { get; set; }

        [Dependency]
        public IWebLoader WebLoader { get; set; }

        public ExpandoObject ScrapPage(string url)
        {
            var mapper = new DomMapper();

            var doc = this.WebLoader.LoadPageFromString(this.WebLoader.GetPageContent(url));
            var documentMap = mapper.ParseDocumentMap(doc);
            File.WriteAllText("data\\map.json", JsonConvert.SerializeObject(documentMap, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            var data = new Scraper().ScrapNode(documentMap);
            //File.WriteAllText("data\\data.json", JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            var objects = this.Converter.Convert(data);
            return (ExpandoObject)objects;
        }

    }
}