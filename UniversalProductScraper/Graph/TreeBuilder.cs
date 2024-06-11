namespace UniversalProductScraper.Graph
{
    internal class TreeBuilder<T> : ITreeBuilder<T> where T : class
    {
        public ITree<T> Create() => new Tree<T>();
        public ITree<T> Create(T root) => new Tree<T>(root);
        public ITree<T> Create(T root, ITree<T> parent) => new Tree<T>(root, parent);
        public ITree<T> Create(ITree<T> @base) => new Tree<T>(@base);
    }

}
