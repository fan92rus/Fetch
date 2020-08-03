namespace UniversalProductScraper
{
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
            var root = new InfoNode() { Selector = "html", Element = doc.QuerySelector("html"), Type = Type.Container };

            var finalNode = new Tree<InfoNode>(root) { this.ParseElementMap(doc.QuerySelector("head")), this.ParseElementMap(doc.QuerySelector("body")) };

            this.DefineTypes(finalNode);

            return finalNode;
        }

        public void DefineTypes(ITree<InfoNode> infoNode)
        {
            if (infoNode?.Children?.Any() ?? false)
            {
                infoNode.Item.Type = Type.Container;

                foreach (var subNode in infoNode.Children)
                    this.DefineTypes(subNode);
            }
            else
            {
                var element = infoNode?.Item?.Element;
                var urlExpr = "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,})";

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

        public ITree<InfoNode> ParseElementMap(IElement element) => this.ParseElementMap(element, null);

        public ITree<InfoNode> ParseElementMap(IElement element, ITree<InfoNode> parent)
        {
            var baseTree = new Tree<InfoNode>(new InfoNode(element), parent);
            baseTree.AddRange(this.ParseChildren(element, baseTree));
            return baseTree;
        }

        private IEnumerable<ITree<InfoNode>> ParseChildren(IParentNode element, ITree<InfoNode> baseTree) =>
            element.Children.Select(x => this.ParseElementMap(x, baseTree))
                    .Where(parsedMap => parsedMap != null && this.ValidateNode(parsedMap, baseTree))
                    .Select(x => this.MoveItems(x, element));

        private bool ValidateNode(ITree<InfoNode> node, ITree<InfoNode> baseNode)
        {
            if (baseNode.Contains(x => x.Selector == node.Item.Selector))
                return false;

            if (node.Children.Any())
                return true;
            if (node.Item?.Element?.Attributes?.Any(p => p?.Name != "class") ?? false)
                return true;
            if (node?.Item?.Type == Type.Link)
                return true;

            return false;
        }

        [SuppressMessage("ReSharper", "GenericEnumeratorNotDisposed")]
        private ITree<InfoNode> MoveItems(ITree<InfoNode> parsedMap, IParentNode element)
        {
            var enumerator = parsedMap.Children.GetEnumerator();

            var baseTree = parsedMap.Parent;

            while (enumerator.MoveNext())
            {
                var tree = enumerator.Current;

                var count = element.QuerySelectorAll(tree?.Item?.Selector).Length;

                //Если выполняеться условие то поднимаем элемент на уровень выше (проверка что он один)
                if (tree?.Item != null && (count == 1 || tree?.Children?.Count == 1))
                {
                    parsedMap.Remove(tree);

                    if (!baseTree.Contains(tree))
                        baseTree.Add(tree);
                }
                else
                {
                    break;
                }

                enumerator = parsedMap.Children.GetEnumerator();
            }

            return parsedMap;
        }
    }
}