using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace Funny.WebScrape.Converters;

public class SmartReader
{
    private static readonly string[] StripTags =
        ["script", "style", "noscript", "iframe", "svg", "form", "button", "input", "textarea", "select"];

    private static readonly string[] NegativeClassPatterns =
        ["sidebar", "nav", "footer", "header", "ad", "ads", "advert", "comment", "social", "share", "widget",
         "promo", "banner", "sponsor", "related", "popup", "modal", "cookie", "newsletter", "subscription"];

    private static readonly string[] DiscussionClassPatterns =
        ["signature", "user-sig", "user-info", "post-info", "post-meta", "post-author", "post-head", "post-buttons",
         "postprofile", "online-indicator", "profile-field"];

    private static readonly string[] PositiveClassPatterns =
        ["content", "article", "post", "entry", "blog", "story", "text", "body"];

    private static readonly HashSet<string> ContentTags = ["article", "main", "section", "div", "p"];

    private static readonly Dictionary<string, int> TagScores = new()
    {
        ["ARTICLE"] = 500,
        ["MAIN"] = 400,
        ["SECTION"] = 150,
        ["DIV"] = 0,
    };

    public string ExtractArticleContent(string html, bool stripDiscussion = false)
    {
        var config = Configuration.Default;
        using var context = BrowsingContext.New(config);
        var parser = context.GetService<IHtmlParser>();
        var document = parser.ParseDocument(html);

        if (document.Body == null)
            return string.Empty;

        StripUnwantedElements(document.Body, stripDiscussion);

        var candidates = FindCandidates(document.Body);
        if (candidates.Count == 0)
            return document.Body.InnerHtml;

        var best = candidates.MaxBy(c => c.Score);
        return best?.Element.InnerHtml ?? string.Empty;
    }

    private void StripUnwantedElements(IElement root, bool stripDiscussion)
    {
        foreach (var tag in StripTags)
        {
            foreach (var el in root.QuerySelectorAll(tag).ToList())
                el.Remove();
        }

        if (stripDiscussion)
        {
            StripDiscussionElements(root);
        }

        foreach (var el in root.QuerySelectorAll("*").ToList())
        {
            if (HasNegativeMatch(el) && !HasPositiveMatch(el))
            {
                if (!HasSignificantText(el))
                    el.Remove();
            }
        }
    }

    private static void StripDiscussionElements(IElement root)
    {
        // For container selectors — keep only the first match, remove siblings
        foreach (var pattern in DiscussionContainerPatterns)
        {
            var trimmed = pattern.Trim();
            var elements = root.QuerySelectorAll($"*.{trimmed}, *[id*='{trimmed}']");
            if (elements.Length > 1)
            {
                for (var i = 1; i < elements.Length; i++)
                    elements[i].Remove();
            }
        }

        // For metadata/form selectors — always remove
        foreach (var pattern in DiscussionRemovePatterns)
        {
            var trimmed = pattern.Trim();
            foreach (var el in root.QuerySelectorAll($"*.{trimmed}, *[id*='{trimmed}']").ToList())
                el.Remove();
        }
    }

    // Container patterns — keep first match only (post/message containers)
    private static readonly string[] DiscussionContainerPatterns =
    [
        "message", "post-body", "topic-post", "topic-item", "post-wrap", "comment-item", "thread-item",
    ];

    // Patterns to always remove (metadata, forms, navigation)
    private static readonly string[] DiscussionRemovePatterns =
    [
        "post-entry", "entry-content", "signature", "user-sig", "user-info", "post-info",
        "post-meta", "post-author", "post-head", "post-buttons", "quote-content", "quote-header",
        "spoiler", "poll", "pagination", "page-numbers", "online-indicator", "profile-field",
        "reply-form", "quickreply", "post-form", "create-post",
    ];

    private static bool HasNegativeMatch(IElement el)
    {
        var cls = el.ClassName ?? "";
        var id = el.Id ?? "";
        var combined = $"{cls} {id}".ToLowerInvariant();
        return NegativeClassPatterns.Any(p => combined.Contains(p));
    }

    private static bool HasPositiveMatch(IElement el)
    {
        var cls = el.ClassName ?? "";
        var id = el.Id ?? "";
        var combined = $"{cls} {id}".ToLowerInvariant();
        return PositiveClassPatterns.Any(p => combined.Contains(p));
    }

    private static bool HasSignificantText(IElement el)
    {
        var text = el.TextContent?.Trim() ?? "";
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 50;
    }

    private List<Candidate> FindCandidates(IElement root)
    {
        var candidates = new List<Candidate>();

        foreach (var el in root.QuerySelectorAll("*"))
        {
            if (!TagScores.ContainsKey(el.TagName))
                continue;

            var text = el.TextContent?.Trim() ?? "";
            if (text.Length < 40)
                continue;

            var score = CalculateScore(el, text);
            candidates.Add(new Candidate(el, score));
        }

        return candidates;
    }

    private int CalculateScore(IElement el, string text)
    {
        var score = 0;

        if (TagScores.TryGetValue(el.TagName, out var tagScore))
            score += tagScore;

        score += ClassIdScore(el);

        var textLen = text.Length;
        score += Math.Min(textLen / 10, 200);

        var commas = text.Count(c => c == ',');
        score += commas * 5;

        var paragraphs = el.QuerySelectorAll("p");
        score += paragraphs.Length * 30;

        var links = el.QuerySelectorAll("a");
        var linkTextLen = links.Sum(a => (a.TextContent?.Length ?? 0));
        if (textLen > 0)
        {
            var linkDensity = (double)linkTextLen / textLen;
            if (linkDensity > 0.25)
                score -= (int)(linkDensity * 200);
        }

        return score;
    }

    private static int ClassIdScore(IElement el)
    {
        var score = 0;
        var cls = (el.ClassName ?? "").ToLowerInvariant();
        var id = (el.Id ?? "").ToLowerInvariant();
        var combined = $"{cls} {id}";

        foreach (var p in PositiveClassPatterns)
        {
            if (combined.Contains(p))
                score += 100;
        }

        foreach (var p in NegativeClassPatterns)
        {
            if (combined.Contains(p))
                score -= 100;
        }

        return score;
    }

    private record Candidate(IElement Element, int Score);
}
