using System.Net;
using Extensions.RestSharp;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Funny.WebScrape.Loaders
{
    public class RequestWebLoader : IWebLoader
    {
        public RequestWebLoader()
        {
            Policy = Polly.Policy
                .HandleResult<RestResponse>(
                    (response) => (response.StatusCode == 0 || response.StatusCode == HttpStatusCode.TooManyRequests)
                                  && response.ResponseStatus != ResponseStatus.TimedOut).WaitAndRetry(
                    2,
                    retryAttempt => TimeSpan.FromSeconds(2));
        }

        private RetryPolicy<RestResponse> Policy { get; set; }
        public virtual string GetPageContent(string uri)
        {
            var isCreate = Uri.TryCreate(uri, UriKind.Absolute, out var target);

            if (!isCreate)
                throw new ArgumentException("uri is invalid");

            var rc = new RestClient();
            var req = new RestRequest(target);
            req.AddHeader("Content-Type", "url/html; charset=utf-8");
            var resp = rc.ExecuteWitHeaders(req, Policy);

            return resp.Content;
        }
    }
}
