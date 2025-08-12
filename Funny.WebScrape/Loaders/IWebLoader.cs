using AngleSharp.Html.Dom;

namespace Funny.WebScrape.Loaders
{
    public interface IWebLoader
    {
        string GetPageContent(string uri);
        IHtmlDocument LoadPageFromString(string text);
        IHtmlDocument GetPage(string url);
    }
}