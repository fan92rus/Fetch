using AngleSharp;
using AngleSharp.ContentExtraction;
using AngleSharp.Html.Parser;
using MdreamWrapper;

namespace Funny.WebScrape.Converters;

public enum ConversionMode
{
    Article,
    FullPage
}

public class HtmlToMarkdownConverter
{
    private readonly MdreamConverter _mdream = new();

    public async Task<string> ConvertAsync(string html, string url, ConversionMode mode)
    {
        var inputHtml = mode == ConversionMode.Article
            ? ExtractArticleContent(html)
            : html;

        var result = await _mdream.ConvertAsync(inputHtml, originUrl: url);
        return result.Markdown ?? string.Empty;
    }

    private static string ExtractArticleContent(string html)
    {
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var parser = context.GetService<IHtmlParser>();
        var document = parser.ParseDocument(html);

        var extractor = new ContentExtractor();
        extractor.Extract(document);

        return document.Body?.InnerHtml ?? html;
    }
}
