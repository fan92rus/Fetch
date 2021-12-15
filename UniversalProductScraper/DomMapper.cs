using SimhashLib;
using Unity;

namespace UniversalProductScraper
{
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using MoreLinq;

    using UniversalProductScraper.Graph;
    using UniversalProductScraper.Models;

    internal interface IDomMapper
    {
        ITree<InfoNode> ParseDocumentMap(IHtmlDocument doc);
    }

    class DomMapper : IDomMapper
    {
        [Dependency]
        public ITreeBuilder<InfoNode> TreeBuilder { get; set; }

        public List<Func<ITree<InfoNode>, bool>> CheckRules = new List<Func<ITree<InfoNode>, bool>>()
                                                           {
                                                               x=>x.Children.Any(),
                                                               x=>x.Item.Element.Attributes.Any(e => e.Name!="class" && e.Name!="id"),
                                                               x=>x.Item.Type == DomNodeType.Link
                                                           };
        public ITree<InfoNode> ParseDocumentMap(IHtmlDocument doc)
        {
            var root = new InfoNode() { Selector = "html", Element = doc.QuerySelector("html"), Type = DomNodeType.Container };

            var finalNode = TreeBuilder.Create(root);
            finalNode.Add(ParseElementMap(doc.QuerySelector("head")));
            finalNode.Add(ParseElementMap(doc.QuerySelector("body")));

            DefineTypes(finalNode);
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
                    DefineTypes(subNode);
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

        public ITree<InfoNode> ParseElementMap(IElement element) => ParseElementMap(element, null);
        public ITree<InfoNode> ParseElementMap(IElement element, ITree<InfoNode> parent)
        {
            var baseTree = TreeBuilder.Create(new InfoNode(element), parent);
            ParseChildren(element, baseTree);
            return baseTree;
        }

        /// <summary>
        /// Парсинг дочерних нод
        /// </summary>
        /// <param name="element">Контейнер содержащий ноды</param>
        /// <param name="baseTree">Родительское дерево</param>
        /// <returns>Очищенные дочерние ноды</returns>
        private void ParseChildren(IElement element, ITree<InfoNode> baseTree)
        {
            if (element.Children.Any(x => x?.ClassName == "coupon-store-item"))
            {

            }

            var parsedChildren = element.Children.Select(x => ParseElementMap(x, baseTree)).Where(parsedMap => parsedMap != null).ToList();

            var parsedTrees = TreeBuilder.Create();

            foreach (var child in parsedChildren)
            {
                MoveItems(child, element, baseTree);
                //if (!parsedTrees.Contains(child))
                parsedTrees.Add(child);
            }

            var test = parsedTrees.Children.GroupBy(x => x.Item).Select(x => x.OrderByDescending(e => e.Children.Count));
            ;

            var groupingTrees = test.Select(tree => tree.Select(x =>
            {
                var childrenHashes = x.Children.Select(ch => ch.Item.Selector).Concat(new List<string>() { x.Item.Selector }).ToList();
                var simhash = new Simhash();
                simhash.GenerateSimhash(childrenHashes);
                return new { target = x, Key = simhash };
            }).GroupBy(x => x.Key, new HashComparer(10)));

            foreach (var treeGroup in groupingTrees)
            {
                var target = treeGroup.FirstOrDefault()?.OrderByDescending(x => x?.target?.Children?.Count)?.FirstOrDefault()?.target;
                baseTree.Add(target);
            }

            if (baseTree.Children.Any(e => e.Find(x => x.Selector.Contains("coupon-store-item")) != null) && !baseTree.Item.Selector.Contains("coupon-store-item"))
            {

            }
            else
            {

            }
            //baseTree.AddRange(parsedTrees.Children.GroupBy(x => x.Item).Select(x => x.OrderByDescending(e => e.Children.Count).FirstOrDefault()));
        }

        /// <summary>
        /// Проверка ноды на валидность
        /// </summar48VZXBV By>
        /// <param name="node">Нода</param>
        /// <param name="baseNode">Родительская нода</param>
        /// <returns>Валидна ли нода</returns>
        private bool ValidateNode(ITree<InfoNode> node, ITree<InfoNode> baseNode) => (baseNode == null || !baseNode.Contains(node)) && ValidateNode(node);
        private bool ValidateNode(ITree<InfoNode> node) => CheckRules.Any(x => x.Invoke(node));

        /// <summary>
        /// Перемещение одиночных дочерних нод на уровень выше
        /// </summary>
        /// <param name="parsedMap">Спаршеная карта документа</param>
        /// <param name="element">Родительский элемент</param>
        /// <returns></returns>
        private void MoveItems(ITree<InfoNode> parsedMap, IParentNode element, ITree<InfoNode> baseTree)
        {
            var enumerator = parsedMap.Children.GetEnumerator();
            if (parsedMap.Item.Element is IHtmlTableElement || parsedMap.Item.Element is IHtmlTableRowElement)
                return;

            while (enumerator.MoveNext())
            {
                var tree = enumerator.Current;

                var count = element.QuerySelectorAll(tree?.Item?.Selector).Length;

                //Если выполняеться условие то поднимаем элемент на уровень выше (проверка что он один)
                if (tree?.Item != null && (count == 1 || tree?.Children?.Count == 1) && !baseTree.Contains(tree))
                {
                    parsedMap.Remove(tree);
                    baseTree.Add(tree);
                }
                else
                {
                    break;
                }

                enumerator = parsedMap.Children.GetEnumerator();
            }

            //if (this.ValidateNode(parsedMap))
            //    baseTree.Add(parsedMap);
        }
    }
    class HashComparer : IEqualityComparer<Simhash>
    {
        public int Distance { get; }

        public HashComparer(int distance)
        {
            Distance = distance;
        }
        public bool Equals(Simhash a, Simhash b) => a.distance(b) < Distance;

        public int GetHashCode(Simhash hash) => hash.value.GetHashCode();
    }
    public struct SimhashKey : IEquatable<SimhashKey>
    {
        public Simhash Hash { get; }
        public int Distance { get; }
        public SimhashKey(Simhash hash, int distance)
        {
            Distance = distance;
            Hash = hash;
        }


        public bool Equals(SimhashKey other)
        {
            var avg = new List<int>() { Distance, other.Distance }.Average();
            return Hash.distance(other.Hash) < avg;
        }

        public override bool Equals(object obj) => obj is SimhashKey other && Equals(other);

        public override int GetHashCode() => (Hash != null ? Hash.GetHashCode() : 0);
    }
}