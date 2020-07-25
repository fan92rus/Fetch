namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;

    using UniversalProductScraper.Models;
    using Node = Models.Node;

    class Scraper
    {
        public Node ScrapNode(InfoNode info, Node parent = null)
        {

            var final = SetNode(info, info.Element, parent);
            final.Nodes = new List<Node>();

            foreach (var child in info.Nodes)
            {
                if (child.Type == Type.Container)
                {
                    var childrenAll = info.Element.QuerySelectorAll(child.Selector);

                    foreach (var c in childrenAll)
                    {
                        final.Nodes.Add(this.ScrapNode(new InfoNode()
                        {
                            Selector = child.Selector,
                            Nodes = child.Nodes,
                            Element = c
                        }, final));
                    }

                    continue;
                }

                var children = info.Element.QuerySelectorAll(child.Selector);

                foreach (var element in children)
                {
                    var node = SetNode(child, element, final);
                    final.Nodes.Add(node);
                }
            }

            return final;
        }

        private static Node SetNode(InfoNode child, IElement element, Node parent = null)
        {
            var n = new Node()
            {
                Selector = child.Selector,
                Attributes =
                                element.Attributes.Where(x => x.Name != "class")
                                    .Select(x => new KeyValuePair<string, string>(x.Name, x.Value)).ToList(),
                Type = child.Type,
                ParentNode = parent
            };

            if (child.Type == Type.Text || child.Type == Type.Link)
            {
                var text = element.ChildNodes.FirstOrDefault(o => o.NodeType == NodeType.Text && o.TextContent.Trim() != "");

                try
                {
                    if (element is IHtmlScriptElement)
                        n.Text = Regex.Unescape(element.InnerHtml);
                    else if (text?.TextContent != null)
                        n.Text = Regex.Unescape(Regex.Replace(text?.TextContent.Replace("\n", "").Replace("\r", ""), "\\s+", " ").Trim());
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