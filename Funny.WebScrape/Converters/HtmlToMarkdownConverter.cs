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

    public async Task<string> ConvertAsync(string html, string url, ConversionMode mode, bool images = false, bool stripDiscussion = false)
    {
        var inputHtml = mode == ConversionMode.FullPage
            ? html
            : ExtractWithFallback(html, stripDiscussion);

        if (string.IsNullOrEmpty(inputHtml))
            inputHtml = html;

        var result = await _mdream.ConvertAsync(inputHtml, originUrl: url);
        var markdown = result.Markdown ?? string.Empty;

        if (!images)
            markdown = ImagePattern.Replace(markdown, "");

        return markdown;
    }

    private static string ExtractWithFallback(string html, bool stripDiscussion)
    {
        var smartResult = new SmartReader().ExtractArticleContent(html, stripDiscussion);

        if (stripDiscussion && !string.IsNullOrEmpty(smartResult))
        {
            smartResult = StripDiscussion(smartResult);
        }

        if (!string.IsNullOrEmpty(smartResult))
            return smartResult;

        var fallback = ExtractArticleContent(html);
        return stripDiscussion ? StripDiscussion(fallback) : fallback;
    }

    private static string StripDiscussion(string html)
    {
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var parser = context.GetService<IHtmlParser>();
        var document = parser.ParseDocument(html);

        if (document.Body == null)
            return html;

        var body = document.Body;

        // Forum reply containers — keep only first child with text, remove siblings
        foreach (var selector in DiscussionSelectors)
        {
            var elements = body.QuerySelectorAll(selector);
            if (elements.Length > 1)
            {
                var first = elements[0];
                for (var i = 1; i < elements.Length; i++)
                    elements[i].Remove();
            }
        }

        // Remove discussion-related blocks entirely
        foreach (var selector in RemoveSelectors)
        {
            foreach (var el in body.QuerySelectorAll(selector))
                el.Remove();
        }

        return body.InnerHtml;
    }

    // Selectors for reply/message blocks — keep only the first match
    private static readonly string[] DiscussionSelectors =
    [
        ".post_wrap",
        ".message",
        ".postBody",
        ".post-body",
        ".post_body",
        ".topic-post",
        ".topic-item",
        ".comment-item",
        ".thread-item",
    ];

    // Selectors for blocks to always remove when stripDiscussion is enabled
    private static readonly string[] RemoveSelectors =
    [
        // Reply forms
        ".reply-form",
        "#reply-form",
        ".quickreply",
        "#quick-reply",
        "#replybox",
        ".post-form",
        ".create-post",
        ".postcontrols",
        ".post-links",
        // Pagination
        ".pagination",
        ".page-buttons",
        ".forum-pages",
        ".topic-pages",
        ".pager",
        "#topic-pages",
        ".page-breadcrumb",
        // User info blocks
        ".user-info",
        ".poster_info",
        ".postprofile",
        ".author-info",
        ".post-author",
        ".post-header",
        ".post-info",
        ".post-meta",
        ".post-buttons",
        ".t-post-buttons",
        ".post-head",
        ".post_head",
        ".signature",
        ".user-sig",
        ".post-signature",
        ".online-indicator",
        ".post-controls",
        ".posted_since",
        ".rank_img",
        // Polls
        ".poll",
        // Breadcrumbs and forum nav
        ".breadcrumbs",
        ".forum-nav",
        ".topic-title",
        ".topic-actions",
        ".bottom_info",
        ".footer-bottom-links",
        // Avatar images
        ".avatar",
    ];

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
