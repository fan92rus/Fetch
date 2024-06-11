namespace UniversalProductScraper.Graph
{
    internal interface ITreeBuilder<T> where T : class
    {
        ITree<T> Create();
        ITree<T> Create(T root);
        ITree<T> Create(T root, ITree<T> parent);
        ITree<T> Create(ITree<T> @base);
    }

}
