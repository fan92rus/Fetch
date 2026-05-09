namespace Funny.WebScrape.Loaders
{
    public interface IWebLoader
    {
        Task<string> GetPageContentAsync(string uri);
    }
}
