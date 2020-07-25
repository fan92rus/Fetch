namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MoreLinq;

    using UniversalProductScraper.Models;

    class Converter
    {
        public Dictionary<string, string> Names { get; set; } = new Dictionary<string, string>();
        private List<Table> Tables { get; set; } = new List<Table>();

        public IEnumerable<Table> Convert(Node node)
        {
            this.ConvertNode(node, 0);
            return this.Tables.Where(x => x.Properties != null && x.Properties.Any() && x.Properties.Any(e => e?.Values?.Any() ?? false));
        }
        public List<string> Collections = new List<string>();
        private IDictionary<string, string> ConvertNode(Node node, int id)
        {
            if (node?.Selector != null && node.Selector == "li.literal__item > a")
            {

            }
            Dictionary<string, string> SetParent(Dictionary<string, string> properties)
            {
                properties.Add("parent_id", id.ToString());
                properties.Add("parent_selector", node.Selector);
                return properties;
            }

            var subElements = node.Nodes.Where(x => (x?.Nodes?.Any() ?? false)).ToList();
            var voidChildren = node.Nodes.Where(x => !(x?.Nodes?.Any() ?? false)).ToList();

            Console.WriteLine(id);

            if (subElements.Any())
            {
                foreach (var group in subElements.GroupBy(x => x.Selector))
                {
                    var @object = @group.Select(this.ConvertNode);
                    this.AddObject(@group.Key, @object);
                }
            }

            if (!voidChildren.Any()) return null;

            var collection = new MaybeDictionary<string, string>();

            foreach (var group in voidChildren.GroupBy(x => x.Selector))
            {
                if (@group.Count() == 1)
                {
                    var obj = SetParent(this.GetProperty(@group.FirstOrDefault())).Where(x => !collection.ContainsKey(x.Key));

                    foreach (var (key, value) in obj)
                        collection.Add(key, value);
                }
                else
                {
                    this.AddObject(@group.Key, group.Select(this.GetProperty).Where(x => x.Any(e => e.Value != null)).Select(SetParent));
                }
            }


            return collection;

        }

        private void AddObject(string key, IEnumerable<IDictionary<string, string>> els)
        {
            var targetTable = this.Tables.FirstOrDefault(x => x.Name == key);

            if (targetTable == null)
            {
                targetTable = new Table(key, new List<IDictionary<string, string>>());
                this.Tables.Add(targetTable);
            }

            targetTable.Properties.AddRange(els.Where(x => x != null && x.Values.Any()));
            targetTable.Properties.DistinctBy(x => x.Keys);
        }

        private Dictionary<string, string> GetProperty(Node gNode)
        {
            Dictionary<string, string> dList = new Dictionary<string, string>();

            foreach (var el in gNode.Attributes)
            {
                var key = el.Key.Replace("-", "_");
                if (!dList.ContainsKey(key))
                    dList.Add(key, el.Value);
            }

            dList.Add(gNode.Selector.Replace("-", "_"), gNode.Text);
            return dList;
        }
    }

    public class MaybeList<T> : List<T>
    {
        public new void Add(T item)
        {
            if (item != null)
                base.Add(item);
        }
    }
    public class MaybeDictionary<T, TE> : Dictionary<T, TE>
    {
        public new void Add(T item, TE item2)
        {
            if (item != null && item2 != null)
                base.Add(item, item2);
        }
    }
    public class Table
    {
        public Table(string name, List<IDictionary<string, string>> properties)
        {
            this.Name = name;
            this.Properties = properties;
        }

        public string Name { get; }
        public List<IDictionary<string, string>> Properties { get; }
    }
}