namespace UniversalProductScraper.Loaders
{
    using System;
    using System.Net;
    using AngleSharp;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;
    using Extentions.RestSharp;
    using Polly;
    using Polly.Retry;
    using RestSharp;

    public class RequestWebLoader : IWebLoader
    {
        public RequestWebLoader()
        {
            this.Policy = Polly.Policy
                .HandleResult<IRestResponse>(
                    (response) => (response.StatusCode == 0 || response.StatusCode == HttpStatusCode.TooManyRequests)
                                  && response.ResponseStatus != ResponseStatus.TimedOut).WaitAndRetry(
                    2,
                    retryAttempt => TimeSpan.FromSeconds(2));
        }

        public RetryPolicy<IRestResponse> Policy { get; set; }
        public virtual string GetPageContent(string uri)
        {
            var isCreate = Uri.TryCreate(uri, UriKind.Absolute, out var target);
            
            if (!isCreate)
                throw new ArgumentException("uri is invalid");
            
            var rc = new RestClient();
            var req = new RestRequest(target);
            req.AddHeader("Content-Type", "text/html; charset=utf-8");
            var resp = rc.ExecuteWitHeaders(req, this.Policy);
            
            return resp.Content;
        }


        public IHtmlDocument LoadPageFromString(string text)
        {
            var config = Configuration.Default.WithDefaultLoader().WithCss().WithJs();
            var context = BrowsingContext.New(config);
            var parser = context.GetService<IHtmlParser>();
            var doc = parser.ParseDocument(text);

            return doc;
        }
    }
}