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

        inputHtml = PreprocessForMdream(inputHtml);

        var result = await _mdream.ConvertAsync(inputHtml, originUrl: url);
        var markdown = result.Markdown ?? string.Empty;

        if (!images)
            markdown = ImagePattern.Replace(markdown, "");

        return markdown;
    }

    private static string PreprocessForMdream(string html)
    {
        // mdream truncates text after <br> in fragments without <html><body> wrapper
        if (!html.Contains("<html", StringComparison.OrdinalIgnoreCase))
            html = $"<html><body>{html}</body></html>";

        // mdream ignores <br> tags; replace with paragraph breaks to force line breaks
        html = html.Replace("<br>", "</p><p>")
                   .Replace("<br/>", "</p><p>")
                   .Replace("<br />", "</p><p>");

        return html;
    }
}
