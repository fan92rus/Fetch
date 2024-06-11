using System.Collections;
using System.Collections.Generic;

namespace UniversalProductScraper.Graph
{
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

}
