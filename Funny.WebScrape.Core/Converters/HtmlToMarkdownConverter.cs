using System.Text.RegularExpressions;
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
    private static readonly Regex ImagePattern = new(@"!\[[^\]]*\]\([^)]+\)\s*", RegexOptions.Compiled);
    private readonly MdreamConverter _mdream = new();

    public async Task<string> ConvertAsync(string html, string url, ConversionMode mode, bool images = false)
    {
        var inputHtml = mode == ConversionMode.FullPage
            ? html
            : ExtractWithFallback(html);

        if (string.IsNullOrEmpty(inputHtml))
            inputHtml = html;

        var result = await _mdream.ConvertAsync(inputHtml, originUrl: url);
        var markdown = result.Markdown ?? string.Empty;

        if (!images)
            markdown = ImagePattern.Replace(markdown, "");

        return markdown;
    }

    private static string ExtractWithFallback(string html)
    {
        var smartResult = new SmartReader().ExtractArticleContent(html);
        if (!string.IsNullOrEmpty(smartResult))
            return smartResult;

        return ExtractArticleContent(html);
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
