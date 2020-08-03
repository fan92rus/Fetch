namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using UniversalProductScraper.Extensions;
    using UniversalProductScraper.Graph;
    class TreeConverter
    {
        public IDictionary<string, object> Convert(ITree<BaseNode> tree)
        {
            IDictionary<string, object> target = new ExpandoObject();

            var fullChildren = tree.Children.Where(x => x.Children.Any());
            var voidChildren = tree.Children.Where(x => !x.Children.Any());

            target.AddRange(this.FullChildrenProcessing(fullChildren));
            target.AddRange(this.VoidChildrenProcessing(SortedBaseNodeTree.TreeMarkup(voidChildren)));

            return target;
        }

        private IDictionary<string, object> VoidChildrenProcessing(IEnumerable<SortedBaseNodeTree> targetEls)
        {
            IDictionary<string, object> target = new Dictionary<string, object>();

            if (!targetEls.Any())
                return target;

            foreach (var group in targetEls.GroupBy(x => x.Key))
            {
                var objects = @group.Select(element => element.Tree.Item.GetProperties().ToExpandoObject()).ToList();

                if (objects.Count() > 1)
                    target[@group.Key.ToString()] = objects;
                else if (objects.Any(x => x.Any()))
                    target[@group.Key.ToString()] = objects.FirstOrDefault();
            }

            return target;
        }

        private IDictionary<string, object> FullChildrenProcessing(IEnumerable<ITree<BaseNode>> subElements)
        {
            IDictionary<string, object> target = new Dictionary<string, object>();

            if (!subElements.Any()) return target;

            foreach (var group in subElements.GroupBy(x => x.Item.Selector))
            {
                var key = $"{group.FirstOrDefault()?.Parent?.Item?.Selector ?? ""}_{@group.Key}";

                var objects = @group.Select(this.Convert).Where(x => x.Values.Any()).ToList();

                if (objects.Count() == 1)
                    target[key] = objects.FirstOrDefault();
                else
                    target[key] = objects;
            }

            return target;
        }
    }
}