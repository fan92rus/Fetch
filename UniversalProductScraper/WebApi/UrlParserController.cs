using System;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.Extensions.DependencyInjection;
using NBoilerpipe.Extractors;
using Newtonsoft.Json;
using UniversalProductScraper.Loaders;

namespace UniversalProductScraper.WebApi;

public class UrlParserController : WebApiController
{
    private readonly IServiceProvider _serviceProvider;

    public UrlParserController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [Route(HttpVerbs.Get, "/parse/url")]
    public ParseResponce ParseUrl([QueryField] string url, [QueryField] LoadingType loadingType)
    {
        var loaderFactory = _serviceProvider.GetService<ILoaderFactory>();
        var loader = loaderFactory.CreateLoader(loadingType);
        var result = loader.GetPageContent(url);
        
        return new ParseResponce
        {
            Content = result,
        };
    }
}

public class ParseResponce
{
    public string Content { get; init; }
}