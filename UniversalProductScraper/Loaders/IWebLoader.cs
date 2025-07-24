namespace UniversalProductScraper.Loaders
{
    using AngleSharp.Html.Dom;

    public interface IWebLoader
    {
        string GetPageContent(string uri);
        IHtmlDocument LoadPageFromString(string text);
        IHtmlDocument GetPage(string url);
    }
}