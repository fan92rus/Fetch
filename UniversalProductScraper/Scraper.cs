namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;

    using AngleSharp.Css.Dom;
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
                                element.Attributes.Where(x => x.Name != "class").Where(x => x.Name != "d")
                                    .Select(x => new KeyValuePair<string, string>(x.Name, x.Value)).ToList(),
                Type = child.Type,
                ParentNode = parent
            };

            var flags = (int)child.Element.Flags;

            bool checkFlags(int flags)
            {
                return flags <= 270 && flags >= 250 || flags >= 306 && flags <= 340 || (flags >= 2304 && flags <= 2340);
            }
            if (checkFlags(flags))
            {
                var count = element.ChildNodes.Count(x => (x.NodeType == NodeType.Text || checkFlags((int)x.Flags)) && !string.IsNullOrEmpty(x.TextContent.RemoveSpaces()));

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