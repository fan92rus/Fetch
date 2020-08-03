namespace UniversalProductScraper
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Threading.Tasks;

    using MoreLinq;

    using Newtonsoft.Json.Linq;

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
                IEnumerable<ExpandoObject> array = @group.Select(element => element.Tree.Item.GetProperties().ToExpandoObject()).ToList();

                if (group.Count() > 1 && array.Any())
                    target[@group.Key.ToString()] = array;

                else if (array.Any(x => x.Any()))
                    target[@group.Key.ToString()] = array.FirstOrDefault();
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

                if (@group.Count() == 1)
                    target[key] = this.Convert(@group.FirstOrDefault());
                else
                    target[key] = @group.Select(this.Convert).Where(x => x.Values.Any());
            }

            return target;
        }
    }

    public static class ExpandoEx
    {
        public static void AddRange(this IDictionary<string, object> target, IDictionary<string, object> elements)
        {
            foreach (var element in elements) target.Add(element.Key, element.Value);
        }
        public static ExpandoObject ToExpandoObject(this IDictionary<string, string> dictionary)
        {
            IDictionary<string, object> target = new ExpandoObject();
            foreach (var element in dictionary) target.Add(element.Key, element.Value);
            return (ExpandoObject)target;
        }
    }
}