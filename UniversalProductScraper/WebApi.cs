namespace UniversalProductScraper
{
    using System;

    using AngleSharp;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;

    using RestSharp;

    internal interface IWebApi
    {
        IHtmlDocument LoadPage(string uri);
    }
    internal class WebApi : IWebApi
    {
        public IHtmlDocument LoadPage(string uri)
        {
            var isCreate = Uri.TryCreate(uri, UriKind.Absolute, out var target);
            if (!isCreate)
                throw new ArgumentException("uri is invalid");

            var rc = new RestClient();

            var resp = rc.Execute(new RestRequest(target));

            var config = Configuration.Default.WithDefaultLoader().WithCss().WithJs();

            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var doc = parser.ParseDocument(resp.Content);

            return doc;
        }
    }
}