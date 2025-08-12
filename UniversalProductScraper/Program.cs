using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;
using Microsoft.Extensions.DependencyInjection;
using NBoilerpipe.Extractors;
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

            serviceCollection.AddSingleton<IWebDriver>(_ => new ChromeDriver(chromeOprions));
            serviceCollection.AddSingleton<RequestWebLoader>();
            serviceCollection.AddSingleton<IWebLoader, RequestWebLoader>();
            serviceCollection.AddSingleton<SeleniumLoader>();
            serviceCollection.AddSingleton<ILoaderFactory, LoaderFactory>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }

    class Program
    {
        public static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var loader = DI.ServiceProvider.GetService<RequestWebLoader>();
            var result =
                loader.GetPageContent(
                    "https://habr.com/ru/companies/vk/articles/200394/?roistat_visit=1861572/");
            var text = ArticleExtractor.ExtractArticle(result);

            var server = new WebServer(c => c.WithUrlPrefix("http://*:5020"))
                .WithCors()
                .WithWebApi("/", m => m
                    .WithController<UrlParserController>(() => new UrlParserController(DI.ServiceProvider)));

            await server.RunAsync();
        }
    }

    // Вспомогательный класс для хранения информации о каждом узле
    public class NodeScoreInfo
    {
        public double Score { get; set; }
        public int TextLength { get; set; }
        public Dictionary<string, int> TagCount { get; set; } = new Dictionary<string, int>();
        public int ChildCount { get; set; }
    }
}