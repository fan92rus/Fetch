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
                    var remoteUrl = Environment.GetEnvironmentVariable("REMOTE_BROWSER_URL");

                    if (!string.IsNullOrEmpty(remoteUrl))
                    {
                        var options = new OpenQA.Selenium.Chrome.ChromeOptions();
                        var driver = new OpenQA.Selenium.Remote.RemoteWebDriver(new Uri(remoteUrl), options.ToCapabilities());
                        return new SeleniumLoader(driver);
                    }
                    else
                    {
                        var driver = new OpenQA.Selenium.Chrome.ChromeDriver();
                        return new SeleniumLoader(driver);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadingType), loadingType, null);
    }
    }
}
}
