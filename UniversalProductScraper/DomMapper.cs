namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.Linq;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using MoreLinq;
    using UniversalProductScraper.Models;

    enum Type
    {
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

                if (element is IHtmlAnchorElement)
                {
                    infoNode.Type = Type.Link;
                }
                else if (element is IHtmlImageElement)
                {
                    infoNode.Type = Type.Image;
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


            if (element.LocalName == "tr")
            {

            }
            if (!element.Children.Any())
            {
                var isOk = element.IsText() || element.Attributes.Any(p => p.Name != "class");
                if (!isOk)
                    return null;
            }

            foreach (var child in element.Children)
            {
                var parsed = this.ParseElementMap(child);

                if (parsed == null)
                    continue;

                foreach (var n in parsed.Nodes.ToList())
                {
                    var count = element.QuerySelectorAll(n.Selector).Length;

                    if (count == 1 || parsed.Nodes.Count == 1)
                    {
                        var containerSelector = child.GetContainer().GetSelector();
                        n.Selector = n.Element.GetSelector(containerSelector);
                        parsed.Nodes.Remove(n);
                        node.Nodes.Add(n);
                    }
                }

                var isOk = parsed.Element.IsText() || parsed.Element.Attributes.Any(p => p.Name != "class");

                if (isOk && node.Nodes.All(x => x.Selector != parsed.Selector) || parsed.Nodes.Any())
                    node.Nodes.Add(parsed);
            }

            node.Nodes = node.Nodes.DistinctBy(x => x.Selector + x.Nodes).ToList();

            return node;
        }
    }
}