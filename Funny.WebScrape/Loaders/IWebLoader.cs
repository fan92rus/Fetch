namespace Funny.WebScrape.Loaders
{
    public interface IWebLoader
    {
        string GetPageContent(string uri);
    }
}
