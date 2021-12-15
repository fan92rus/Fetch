using SimhashLib;
using UniversalProductScraper.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UniversalProductScraper.Graph;
using UniversalProductScraper.Loaders;

namespace UniversalProductScraper
{
    public class SortedBaseNodeTree
    {
        public SortedBaseNodeTree(ITree<BaseNode> tree, Dictionary<string, string> properties)
        {
            Properties = properties;
            Key = TableKey.Create(tree?.Parent?.Item?.Selector, tree?.Item?.Selector, Properties);
            Tree = tree;
        }

        public SortedBaseNodeTree(ITree<BaseNode> tree)
        {
            Key = TableKey.Create(tree?.Parent?.Item?.Selector, tree?.Item?.Selector, tree?.Item?.GetProperties());
            Tree = tree;
        }
        public Dictionary<string, string> Properties { get; set; }
        public ITree<BaseNode> Tree { get; set; }
        public TableKey Key { get; set; }
        public static IEnumerable<SortedBaseNodeTree> TreeMarkup(IEnumerable<ITree<BaseNode>> voidChildren) => voidChildren.Select(x => new SortedBaseNodeTree(x, x.Item.GetProperties()));

    }

    class ScrapingService
    {
        private static readonly IDictionary<Simhash, ITree<InfoNode>> maps = new Dictionary<Simhash, ITree<InfoNode>>();

        public ScrapingService(ITreeConverter converter, IWebLoader webLoader, IDomMapper domMapper, IScraper scraper)
        {
            Converter = converter;
            Mapper = domMapper;
            WebLoader = webLoader;
            Scraper = scraper;
        }

        private ITreeConverter Converter { get; }
        private IWebLoader WebLoader { get; }
        private IDomMapper Mapper { get; }
        private IScraper Scraper { get; }

        public ITree<BaseNode> ScrapPage(string url)
        {
            var doc = WebLoader.GetPage(url);

            var hash = new Simhash(Simhash.HashingType.Jenkins);
            hash.GenerateSimhash(doc.Head.InnerHtml);

            var okMaps = maps?.Where(x => x.Key.distance(hash) < 5);

            ITree<InfoNode> documentMap;

            if (okMaps.Any())
                documentMap = okMaps.Min().Value;
            else
            {
                documentMap = Mapper.ParseDocumentMap(doc);
                maps.Add(hash, documentMap);
            }

            File.WriteAllText("data\\map.json", JsonConvert.SerializeObject(documentMap, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            return Scraper.ScrapNode(documentMap);
        }
    }
}