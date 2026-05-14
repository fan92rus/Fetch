using Funny.WebScrape.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Funny.WebScrape
{
    public static class DI
    {
        public static IServiceCollection AddWebScrapeCore(this IServiceCollection services)
        {
            services.AddSingleton<HtmlToMarkdownConverter>();
            return services;
        }
    }
}
