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
                    this.AddObject(@group.Key, @object);
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
                        this.AddObject(@group.Key, @group.Select(this.GetProperty).Where(x => x.Any(e => e.Value != null)));
                    }
                }

                return collection;
            }

            return null;

        }

        private void AddObject(string name, IEnumerable<IDictionary<string, string>> els)
        {
            if (name.Contains("nth-child"))
                return;

            foreach (var el in els)
            {
                this.AddObject(name, el);
            }
        }

        private void AddObject(string name, IDictionary<string, string> row)
        {
            if (row == null)
                return;

            var targetTable = this.FindTable(name, row, out var hash);

            if (targetTable == null)
            {
                targetTable = new Table(name, hash, new List<IDictionary<string, string>>());
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

        private Table FindTable(string name, IDictionary<string, string> els, out Simhash hash)
        {
            var localSimhash = new Simhash(Simhash.HashingType.Jenkins);
            hash = localSimhash;
            hash.GenerateSimhash(els.Keys.ToList());

            return this.Tables.FirstOrDefault(x => x.Key.Equals(new TableKey(name, localSimhash)));
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

    public class TableKey : IEquatable<TableKey>
    {
        public TableKey(string name, Simhash propertyHash)
        {
            this.Name = name;
            this.PropertyHash = propertyHash;
        }

        public Simhash PropertyHash { get; }
        public string Name { get; }

        private const int EqualsDistance = 20;

        public bool Equals(TableKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var distance = this.PropertyHash.distance(other.PropertyHash);
            return distance < EqualsDistance && string.Equals(this.Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && this.Equals((TableKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.PropertyHash != null ? this.PropertyHash.GetHashCode() : 0) * 397) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
            }
        }
    }
    public class Table
    {
        public Table(string name, Simhash hash, List<IDictionary<string, string>> properties)
        {
            this.Key = new TableKey(name, hash);
            this.Properties = properties;
        }

        public TableKey Key { get; }
        public List<IDictionary<string, string>> Properties { get; }
    }
}