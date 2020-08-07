namespace UniversalProductScraper.Loaders
{
    using AngleSharp.Html.Dom;

    internal interface IWebLoader
    {
        string GetPageContent(string uri);
        IHtmlDocument LoadPageFromString(string text);
    }
}