using Funny.WebScrape.Converters;
using Xunit;

namespace Funny.WebScrape.Tests;

public class SmartReaderTests
{
    private static string LoadFixture(string name)
    {
        var path = Path.Combine("Fixtures", name);
        return File.ReadAllText(path);
    }

    [Fact]
    public void Extract_SimpleArticle_ReturnsMainContent()
    {
        var html = LoadFixture("simple-article.html");
        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html);

        Assert.Contains("Introduction to SmartReader", result);
        Assert.Contains("scoring algorithm", result);
        Assert.DoesNotContain("My Website", result); // header
        Assert.DoesNotContain("Related Articles", result); // sidebar
        Assert.DoesNotContain("Copyright 2025", result); // footer
    }

    [Fact]
    public void Extract_SimpleArticle_ContainsAllParagraphs()
    {
        var html = LoadFixture("simple-article.html");
        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html);

        Assert.Contains("new approach to content extraction", result);
        Assert.Contains("assigns scores to HTML elements", result);
        Assert.Contains("adapts to different page layouts", result);
        Assert.Contains("validated against thousands", result);
        Assert.Contains("machine learning enhancements", result);
    }

    [Fact]
    public void Extract_ComplexPage_ReturnsArticleContent()
    {
        var html = LoadFixture("complex-page.html");
        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html);

        Assert.Contains("Future of Web Content Extraction", result);
        Assert.Contains("rapidly evolving landscape", result);
        Assert.DoesNotContain("Categories", result); // left sidebar
        Assert.DoesNotContain("Popular Posts", result); // right sidebar
        Assert.DoesNotContain("Copyright 2025 Tech Blog", result); // footer
    }

    [Fact]
    public void Extract_ComplexPage_IgnoresComments()
    {
        var html = LoadFixture("complex-page.html");
        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html);

        Assert.DoesNotContain("Great article!", result);
        Assert.DoesNotContain("Have you tried using ML", result);
    }

    [Fact]
    public void Extract_ComplexPage_IgnoresAds()
    {
        var html = LoadFixture("complex-page.html");
        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html);

        Assert.DoesNotContain("BUY NOW", result);
        Assert.DoesNotContain("ads.example.com", result);
    }

    [Fact]
    public void Extract_ShortArticle_ReturnsContent()
    {
        var html = LoadFixture("short-article.html");
        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html);

        Assert.Contains("brief article about testing", result);
        Assert.Contains("short articles deserve", result);
    }

    [Fact]
    public void Extract_ShortArticle_IgnoresNav()
    {
        var html = LoadFixture("short-article.html");
        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html);

        Assert.DoesNotContain("Copyright 2025", result);
    }

    [Fact]
    public void Extract_EmptyHtml_ReturnsEmpty()
    {
        var reader = new SmartReader();
        var result = reader.ExtractArticleContent("<html><body></body></html>");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Extract_NoArticleTag_UsesBestScoringElement()
    {
        var html = """
            <html><body>
                <nav><a href="/">Home</a><a href="/about">About</a></nav>
                <div class="post-body">
                    <p>This is the main content of the page, which is quite long and contains multiple sentences about various topics. The content should be detected even without an article tag, because the scoring algorithm evaluates text density and paragraph count.</p>
                    <p>Here is another paragraph to increase the text density further. With enough text, the div element should receive a high enough score to be selected as the main content container by the SmartReader algorithm.</p>
                    <p>A third paragraph adds more weight to this content block, making it clearly distinguishable from the navigation and other non-content elements on the page.</p>
                </div>
                <footer><p>Footer text</p></footer>
            </body></html>
            """;

        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html);

        Assert.Contains("main content of the page", result);
        Assert.DoesNotContain("Footer text", result);
    }

    [Fact]
    public void Extract_WithStripDiscussion_RemovesForumMetadata()
    {
        var html = """
            <html><body>
                <div class="message first-post">
                    <div class="user-info">User123</div>
                    <div class="post-info">Posted: Jan 1</div>
                    <div class="post-body">
                        <p>This is the original post content with enough words to be considered significant text content for the SmartReader extraction algorithm to pick up as the main content of this forum thread.</p>
                    </div>
                </div>
                <div class="pagination">1 2 3</div>
                <div class="reply-form"><textarea></textarea></div>
            </body></html>
            """;

        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html, stripDiscussion: true);

        Assert.Contains("original post content", result);
        Assert.DoesNotContain("User123", result);
        Assert.DoesNotContain("Posted: Jan 1", result);
    }

    [Fact]
    public void Extract_WithoutStripDiscussion_ReturnsPostContent()
    {
        var html = """
            <html><body>
                <div class="message first-post">
                    <div class="user-info">User123</div>
                    <div class="post-body">
                        <p>This is the original post content with enough words to be considered significant text content for the SmartReader extraction algorithm to pick up as the main content of this forum thread.</p>
                    </div>
                </div>
            </body></html>
            """;

        var reader = new SmartReader();
        var result = reader.ExtractArticleContent(html, stripDiscussion: false);

        Assert.Contains("original post content", result);
    }
}
