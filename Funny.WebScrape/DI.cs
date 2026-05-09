using Funny.WebScrape.Converters;
using Funny.WebScrape.Loaders;
using Microsoft.Extensions.DependencyInjection;

namespace Funny.WebScrape
{
    public static class DI
    {
        public static IServiceCollection AddWebScrapingServices(this IServiceCollection services)
        {
            services.AddSingleton<ILoaderFactory, LoaderFactory>();
            services.AddSingleton<HtmlToMarkdownConverter>();

            return services;
        }
    }
}
