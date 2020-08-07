namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using UniversalProductScraper.Graph;
    using UniversalProductScraper.Models;
    using Node = Models.Node;

    class Scraper
    {
        public ITree<BaseNode> ScrapNode(ITree<InfoNode> info)
        {
            ITree<BaseNode> tree = new Tree<BaseNode>(this.SetNode(info.Item));

            foreach (var child in info.Children)
            {
                if (child.Item.Type == DomNodeType.Container)
                {
                    var childrenAll = info.Item.Element.QuerySelectorAll(child.Item.Selector);

                    foreach (var c in childrenAll)
                    {
                        var element = this.ScrapNode(new Tree<InfoNode>(child)
                        {
                            Item = new InfoNode(child.Item)
                            {
                                Element = c,
                            },
                        });

                        if (element != null && !tree.Contains(element) && (element.Children?.Any() ?? false || (element.Item?.Attributes?.Any() ?? false)))
                            tree.Add(element);
                    }

                    continue;
                }

                var children = info.Item.Element.QuerySelectorAll(child.Item.Selector);

                foreach (var element in children)
                {
                    var el = this.SetNode(child.Item, element);
                    if (el?.Text != null || (el?.Attributes?.Any() ?? false) && tree.Children.All(e => e.Item.Selector != el.Selector))
                        tree.Add(el);
                }
            }

            if (tree.Item?.Text != null || (tree?.Item?.Attributes?.Any() ?? false) || (tree?.Children?.Any() ?? false))
                return tree;

            return null;
        }

        private BaseNode SetNode(InfoNode child) => this.SetNode(child, child.Element);

        private BaseNode SetNode(InfoNode child, IElement element)
        {
            var n = new BaseNode()
            {
                Selector = child.Selector,
                Attributes = element.Attributes.Where(x => x.Name != "class").Where(x => x.Name != "d" && !string.IsNullOrEmpty(x.Name))
                                    .Select(x => new KeyValuePair<string, string>(x.Name, x.Value)).ToList(),
                Type = child.Type,
            };
            Console.WriteLine("SCRAP  - " + n.Selector);

            var flags = (int)child.Element.Flags;

            var text = element.Text();

            try
            {
                if (element is IHtmlScriptElement && element.InnerHtml != null)
                    n.Text = Regex.Unescape(HttpUtility.HtmlDecode(HttpUtility.UrlDecode(element.InnerHtml)));
                else if (!string.IsNullOrEmpty(text) && (!element.Children.Any() || element?.Children?.Count(x => string.IsNullOrEmpty(x.Text())) / element?.Children?.Length > 0.8))
                    n.Text = Regex.Unescape(text).RemoveSpaces();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            return n;
        }

        private bool CheckTextElements(IElement x) => (x.NodeType == NodeType.Text || this.CheckFlags((int)x.Flags)) && !string.IsNullOrEmpty(x.TextContent.RemoveSpaces());

        private bool CheckFlags(int flag) => flag <= 280 && flag >= 250 || flag >= 306 && flag <= 340 || (flag >= 2304 && flag <= 2340);
    }


}