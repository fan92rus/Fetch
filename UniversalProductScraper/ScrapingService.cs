using System.Security.Cryptography.X509Certificates;
using SimhashLib;
using UniversalProductScraper.Models;

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
        private static IDictionary<Simhash, ITree<InfoNode>> maps = new Dictionary<Simhash, ITree<InfoNode>>();

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
            var doc = this.WebLoader.GetPage(url);

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