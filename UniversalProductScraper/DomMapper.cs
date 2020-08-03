namespace UniversalProductScraper
{
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UniversalProductScraper.Graph;
    using UniversalProductScraper.Models;

    class DomMapper
    {
        public List<Func<ITree<InfoNode>, bool>> CheckRules = new List<Func<ITree<InfoNode>, bool>>()
                                                           {
                                                               x=>x.Children.Any(),
                                                               x=>x.Item.Element.Attributes.Any(e => e.Name!="class" && e.Name!="id"),
                                                               x=>x.Item.Type == DomNodeType.Link
                                                           };
        public ITree<InfoNode> ParseDocumentMap(IHtmlDocument doc)
        {
            var root = new InfoNode() { Selector = "html", Element = doc.QuerySelector("html"), Type = DomNodeType.Container };

            var finalNode = new Tree<InfoNode>(root) { this.ParseElementMap(doc.QuerySelector("head")), this.ParseElementMap(doc.QuerySelector("body")) };

            this.DefineTypes(finalNode);

            return finalNode;
        }

        /// <summary>
        /// Типизация дочерних нод
        /// </summary>
        /// <param name="infoNode"></param>
        public void DefineTypes(ITree<InfoNode> infoNode)
        {
            if (infoNode?.Children?.Any() ?? false)
            {
                infoNode.Item.Type = DomNodeType.Container;

                foreach (var subNode in infoNode.Children)
                    this.DefineTypes(subNode);
            }
            else
            {
                var element = infoNode?.Item?.Element;
                var urlExpr = "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,})";

                if (element is IHtmlImageElement)
                    infoNode.Item.Type = DomNodeType.Image;
                else if (element is IHtmlAnchorElement || element.Attributes.Any(a => Regex.IsMatch(a.Value, urlExpr)))
                    infoNode.Item.Type = DomNodeType.Link;
                else if (element is IHtmlButtonElement)
                    infoNode.Item.Type = DomNodeType.Button;
                else
                    infoNode.Item.Type = DomNodeType.Text;
            }
        }
        public ITree<InfoNode> ParseElementMap(IElement element) => this.ParseElementMap(element, null);

        public ITree<InfoNode> ParseElementMap(IElement element, ITree<InfoNode> parent)
        {
            var baseTree = new Tree<InfoNode>(new InfoNode(element), parent);
            baseTree.AddRange(this.ParseChildren(element, baseTree));
            return baseTree;
        }

        /// <summary>
        /// Парсинг дочерних нод
        /// </summary>
        /// <param name="element">Контейнер содержащий ноды</param>
        /// <param name="baseTree">Родительское дерево</param>
        /// <returns>Очищенные дочерние ноды</returns>
        private IEnumerable<ITree<InfoNode>> ParseChildren(IParentNode element, ITree<InfoNode> baseTree) =>
                //Парсим дочерние ноды
                element.Children.Select(x => this.ParseElementMap(x, baseTree))
                //Выкидываем пустые и невалидные ноды
                .Where(parsedMap => parsedMap != null && this.ValidateNode(parsedMap, baseTree))
                //Перемещаем одиночные ноды на уровень выше
                .Select(x => this.MoveItems(x, element));

        /// <summary>
        /// Проверка ноды на валидность
        /// </summary>
        /// <param name="node">Нода</param>
        /// <param name="baseNode">Родительская нода</param>
        /// <returns>Валидна ли нода</returns>
        private bool ValidateNode(ITree<InfoNode> node, ITree<InfoNode> baseNode) => (baseNode == null || !baseNode.Contains(x => x.Selector == node.Item.Selector)) && this.CheckRules.Any(x => x.Invoke(node));

        /// <summary>
        /// Перемещение одиночных дочерних нод на уровень выше
        /// </summary>
        /// <param name="parsedMap">Спаршеная карта документа</param>
        /// <param name="element">Родительский элемент</param>
        /// <returns></returns>
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