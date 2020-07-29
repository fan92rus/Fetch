using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalProductScraper.Graph
{
    using System.Collections;
    using System.ComponentModel;
    using System.Linq;

    using QuickGraph;
    using QuickGraph.Graphviz;

    using UniversalProductScraper.Models;

    public interface ITree<T>
    {
        T Item { get; }
        IEnumerable<ITree<T>> Children { get; }
        bool Contains(T item);

        void Add(ITree<T> item);
        void Add(T item);
    }

    class Tree<T> : ITree<T>, IEnumerable<T> where T : class
    {
        public Tree(T root)
        {
            this.Item = root;
            this.Children = new List<ITree<T>>();
        }
        public void Add(ITree<T> item)
        {
            (this.Children as List<ITree<T>>)?.Add(item);
        }

        public void Add(T item)
        {
            (this.Children as List<ITree<T>>)?.Add(new Tree<T>(item));
        }

        public T Item { get; }

        public IEnumerable<ITree<T>> Children { get; }

        public bool Contains(T item)
        {
            return this.Item == item || this.Children.Any(x => x.Contains(item));
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return this.Item;

            foreach (var child in this.Children)
            {
                foreach (var item in (IEnumerable<T>)child)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
