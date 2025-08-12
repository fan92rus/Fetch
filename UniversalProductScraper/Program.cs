using System;
using OpenQA.Selenium;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;
using Funny.WebScrape;
using Funny.WebScrape.Loaders;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.Chrome;
using UniversalProductScraper.WebApi;

namespace UniversalProductScraper
{
    static class DI
    {
        public static IServiceProvider ServiceProvider { get; }

        static DI()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWebScrapingServices();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }

    class Program
    {
        public static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var loaderFactory = DI.ServiceProvider.GetService<ILoaderFactory>();
            var loader = loaderFactory.CreateLoader(LoadingType.HttpRequest);

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

}