using System;
using System.Collections.Generic;

namespace UniversalProductScraper.Graph
{
    using System.Linq;

    internal class Tree<T> : ITree<T> where T : class
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

}
