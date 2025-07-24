using System;
using OpenQA.Selenium;
using UniversalProductScraper.Graph;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using UniversalProductScraper.Loaders;
using UniversalProductScraper.WebApi;

namespace UniversalProductScraper
{
    static class DI
    {
        public static IServiceProvider ServiceProvider { get; }

        static DI()
        {
            var chromeOprions = new ChromeOptions();
            // chromeOprions.AddArgument("--headless");

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IDomMapper, DomMapper>();
            serviceCollection.AddSingleton<IScraper, Scraper>();
            serviceCollection.AddSingleton<ITreeConverter, TreeDictionaryConverter>();
            serviceCollection.AddSingleton<IWebDriver>(_ => new ChromeDriver(chromeOprions));
            serviceCollection.AddSingleton<RequestWebLoader>();
            serviceCollection.AddSingleton<IWebLoader, RequestWebLoader>();
            serviceCollection.AddSingleton<SeleniumLoader>();
            serviceCollection.AddSingleton<ILoaderFactory, LoaderFactory>();
            serviceCollection.AddSingleton<ScrapingService>();
            serviceCollection.AddSingleton(typeof(ITreeBuilder<>), typeof(TreeBuilder<>));

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }

    class Program
    {
        public static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var server = new WebServer(c => c.WithUrlPrefix("http://*:5020"))
                .WithCors()
                .WithWebApi("/", m => m
                    .WithController<UrlParserController>(() => new UrlParserController(DI.ServiceProvider)));

            await server.RunAsync();
        }
    }
}
