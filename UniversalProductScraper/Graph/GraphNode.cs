using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalProductScraper.Graph
{
    using System.Linq;
    using Newtonsoft.Json;
    using UniversalProductScraper.Models;

    public interface ITree<T> : IEquatable<T>
    {
        T Item { get; set; }
        ICollection<ITree<T>> Children { get; set; }
        IEnumerable<T> GetCollection();

        [JsonIgnore]
        ITree<T> Parent { get; set; }
        bool Contains(T item);
        bool Contains(ITree<T> item);
        bool Contains(Func<T, bool> func);
        void Add(ITree<T> item);
        void AddRange(IEnumerable<ITree<T>> items);
        void Add(T item);
        bool Remove(T item);
        bool Remove(ITree<T> item);
    }

    public interface ITreeBuilder<T> where T : class
    {
        ITree<T> Create();
        ITree<T> Create(T root);
        ITree<T> Create(T root, ITree<T> parent);
        ITree<T> Create(ITree<T> @base);
    }

    public class TreeBuilder<T> : ITreeBuilder<T> where T : class
    {
        public ITree<T> Create() => new Tree<T>();
        public ITree<T> Create(T root) => new Tree<T>(root);
        public ITree<T> Create(T root, ITree<T> parent) => new Tree<T>(root, parent);
        public ITree<T> Create(ITree<T> @base) => new Tree<T>(@base);
    }

    class Tree<T> : ITree<T> where T : class
    {
        public Tree() => this.Children = new List<ITree<T>>();
        public Tree(T root) : this() => this.Item = root;
        public Tree(T root, ITree<T> parent) : this(root)
        {
            if (parent != this) this.Parent = parent;
        }
        public Tree(ITree<T> @base)
        {
            this.Children = @base.Children;
            this.Item = @base.Item;
            this.Parent = @base.Parent;
        }

        public void Add(ITree<T> item) => this.Children?.Add(item);

        public void AddRange(IEnumerable<ITree<T>> items)
        {
            foreach (var item in items) this.Add(item);
        }

        public void Add(T item) => this.Children?.Add(new Tree<T>(item, this));

        public bool Remove(T item)
        {
            var isRemoved = item == this.Item && this.Parent != null && this.Parent.Remove((ITree<T>)this);
            if (isRemoved) return true;

            var target = this.Children.FirstOrDefault(x => x.Item == item);
            if (target != null)
            {
                isRemoved = this.Children.Remove(target);
            }

            return isRemoved;
        }

        public bool Remove(ITree<T> item)
        {
            var target = this.Children.FirstOrDefault(x => x == item);
            return this.Children.Remove(target);
        }

        public T Item { get; set; }
        public ITree<T> Parent { get; set; }
        public ICollection<ITree<T>> Children { get; set; }
        public ITree<T> Find(Func<T, bool> item) => this.Children.FirstOrDefault(x => item.Invoke(x.Item));

        private bool CheckThis(T item) => this.Item != null && this.Item.Equals(item);
        private bool CheckThis(ITree<T> tree)
        {
            var equalThis = this.CheckThis(tree.Item);
            //var equalsChildren = tree.Children.All(x => this.Children.Any(e => e == x));
            var nonEqualChildren = !tree.Children.Select(x => x.Item).Except(this.Children.Select(x => x.Item)).Any();
            return equalThis && nonEqualChildren;
        }

        public bool Contains(Func<T, bool> func) => (this.Item != null && func.Invoke(this?.Item)) || (this.Children != null && this.Children.Any(x => x.Contains(func)));
        public bool Contains(T item) => this.CheckThis((T)item) || (this.Children != null && this.Children.Any(x => x.Contains(item)));
        public bool Contains(ITree<T> item) => this.CheckThis(item) || this.Children.Any(x => x.Contains(item));

        public IEnumerable<T> GetCollection()
        {
            var list = new List<T> { this.Item };

            list.AddRange(this.Children.SelectMany(child => child.GetCollection()));

            return list;
        }

        public static implicit operator T(Tree<T> tree) => tree.Item;
        protected bool Equals(ITree<T> other) => EqualityComparer<T>.Default.Equals(this.Item, other.Item) && Equals(this.Children, other.Children);
        public bool Equals(T obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((ITree<T>)obj);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(this.Item) * 397) ^ (this.Children != null ? this.Children.GetHashCode() : 0);
            }
        }
    }

    //class EnumerableTree<T> : Tree<T>, IEnumerable<ITree<T>> where T : class
    //{

    //    public IEnumerator<ITree<T>> GetEnumerator()
    //    {
    //        yield return this;

    //        foreach (Tree<T> child in this.Children)
    //        {
    //            var enumerator = (child as EnumerableTree<T>).GetEnumerator();

    //            while (enumerator.MoveNext())
    //            {
    //                yield return enumerator.Current;
    //            }
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return this.GetEnumerator();
    //    }
    //}
    public class BaseNode : IEquatable<BaseNode>
    {
        public DomNodeType Type { get; set; }
        public string Selector { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }
        public string Text { get; set; }

        string Clear(string @base)
        {
            return @base.Replace("-", "_").Replace(".", "_").Replace("[", "").Replace("]", "");
        }

        public Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> dList = new Dictionary<string, string>();

            foreach (var el in this.Attributes)
            {
                var key = this.Clear(el.Key);
                if (!dList.ContainsKey(key) && !string.IsNullOrWhiteSpace(el.Value))
                    dList.Add(key, el.Value);
            }

            if (!string.IsNullOrWhiteSpace(this.Text))
                dList.Add(this.Clear(this.Selector), this.Text);

            return dList;
        }

        public bool Equals(BaseNode other)
        {
            bool equals = this.Type == other.Type && this.Selector == other.Selector && ((this.Text != null && other.Text != null && this.Text.Equals(other.Text, StringComparison.InvariantCultureIgnoreCase)) || this.Text == other.Text) && !this.Attributes.Except(other.Attributes).Any();
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseNode)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)this.Type;
                hashCode = (hashCode * 397) ^ (this.Selector != null ? this.Selector.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Attributes != null ? this.Attributes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Text != null ? this.Text.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
