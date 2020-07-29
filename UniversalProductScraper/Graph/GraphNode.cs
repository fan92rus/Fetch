using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalProductScraper.Graph
{
    using System.Collections;
    using System.ComponentModel;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using QuickGraph;
    using QuickGraph.Graphviz;

    using UniversalProductScraper.Models;
    using Type = UniversalProductScraper.Type;

    public interface ITree<T> : IEquatable<T>
    {
        T Item { get; }
        ICollection<ITree<T>> Children { get; }
        IEnumerable<T> GetCollection();
        bool Contains(T item);

        void Add(ITree<T> item);
        void Add(T item);

        bool Remove(T item);

        bool Remove(ITree<T> item);
    }


    class Tree<T> : ITree<T> where T : class
    {
        public Tree(T root)
        {
            this.Item = root;
            this.Children = new List<ITree<T>>();
        }

        public Tree(T root, ITree<T> parent) : this(root) => this.Parent = parent;

        public void Add(ITree<T> item) => (this.Children as List<ITree<T>>)?.Add(item);

        public void Add(T item) => (this.Children as List<ITree<T>>)?.Add(new Tree<T>(item, this));

        public bool Remove(T item)
        {
            var isRemoved = item == this.Item && this.Parent != null && this.Parent.Remove(this);
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

        public T Item { get; }
        private ITree<T> Parent { get; set; }
        public ICollection<ITree<T>> Children { get; }

        public bool Contains(T item) => this.Item == item || this.Children.Any(x => x.Contains(item));
        public bool Contains(ITree<T> item) => this.Children.Any(x => x == item);

        public IEnumerable<T> GetCollection()
        {
            var list = new List<T> { this.Item };

            list.AddRange(this.Children.SelectMany(child => child.GetCollection()));

            return list;
        }

        protected bool Equals(Tree<T> other)
        {
            return EqualityComparer<T>.Default.Equals(this.Item, other.Item) && Equals(this.Children, other.Children);
        }

        public bool Equals(T obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(this.Item) * 397) ^ (this.Children != null ? this.Children.GetHashCode() : 0);
            }
        }
    }


    public class BaseNode : IEquatable<BaseNode>
    {
        public Type Type { get; set; }
        public string Selector { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }
        public string Text { get; set; }

        public bool Equals(BaseNode other)
        {
            bool equals = this.Type == other.Type && this.Selector == other.Selector && this.Text == other.Text && !this.Attributes.Except(other.Attributes).Any();
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

    class TestGraphConverter
    {
        public AdjacencyGraph<Node, TaggedEdge<Node, int>> Test(Node node)
        {
            var g = new AdjacencyGraph<Node, TaggedEdge<Node, int>>();

            g.AddVertex(node);

            foreach (var child in node.Nodes)
                this.ConvertNode(child, g);

            return g;
        }

        public void ConvertNode(Node node, AdjacencyGraph<Node, TaggedEdge<Node, int>> g)
        {
            if (node.Nodes != null)
            {
                g.AddVerticesAndEdgeRange(node.Nodes.Select(x => new TaggedEdge<Node, int>(node.ParentNode, x, 0)));

                foreach (var child in node.Nodes)
                    this.ConvertNode(child, g);
            }
            else
            {
                try
                {
                    g.AddVerticesAndEdge(new TaggedEdge<Node, int>(node.ParentNode, node, 0));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }
    }
}
