namespace Funny.WebScrape.Loaders
{
    public interface ILoaderFactory
    {
        IWebLoader CreateLoader(LoadingType loadingType);
    }

    public class LoaderFactory : ILoaderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public LoaderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IWebLoader CreateLoader(LoadingType loadingType)
        {
            switch (loadingType)
            {
                case LoadingType.HttpRequest:
                    return new RequestWebLoader();
                case LoadingType.Selenium:
                    return new FlareSolverrLoader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadingType), loadingType, null);
            }
        }
    }
}
