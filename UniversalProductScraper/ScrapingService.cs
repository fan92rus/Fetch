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
        public ScrapingService(ITreeConverter converter, IWebLoader webLoader, IDomMapper domMapper, IScraper scraper)
        {
            this.Converter = converter;
            this.Mapper = domMapper;
            this.WebLoader = webLoader;
            this.Scraper = scraper;
        }

        private ITreeConverter Converter { get; }
        private IWebLoader WebLoader { get; }
        private IDomMapper Mapper { get; }
        private IScraper Scraper { get; }

        public IDictionary<string, object> ScrapPage(string url)
        {
            var doc = this.WebLoader.LoadPageFromString(this.WebLoader.GetPageContent(url));
            var documentMap = Mapper.ParseDocumentMap(doc);
            var data = Scraper.ScrapNode(documentMap);
            var objects = this.Converter.Convert(data);
            File.WriteAllText("data\\res.json", JsonConvert.SerializeObject(objects, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
            return objects;
        }

    }
}