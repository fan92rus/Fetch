namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;

    using UniversalProductScraper.Graph;
    using UniversalProductScraper.Models;
    using Node = Models.Node;

    class Scraper
    {
        public Node ScrapNode(ITree<MapperNode> info, Node parent = null)
        {
            var final = SetNode(info.Item, info.Item.Element, parent);

            final.Nodes = new List<Node>();

            foreach (var child in info.Children)
            {
                if (child.Item.Type == Type.Container)
                {
                    var childrenAll = info.Item.Element.QuerySelectorAll(child.Item.Selector);

                    foreach (var c in childrenAll)
                    {
                        child.Item.Element = c;
                        final.Nodes.Add(this.ScrapNode(child, final));
                    }

                    continue;
                }

                var children = info.Item.Element.QuerySelectorAll(child.Item.Selector);

                foreach (var element in children)
                {
                    var node = SetNode(child.Item, element, final);
                    final.Nodes.Add(node);
                }
            }

            return final;
        }

        private static Node SetNode(MapperNode child, IElement element, Node parent = null)
        {
            var n = new Node()
            {
                Selector = child.Selector,
                Attributes = element.Attributes.Where(x => x.Name != "class").Where(x => x.Name != "d")
                                    .Select(x => new KeyValuePair<string, string>(x.Name, x.Value)).ToList(),
                Type = child.Type,
                ParentNode = parent
            };

            var flags = (int)child.Element.Flags;

            bool CheckFlags(int flag) => flag <= 270 && flag >= 250 || flag >= 306 && flag <= 340 || (flag >= 2304 && flag <= 2340);

            if (CheckFlags(flags))
            {
                var count = element.ChildNodes.Count(x => (x.NodeType == NodeType.Text || CheckFlags((int)x.Flags)) && !string.IsNullOrEmpty(x.TextContent.RemoveSpaces()));

                var textElements = element.ChildNodes?.Where(x => !string.IsNullOrEmpty(x.TextContent.RemoveSpaces()) && (x.NodeType == NodeType.Text));

                var text = string.Join(" ", textElements?.Select(x => x.TextContent));
                float index = (float)text.Length / child.Element.TextContent.Length;

                try
                {
                    if (element is IHtmlScriptElement)
                        n.Text = Regex.Unescape(element.InnerHtml);
                    else if (index == 0 || index > 0.35f)
                        n.Text = Regex.Unescape(Regex.Replace(text.Replace("\n", "").Replace("\r", ""), "\\s+", " ").Trim());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            return n;
        }
    }


}