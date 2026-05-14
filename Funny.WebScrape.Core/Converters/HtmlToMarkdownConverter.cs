using System.Text.RegularExpressions;
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
            : new SmartReader().ExtractArticleContent(html);

        if (string.IsNullOrEmpty(inputHtml))
            inputHtml = html;

        var result = await _mdream.ConvertAsync(inputHtml, originUrl: url);
        var markdown = result.Markdown ?? string.Empty;

        if (!images)
            markdown = ImagePattern.Replace(markdown, "");

        return markdown;
    }
}
