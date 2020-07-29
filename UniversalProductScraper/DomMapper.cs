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
        public ITree<MapperNode> ParseDocumentMap(IHtmlDocument doc)
        {
            var head = doc.QuerySelector("head");
            var body = doc.QuerySelector("body");
            var root = new MapperNode() { Selector = "html", Element = doc.QuerySelector("html"), Type = Type.Container };

            var finalNode = new Tree<MapperNode>(root);

            finalNode.Add(this.ParseElementMap(head));
            finalNode.Add(this.ParseElementMap(body));


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

        public ITree<MapperNode> ParseElementMap(IElement element, ITree<MapperNode> parent = null)
        {
            var node = new MapperNode
            {
                Element = element,
                Selector = element.ParentElement != null
                                              ? element.GetSelector((element.ParentElement).GetSelector())
                                              : element.GetSelector(),
            };

            var tree = new Tree<MapperNode>(node, parent);

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

                        bool moved = false;

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
                                    moved = true;

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

            //var nodes = node.Nodes.DistinctBy(x => x.Selector + "_" + string.Join("_", x.Nodes.Select(e => e.Selector)))/*.ToList();//*/.GroupBy(x => x.Selector);

            //node.Nodes = new List<MapperNode>();

            //foreach (var group in nodes)
            //{
            //    var el = group.First();

            //    foreach (var ge in group.SelectMany(g => g.Nodes))
            //    {
            //        if (el.Nodes.All(x => x.Selector != ge.Selector))
            //        {
            //            el.Nodes.Add(ge);
            //        }
            //    }

            //    node.Nodes.Add(el);
            //}

            return tree;
        }
    }
}