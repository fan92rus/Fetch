using System;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace UniversalProductScraper.Loaders
{
    public class SeleniumLoader : IWebLoader
    {
        private readonly IWebDriver _driver;

        public SeleniumLoader(IWebDriver driver)
        {
            _driver = driver;
        }

        public string GetPageContent(string uri)
        {
            _driver.Navigate().GoToUrl(uri);
            WaitForPageLoad();
            return _driver.PageSource;
        }

        protected void WaitForPageLoad()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(50));
            wait.Until(driver =>
                ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        public IHtmlDocument LoadPageFromString(string text)
        {
            var config = Configuration.Default.WithDefaultLoader().WithCss();
            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            return parser.ParseDocument(text);
        }

        public IHtmlDocument GetPage(string url) => LoadPageFromString(GetPageContent(url));
    }
}