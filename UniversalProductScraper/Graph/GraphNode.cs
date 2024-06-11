using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UniversalProductScraper.Graph
{

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
}
