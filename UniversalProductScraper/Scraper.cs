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

        public Node ScrapNode(InfoNode info)
        {
            try
            {
                var element = new Node()
                {
                    Selector = info.Selector,
                    Nodes = this.ScrapListNode(info)
                };


                return element;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private List<Node> ScrapListNode(InfoNode info)
        {
            try
            {
                if (info.Selector == "tr")
                {

                }

                Console.WriteLine("ScrapNode - " + info.Selector);
                var nodes = new List<Node>();

                var parent = info.Element.ParentElement ?? info.Element;
                var elements = parent.QuerySelectorAll(info.Selector).ToList();

                if (!elements.Any())
                    elements.Add(parent);

                foreach (var e in elements)
                {
                    var finalNode = new Node
                    {
                        Selector = info.Selector,
                        Attributes = e.Attributes.Where(x => x.Name != "class").Select(
                                                x => new KeyValuePair<string, string>(x.Name, x.Value)).ToList(),
                        Classes = e.ClassList.Select(x => x.ToString()).ToList()
                    };

                    if (info.Type == Type.Text || info.Type == Type.Link)
                    {
                        var text = e.ChildNodes.FirstOrDefault(o => o.NodeType == NodeType.Text && o.TextContent.Trim() != "");

                        if (e is IHtmlScriptElement)
                        {
                            finalNode.Text = Regex.Unescape(e.Text());
                        }
                        else
                        {
                            if (text?.TextContent != null)
                                finalNode.Text = Regex.Unescape(Regex.Replace(text?.TextContent.Replace("\n", "").Replace("\r", ""), "\\s+", " ").Trim());
                        }
                    } 
                    finalNode.Nodes = new List<Node>();

                    foreach (var node in info.Nodes)
                    {
                        var el = e.QuerySelector(node.Selector);
                        finalNode.Nodes.AddRange(this.ScrapListNode(new InfoNode()
                        {
                            Selector = node.Selector,
                            Element = el ?? e,
                            Nodes = node.Nodes
                        }));
                    }

                    nodes.Add(finalNode);
                }

                return nodes;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }
    }
}