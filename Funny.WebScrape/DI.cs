using Funny.WebScrape.Loaders;
using Microsoft.Extensions.DependencyInjection;

namespace Funny.WebScrape
{
    public static class DI
    {
        public static IServiceCollection AddWebScrapingServices(this IServiceCollection services)
        {
            services.AddWebScrapeCore();
            services.AddSingleton<ILoaderFactory, LoaderFactory>();

            return services;
        }
    }
}
