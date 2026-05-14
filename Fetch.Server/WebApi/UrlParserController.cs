using System;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Funny.WebScrape.Converters;
using Funny.WebScrape.Loaders;
using Microsoft.Extensions.DependencyInjection;

namespace Fetch.Server.WebApi;

public class UrlParserController : WebApiController
{
    private readonly IServiceProvider _serviceProvider;

    public UrlParserController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [Route(HttpVerbs.Get, "/parse/url")]
    public async Task<ParseResponse> ParseUrl(
        [QueryField] string url,
        [QueryField] LoadingType loadingType,
        [QueryField] ConversionMode mode = ConversionMode.Article,
        [QueryField] bool images = false)
    {
        var loaderFactory = _serviceProvider.GetService<ILoaderFactory>();
        var converter = _serviceProvider.GetService<HtmlToMarkdownConverter>();

        var loader = loaderFactory.CreateLoader(loadingType);
        var html = await loader.GetPageContentAsync(url);
        var markdown = await converter.ConvertAsync(html, url, mode, images);

        return new ParseResponse
        {
            Content = markdown,
        };
    }
}

public class ParseResponse
{
    public string Content { get; init; }
}
