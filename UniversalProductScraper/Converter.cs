namespace UniversalProductScraper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MoreLinq;

    using Newtonsoft.Json.Linq;

    using SimhashLib;
    using UniversalProductScraper.Graph;
    class TreeConverter
    {

        public JObject Convert(ITree<BaseNode> tree) => this.ConvertNode(tree);
        private JObject ConvertNode(ITree<BaseNode> tree)
        {
            var target = new JObject();

            var subElements = tree.Children.Where(x => (x?.Children?.Any() ?? false)).ToList();
            var voidChildren = tree.Children.Where(x => !(x?.Children?.Any() ?? false)).ToList();

            if (subElements.Any())
            {
                foreach (var group in subElements.GroupBy(x => x.Item.Selector))
                {
                    if (group.Count() == 1)
                    {
                        var @object = this.ConvertNode(group.FirstOrDefault());
                        
                        var name = $"{tree.Item.Selector}_{group.Key}";
                        target.Add(name, @object);
                    }
                    else
                    {
                        var array = new JArray();

                        foreach (var ob in group)
                        {
                            var @object = this.ConvertNode(ob);
                            if (@object.HasValues)
                                array.Add(@object);
                        }

                        var name = $"{tree.Item.Selector}_{group.Key}";
                        target.Add(name, array);
                    }

                }
            }

            if (!voidChildren.Any()) return target;

            var targetEls = voidChildren.Select(
                x =>
                    {
                        var properties = this.GetProperty(x.Item);
                        return new
                        {
                            Properties = properties,
                            Node = x,
                            Key = TableKey.Create(tree.Item.Selector, x.Item.Selector, properties)
                        };
                    }).GroupBy(x => x.Key);

            foreach (var group in targetEls)
            {
                var converted = group.Select(x => JObject.FromObject(x.Properties)).ToList();

                if (converted.Count() > 1)
                {
                    var objects = new JArray();

                    foreach (var obj in converted)
                    {
                        objects.Add(obj);
                    }
                    if (objects.HasValues)
                        target.Add(group.Key.ToString(), objects);
                }
                else
                {
                    if (converted.Any(x => x.HasValues))
                        target.Add(group.Key.ToString(), converted.FirstOrDefault());
                }
            }

            return target;
        }

        private void AddObject(string key, ITree<BaseNode> tree, IEnumerable<IDictionary<string, string>> els)
        {
            foreach (var el in els)
            {
                //var tableKey = TableKey.Create(tree, key, el);
            }
        }

        string Clear(string @base)
        {
            return @base.Replace("-", "_").Replace(".", "_").Replace("[", "").Replace("]", "");
        }
        private Dictionary<string, string> GetProperty(BaseNode gNode)
        {
            Dictionary<string, string> dList = new Dictionary<string, string>();

            foreach (var el in gNode.Attributes)
            {
                var key = this.Clear(el.Key);
                if (!dList.ContainsKey(key) && !string.IsNullOrWhiteSpace(el.Value))
                    dList.Add(key, el.Value);
            }

            if (!string.IsNullOrWhiteSpace(gNode.Text))
                dList.Add(this.Clear(gNode.Selector), gNode.Text);

            return dList;
        }
    }

    #region OldConverter

    //class Converter
    //{
    //    private List<Table> Tables { get; set; } = new List<Table>();

    //    public IEnumerable<Table> Convert(ITree<BaseNode> tree)
    //    {
    //        this.ConvertNode(tree, 0);
    //        return this.Tables;
    //    }

    //    public IEnumerable<Table> GetTables()
    //    {
    //        return this.Tables.Where(x => x.Properties != null && x.Properties.Any() && x.Properties.Any(e => e?.Values?.Any() ?? false));
    //    }
    //    private IDictionary<string, string> ConvertNode(ITree<BaseNode> tree, int id)
    //    {
    //        var subElements = tree.Children.Where(x => (x?.Children?.Any() ?? false)).ToList();
    //        var voidChildren = tree.Children.Where(x => !(x?.Children?.Any() ?? false)).ToList();

    //        if (subElements.Any())
    //        {
    //            foreach (var group in subElements.GroupBy(x => x.Item.Selector))
    //            {
    //                var @object = @group.Select(this.ConvertNode);
    //                this.AddObject(group.Key, tree, @object);
    //            }
    //        }

    //        if (voidChildren.Any())
    //        {
    //            var collection = new MaybeDictionary<string, string>();

    //            foreach (var group in voidChildren.GroupBy(x => x.Item.Selector))
    //            {
    //                if (@group.Count() == 1 && (this.Tables?.All(x => x.Key.Name != @group.Key) ?? true))
    //                {
    //                    var obj = this.GetProperty(@group.FirstOrDefault()?.Item);

    //                    foreach (var (key, value) in obj.Where(x => !collection.ContainsKey(x.Key)))
    //                        collection.Add(key, value);
    //                    var nodeProps = this.GetProperty(tree.Item);
    //                    foreach (var (key, value) in nodeProps.Where(x => !collection.ContainsKey(x.Key)))
    //                        collection.Add(key, value);
    //                }
    //                else
    //                {
    //                    this.AddObject(group.Key, tree, @group.Select(x => this.GetProperty(x.Item)).Where(x => x.Any(e => e.Value != null)));
    //                }
    //            }

    //            return collection;
    //        }

    //        return null;

    //    }

    //    private void AddObject(string key, ITree<BaseNode> tree, IEnumerable<IDictionary<string, string>> els)
    //    {
    //        foreach (var el in els)
    //            if (el != null)
    //                this.AddObject(TableKey.Create(tree, key, el), el);
    //    }

    //    private void AddObject(TableKey tableKey, IDictionary<string, string> row)
    //    {
    //        if (row == null)
    //            return;

    //        var targetTable = this.FindTable(tableKey);

    //        if (targetTable == null)
    //        {
    //            targetTable = new Table(tableKey, new List<IDictionary<string, string>>());
    //            this.Tables.Add(targetTable);
    //        }

    //        if (!targetTable.Properties.Any(x => row.Values.All(e => x.Values.Contains(e))))
    //        {
    //            targetTable.Properties.Add(row);
    //        }
    //        else
    //        {

    //        }
    //    }

    //    private Table FindTable(TableKey key)
    //    {
    //        return this.Tables.FirstOrDefault(x => x.Key.Equals(key));
    //    }

    //    private Dictionary<string, string> GetProperty(BaseNode gNode)
    //    {
    //        Dictionary<string, string> dList = new Dictionary<string, string>();

    //        foreach (var el in gNode.Attributes)
    //        {
    //            var key = el.Key.Replace("-", "_");
    //            if (!dList.ContainsKey(key))
    //                dList.Add(key, el.Value);
    //        }

    //        dList.Add(gNode.Selector.Replace("-", "_"), gNode.Text);
    //        return dList;
    //    }
    //}


    #endregion

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

        public TableKey(Simhash propertyHash)
        {
            var def = "Default";
            this.PropertyHash = propertyHash;
            this.Name = def;
            this.ParentKey = def;
        }

        public Simhash PropertyHash { get; }
        public string Name { get; }
        public string ParentKey { get; }

        private const int EqualsDistance = 15;

        public static TableKey Create(string parentSelector, string key, IDictionary<string, string> els)
        {
            var simhash = new Simhash(Simhash.HashingType.Jenkins);
            simhash.GenerateSimhash(els.Keys.ToList());
            return new TableKey(key, parentSelector, simhash);
        }


        public bool Equals(TableKey other)
        {
            var distance = this.PropertyHash.distance(other.PropertyHash);
            return distance < 20 && other.ParentKey == this.ParentKey && string.Equals(this.Name, other.Name);
        }

        public override int GetHashCode()
        {
            return $"{this.Name}{this.ParentKey}{this.PropertyHash.value}".GetHashCode(StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is TableKey other && this.Equals(other);
        }

        public override string ToString()
        {
            return $"{this.ParentKey}_{this.Name}_{this.PropertyHash.value.GetHashCode()}";
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