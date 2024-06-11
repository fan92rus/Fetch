using System.Collections.Generic;
using System.Linq;

namespace UniversalProductScraper.Graph
{
    public class SortedBaseNodeTree
    {
        public SortedBaseNodeTree(ITree<BaseNode> tree, Dictionary<string, string> properties)
        {
            Properties = properties;
            Key = TableKey.Create(tree?.Parent?.Item?.Selector, tree?.Item?.Selector, Properties);
            Tree = tree;
        }

        public SortedBaseNodeTree(ITree<BaseNode> tree)
        {
            Key = TableKey.Create(tree?.Parent?.Item?.Selector, tree?.Item?.Selector, tree?.Item?.GetProperties());
            Tree = tree;
        }
        public Dictionary<string, string> Properties { get; set; }
        public ITree<BaseNode> Tree { get; set; }
        public TableKey Key { get; set; }
        public static IEnumerable<SortedBaseNodeTree> TreeMarkup(IEnumerable<ITree<BaseNode>> voidChildren) => voidChildren.Select(x => new SortedBaseNodeTree(x, x.Item.GetProperties()));

    }
}