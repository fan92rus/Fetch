using SimhashLib;
using UniversalProductScraper.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp;
using Newtonsoft.Json;
using UniversalProductScraper.Graph;
using UniversalProductScraper.Loaders;

namespace UniversalProductScraper
{

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

        public string ParsePage(string url)
        {
            return WebLoader.GetPageContent(url);
        }

        public ITree<BaseNode> ScrapPage(string url)
        {
            var doc = WebLoader.GetPage(url);

            var hash = new Simhash(Simhash.HashingType.Jenkins);
            hash.GenerateSimhash(doc.Head.InnerHtml);

            var okMaps = maps?.Where(x => x.Key.distance(hash) < 5);

            ITree<InfoNode> documentMap;

            if (okMaps.Any())
            {
                documentMap = okMaps.Min().Value;
            }
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