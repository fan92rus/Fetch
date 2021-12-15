using System;
using System.Collections;
using System.Collections.Generic;

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
        ITree<T> Find(Func<T, bool> func);
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
        public Tree()
        {
            Children = new List<ITree<T>>();
        }

        public Tree(T root) : this()
        {
            Item = root;
        }

        public Tree(T root, ITree<T> parent) : this(root)
        {
            if (parent != this) Parent = parent;
        }
        public Tree(ITree<T> @base)
        {
            Children = @base.Children;
            Item = @base.Item;
            Parent = @base.Parent;
        }

        public void Add(ITree<T> item) => Children?.Add(item);

        public void AddRange(IEnumerable<ITree<T>> items)
        {
            foreach (var item in items) Add(item);
        }

        public void Add(T item) => Children?.Add(new Tree<T>(item, this));

        public bool Remove(T item)
        {
            var isRemoved = item == Item && Parent != null && Parent.Remove((ITree<T>)this);
            if (isRemoved) return true;

            var target = Children.FirstOrDefault(x => x.Item == item);
            if (target != null)
            {
                isRemoved = Children.Remove(target);
            }

            return isRemoved;
        }

        public bool Remove(ITree<T> item)
        {
            var target = Children.FirstOrDefault(x => x == item);
            return Children.Remove(target);
        }

        public T Item { get; set; }
        public ITree<T> Parent { get; set; }
        public ICollection<ITree<T>> Children { get; set; }
        //public ITree<T> Find(Func<T, bool> item) => this.Children.FirstOrDefault(x => item.Invoke(x.Item));

        public ITree<T> Find(Func<T, bool> item)
        {
            if (item(this))
                return this;

            foreach (var child in Children)
            {
                var founded = child.Find(item);
                if (founded != null)
                    return founded;
            }

            return null;
        }

        private bool CheckThis(T item) => Item != null && Item.Equals(item);
        private bool CheckThis(ITree<T> tree)
        {
            var equalThis = CheckThis(tree.Item);
            //var equalsChildren = tree.Children.All(x => this.Children.Any(e => e == x));
            var nonEqualChildren = !tree.Children.Select(x => x.Item).Except(Children.Select(x => x.Item)).Any();
            return equalThis && nonEqualChildren;
        }

        public bool Contains(Func<T, bool> func) => (Item != null && func.Invoke(this?.Item)) || (Children != null && Children.Any(x => x.Contains(func)));
        public bool Contains(T item) => CheckThis(item) || (Children != null && Children.Any(x => x.Contains(item)));
        public bool Contains(ITree<T> item) => CheckThis(item) || Children.Any(x => x.Contains(item));

        public IEnumerable<T> GetCollection()
        {
            var list = new List<T> { Item };

            list.AddRange(Children.SelectMany(child => child.GetCollection()));

            return list;
        }

        public static implicit operator T(Tree<T> tree) => tree.Item;
        protected bool Equals(ITree<T> other) => EqualityComparer<T>.Default.Equals(Item, other.Item) && Equals(Children, other.Children);
        public bool Equals(T obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ITree<T>)obj);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Item) * 397) ^ (Children != null ? Children.GetHashCode() : 0);
            }
        }
    }

    class EnumerableTree<T> : Tree<T>, ITree<T>, IEnumerable<ITree<T>> where T : class
    {

        public IEnumerator<ITree<T>> GetEnumerator()
        {
            yield return this;

            foreach (Tree<T> child in Children)
            {
                var enumerator = (child as EnumerableTree<T>).GetEnumerator();

                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class BaseNode : IEquatable<BaseNode>
    {
        public DomNodeType Type { get; set; }
        public string Selector { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }
        public string Text { get; set; }

        string Clear(string @base) => @base.Replace("-", "_").Replace(".", "_").Replace("[", "").Replace("]", "");

        public Dictionary<string, string> GetProperties()
        {
            var dList = new Dictionary<string, string>();

            foreach (var el in Attributes)
            {
                var key = Clear(el.Key);
                if (!dList.ContainsKey(key) && !string.IsNullOrWhiteSpace(el.Value))
                    dList.Add(key, el.Value);
            }

            if (!string.IsNullOrWhiteSpace(Text))
                dList.Add(Clear(Selector), Text);

            return dList;
        }

        public bool Equals(BaseNode other)
        {
            var equals = Type == other.Type && Selector == other.Selector && ((Text != null && other.Text != null && Text.Equals(other.Text, StringComparison.InvariantCultureIgnoreCase)) || Text == other.Text) && !Attributes.Except(other.Attributes).Any();
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BaseNode)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ (Selector != null ? Selector.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Attributes != null ? Attributes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}
