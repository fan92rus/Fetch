namespace UniversalProductScraper
{
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using UniversalProductScraper.Graph;
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
        public ITree<InfoNode> ParseDocumentMap(IHtmlDocument doc)
        {
            var head = doc.QuerySelector("head");
            var body = doc.QuerySelector("body");
            var root = new InfoNode() { Selector = "html", Element = doc.QuerySelector("html"), Type = Type.Container };

            var finalNode = new Tree<InfoNode>(root);

            finalNode.Add(this.ParseElementMap(head));
            finalNode.Add(this.ParseElementMap(body));


            this.DefineTypes(finalNode);
            return finalNode;
        }

        public void DefineTypes(ITree<InfoNode> infoNode)
        {
            if (infoNode.Children.Any())
            {
                infoNode.Item.Type = Type.Container;

                foreach (var subNode in infoNode.Children)
                    this.DefineTypes(subNode);
            }
            else
            {
                var element = infoNode.Item.Element;
                var urlExpr =
                    "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,})";


                if (element is IHtmlImageElement)
                    infoNode.Item.Type = Type.Image;
                else if (element is IHtmlAnchorElement || element.Attributes.Any(a => Regex.IsMatch(a.Value, urlExpr)))
                    infoNode.Item.Type = Type.Link;
                else if (element is IHtmlButtonElement)
                    infoNode.Item.Type = Type.Button;
                else
                    infoNode.Item.Type = Type.Text;
            }
        }

        public ITree<InfoNode> ParseElementMap(IElement element, ITree<InfoNode> parent = null)
        {
            var node = new InfoNode
            {
                Element = element,
                Selector = element.ParentElement != null
                                              ? element.GetSelector((element.ParentElement).GetSelector())
                                              : element.GetSelector(),
            };

            var tree = new Tree<InfoNode>(node, parent);

            if (!element.Children.Any())
            {
                var isOk = element.IsText() || element.Attributes.Any(p => p.Name != "class");

                if (!isOk)
                    return null;
            }

            var locker = new object();
            var i = 0;

            Parallel.ForEach(element.Children,
                child =>
                    {
                        var parsed = this.ParseElementMap(child, tree);

                        if (parsed == null)
                            return;

                        foreach (var item in parsed.Children.ToList())
                        {
                            var count = element.QuerySelectorAll(item.Item?.Selector).Length;

                            if (count == 1)
                            {
                                if (item.Item?.Element?.TextContent?.RemoveSpaces() == parsed?.Item?.Element.TextContent?.RemoveSpaces())
                                {
                                    var isRemoved = parsed.Remove(item);
                                }
                                else
                                {
                                    var containerSelector = child.GetContainer().GetSelector();

                                    if (item?.Item == null) continue;

                                    item.Item.Selector = item.Item.Element.GetSelector(containerSelector);

                                    var isRemoved = parsed.Remove(item);

                                    tree.Add(item);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        var isOk = parsed.Item != null && ((parsed?.Item?.Element?.TextContent != null)
                                                           || parsed.Children.Any()
                                                           || parsed.Item?.Element?.Attributes != null && (bool)parsed.Item?.Element?.Attributes?.Any(p => p?.Name != "class")
                                                           || parsed?.Item?.Type == Type.Link);

                        lock (locker)
                        {
                            if (isOk)
                                tree.Add(parsed);

                            Console.WriteLine($"{parsed?.Item?.Selector} {i++}");
                        }
                    });

            return tree;
        }
    }
}