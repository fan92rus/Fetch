using System;
using Microsoft.Extensions.DependencyInjection;

namespace UniversalProductScraper.Loaders
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
            return loadingType switch
            {
                LoadingType.HttpRequest => _serviceProvider.GetService<RequestWebLoader>(),
                LoadingType.Selenium => _serviceProvider.GetService<SeleniumLoader>(),
                _ => throw new ArgumentOutOfRangeException(nameof(loadingType), loadingType, null)
            };
        }
    }
}
