using System;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;
using Funny.WebScrape;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.Chrome;
using Fetch.Server.WebApi;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace Fetch.Server
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
            new DriverManager().SetUpDriver(new ChromeConfig());

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var server = new WebServer(c => c.WithUrlPrefix("http://*:5020"))
                .WithCors()
                .WithWebApi("/", m => m
                    .WithController<UrlParserController>(() => new UrlParserController(DI.ServiceProvider)));

            await server.RunAsync();
        }
    }

}