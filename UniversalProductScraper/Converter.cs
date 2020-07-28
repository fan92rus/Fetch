namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MoreLinq;

    using SimhashLib;

    using UniversalProductScraper.Models;

    class Converter
    {
        private List<Table> Tables { get; set; } = new List<Table>();

        public void Convert(Node node)
        {
            this.ConvertNode(node, 0);
        }

        public IEnumerable<Table> GetTables()
        {
            return this.Tables.Where(x => x.Properties != null && x.Properties.Any() && x.Properties.Any(e => e?.Values?.Any() ?? false));
        }
        private IDictionary<string, string> ConvertNode(Node node, int id)
        {
            var subElements = node.Nodes.Where(x => (x?.Nodes?.Any() ?? false)).ToList();
            var voidChildren = node.Nodes.Where(x => !(x?.Nodes?.Any() ?? false)).ToList();

            Console.WriteLine(id);

            if (subElements.Any())
            {
                foreach (var group in subElements.GroupBy(x => x.Selector))
                {
                    var @object = @group.Select(this.ConvertNode);
                    this.AddObject(group.Key, node, @object);
                }
            }

            if (voidChildren.Any())
            {
                var collection = new MaybeDictionary<string, string>();

                foreach (var group in voidChildren.GroupBy(x => x.Selector))
                {
                    if (@group.Count() == 1 && (this.Tables?.All(x => x.Key.Name != @group.Key) ?? true))
                    {
                        var obj = this.GetProperty(@group.FirstOrDefault());

                        foreach (var (key, value) in obj.Where(x => !collection.ContainsKey(x.Key)))
                            collection.Add(key, value);
                        var nodeProps = this.GetProperty(node);
                        foreach (var (key, value) in nodeProps.Where(x => !collection.ContainsKey(x.Key)))
                            collection.Add(key, value);
                    }
                    else
                    {
                        this.AddObject(group.Key, node, @group.Select(this.GetProperty).Where(x => x.Any(e => e.Value != null)));
                    }
                }

                return collection;
            }

            return null;

        }

        private void AddObject(string key, Node node, IEnumerable<IDictionary<string, string>> els)
        {
            foreach (var el in els)
                if (el != null)
                    this.AddObject(TableKey.Create(node, key, el), el);
        }

        private void AddObject(TableKey tableKey, IDictionary<string, string> row)
        {
            if (row == null)
                return;

            var targetTable = this.FindTable(tableKey);

            if (targetTable == null)
            {
                targetTable = new Table(tableKey, new List<IDictionary<string, string>>());
                this.Tables.Add(targetTable);
            }

            if (!targetTable.Properties.Any(x => row.Values.All(e => x.Values.Contains(e))))
            {
                targetTable.Properties.Add(row);
                targetTable.Properties.DistinctBy(x => x.Keys);
            }
            else
            {

            }
        }

        private Table FindTable(TableKey key)
        {
            return this.Tables.FirstOrDefault(x => x.Key.Equals(key));
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
            if (item != null && item2 != null && !this.ContainsKey(item))
                base.Add(item, item2);
        }
    }

    public struct TableKey : IEquatable<TableKey>
    {
        public TableKey(string name, string containerSelector, Simhash propertyHash)
        {
            this.Name = name;
            this.ParentKey = containerSelector;
            this.PropertyHash = propertyHash;
        }

        public Simhash PropertyHash { get; }
        public string Name { get; }
        public string ParentKey { get; }

        private const int EqualsDistance = 15;

        public bool Equals(TableKey other)
        {
            var distance = this.PropertyHash.distance(other.PropertyHash);
            return distance < 10 && other.ParentKey == this.ParentKey && string.Equals(this.Name, other.Name);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.PropertyHash != null ? this.PropertyHash.GetHashCode() : 0) * 397) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
            }
        }

        public static TableKey Create(Node node, string key, IDictionary<string, string> els)
        {
            var simhash = new Simhash(Simhash.HashingType.Jenkins);
            simhash.GenerateSimhash(els.Keys.ToList());
            return new TableKey(key, node.ParentNode?.Selector, simhash);
        }
    }
    public class Table
    {
        public Table(TableKey key, List<IDictionary<string, string>> properties)
        {
            this.Key = key;
            this.Properties = properties;
        }

        public TableKey Key { get; }
        public List<IDictionary<string, string>> Properties { get; }
    }
}