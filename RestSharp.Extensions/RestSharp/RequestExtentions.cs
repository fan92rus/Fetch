using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using RestSharp;
using Polly;

namespace Extensions.RestSharp
{
    public static class RestExtension
    {
        public static RestResponse ExecuteWithPolicy(this IRestClient client, RestRequest request, Policy<RestResponse> policy)
        {
            var val = policy.ExecuteAndCapture(() => client.Execute(request));

            var rr = val.Result;

            if (rr == null)
            {
                rr = new RestResponse
                {
                    Request = request,
                    ErrorException = val.FinalException
                };
            }

            return rr;
        }

        public static RestResponse ExecuteWitHeaders(this IRestClient client, RestRequest request, Policy<RestResponse> policy)
        {
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.AddHeader("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Accept-Encoding", "gzip");
            request.AddHeader("Content-type", "text/html; charset=utf-8");
            request.AddHeader("TE", "Trailers");
            request.AddHeader("Origin", "https://google.com/");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");

            var resp = client.ExecuteWithPolicy(request, policy);

            if (resp.StatusCode == 0)
            {
                request.AddHeader("X-Requested-With", "XMLHttpRequest");
                resp = client.ExecuteWithPolicy(request, policy);
            }

            return resp;
        }
    }
}
