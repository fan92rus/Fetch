using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Funny.WebScrape.Loaders
{
    public class SeleniumLoader : IWebLoader
    {
        private readonly IWebDriver _driver;

        public SeleniumLoader(IWebDriver driver)
        {
            _driver = driver;
        }

        public Task<string> GetPageContentAsync(string uri)
        {
            _driver.Navigate().GoToUrl(uri);
            WaitForPageLoad();
            return Task.FromResult(_driver.PageSource);
        }

        protected void WaitForPageLoad()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(50));
            wait.Until(driver =>
                ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
    }
}
