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
                //Classes = element.ClassList.Select(x => x.ToString()).ToList(),
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

        private List<Node> ScrapListNoder(InfoNode info)
        {
            try
            {
                if (info.Selector.Contains("tbody"))
                {

                }

                var infoSelector = info.Element.GetSelector();

                Console.WriteLine("ScrapNode - " + info.Selector);
                var nodes = new List<Node>();

                var parent = info.Element.ParentElement;


                var elements = new List<IElement>();

                if (info.Element.LocalName == "html")
                    elements.Add(info.Element);
                else if (parent != null)
                    elements = parent.QuerySelectorAll(info.Selector).ToList();

                if (!elements.Any())
                {

                }
                foreach (var element in elements)
                {
                    var testSelector = element.GetSelector();
                    var finalNode = new Node
                    {
                        Selector = info.Selector,
                        Attributes = element.Attributes.Where(x => x.Name != "class").Select(
                                                x => new KeyValuePair<string, string>(x.Name, x.Value)).ToList(),
                        //Classes = element.ClassList.Select(x => x.ToString()).ToList()
                    };

                    if (info.Type == Type.Text || info.Type == Type.Link)
                    {
                        var text = element.ChildNodes.FirstOrDefault(o => o.NodeType == NodeType.Text && o.TextContent.Trim() != "");

                        if (element is IHtmlScriptElement)
                        {
                            finalNode.Text = Regex.Unescape(element.Text());
                        }
                        else
                        {
                            if (text?.TextContent != null)
                                finalNode.Text = Regex.Unescape(Regex.Replace(text?.TextContent.Replace("\n", "").Replace("\r", ""), "\\s+", " ").Trim());
                        }
                    }

                    finalNode.Nodes = new List<Node>();

                    //foreach (var node in info.Nodes)
                    //{
                    //    var el = element.QuerySelector(node.Selector);

                    //    if (el == null)
                    //    {
                    //        continue;
                    //    }

                    //    finalNode.Nodes.AddRange(this.scrapNode(new InfoNode()
                    //    {
                    //        Selector = node.Selector,
                    //        Element = el ?? element,
                    //        Nodes = node.Nodes
                    //    }));
                    //}

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