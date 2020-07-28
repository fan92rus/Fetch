namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using MoreLinq;
    using UniversalProductScraper.Models;

    public enum Type
    {
        Default,
        Link,
        Text,
        Image,
        Container,
        Button
    }

    class DomMapper
    {

        public InfoNode ParseDocumentMap(IHtmlDocument doc)
        {
            var head = doc.QuerySelector("head");
            var body = doc.QuerySelector("body");

            var finalNode = new InfoNode()
            {
                Element = doc.QuerySelector("html"),
                Selector = "html",
                Nodes = new List<InfoNode>()
                                                {
                                                    new InfoNode()
                                                        {
                                                            Element = head,
                                                            Selector = "head",
                                                            Nodes = this.ParseElementMap(head).Nodes
                                                        },
                                                    new InfoNode()
                                                        {
                                                            Element = body,
                                                            Selector = "body",
                                                            Nodes = this.ParseElementMap(body).Nodes
                                                        }
                                                }
            };

            this.DefineTypes(finalNode);

            return finalNode;
        }

        public void DefineTypes(InfoNode infoNode)
        {
            if (infoNode.Nodes.Any())
            {
                infoNode.Type = Type.Container;

                foreach (var subNode in infoNode.Nodes)
                    this.DefineTypes(subNode);
            }
            else
            {
                var element = infoNode.Element;
                var urlExpr =
                    "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,})";


                if (element is IHtmlImageElement)
                {
                    infoNode.Type = Type.Image;
                }
                else if (element is IHtmlAnchorElement || element.Attributes.Any(a => Regex.IsMatch(a.Value, urlExpr)))
                {
                    infoNode.Type = Type.Link;
                }
                else if (element is IHtmlButtonElement)
                {
                    infoNode.Type = Type.Button;
                }
                else
                {
                    infoNode.Type = Type.Text;
                }
            }
        }

        public InfoNode ParseElementMap(IElement element)
        {

            var node = new InfoNode
            {
                Element = element,
                Selector = element.ParentElement != null
                                              ? element.GetSelector((element.ParentElement).GetSelector())
                                              : element.GetSelector(),
            };


            if (node.Selector.Contains("data-table"))
            {

            }
            if (!element.Children.Any())
            {
                var isOk = element.IsText() || element.Attributes.Any(p => p.Name != "class");
                if (!isOk)
                    return null;
            }
            object locker = new object();
            int i = 0;
            Parallel.ForEach(element.Children,
                child =>
                    {
                        var parsed = this.ParseElementMap(child);

                        if (parsed == null)
                            return;

                        var enumerator = parsed.Nodes.GetEnumerator();
                        var moved = false;

                        while (enumerator.MoveNext())
                        {
                            var n = enumerator.Current;

                            var count = element.QuerySelectorAll(n?.Selector).Length;

                            if (count == 1 && n?.Element?.TextContent?.RemoveSpaces() != parsed?.Element?.TextContent?.RemoveSpaces())
                            {
                                moved = true;
                                var containerSelector = child.GetContainer().GetSelector();
                                n.Selector = n.Element.GetSelector(containerSelector);
                                parsed.Nodes.Remove(n);
                                node.Nodes.Add(n);
                                enumerator = parsed.Nodes.GetEnumerator();
                            }
                            else
                            {
                                break;
                            }
                        }

                        var isOk = (parsed.Element.IsText() || parsed.Element.Attributes.Any(p => p.Name != "class") || parsed.Type == Type.Link) && !moved;

                        lock (locker)
                        {
                            if (isOk && node.Nodes.All(x => x.Selector != parsed.Selector) || parsed.Nodes.Any())
                                node.Nodes.Add(parsed);

                            Console.WriteLine($"{parsed.Selector} {i++}");
                        }
                    });

            var nodes = node.Nodes.DistinctBy(x => x.Selector + "_" + string.Join("_", x.Nodes.Select(e => e.Selector)))/*.ToList();//*/.GroupBy(x => x.Selector);
            node.Nodes = new List<InfoNode>();

            foreach (var group in nodes)
            {
                var el = group.First();

                foreach (var ge in group.SelectMany(g => g.Nodes))
                {
                    if (el.Nodes.All(x => x.Selector != ge.Selector))
                    {
                        el.Nodes.Add(ge);
                    }
                }

                node.Nodes.Add(el);
            }

            return node;
        }
    }
}