using Funny.WebScrape.Loaders;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Funny.WebScrape
{
    public static class DI
    {
        public static IServiceCollection AddWebScrapingServices(this IServiceCollection services)
        {
            var chromeOptions = new ChromeOptions();
            // chromeOptions.AddArgument("--headless");

            services.AddSingleton<IWebDriver>(_ => new ChromeDriver(chromeOptions));
            services.AddSingleton<RequestWebLoader>();
            services.AddSingleton<IWebLoader, RequestWebLoader>();
            services.AddSingleton<SeleniumLoader>();
            services.AddSingleton<ILoaderFactory, LoaderFactory>();

            return services;
        }
    }
}