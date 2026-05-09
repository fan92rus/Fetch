using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Extensions.RestSharp;
using Polly;
using Polly.Retry;
using RestSharp;
using TlsClientWrapperSharp.Handlers;
using TlsClientWrapperSharp.Helpers;
using TlsClientWrapperSharp.Models;

namespace Funny.WebScrape.Loaders
{
    public class RequestWebLoader : IWebLoader
    {
        private static readonly string[] CfIndicators =
        [
            "Enable JavaScript and cookies to continue",
            "Checking your browser",
            "Just a moment",
            "cf-browser-verification",
            "Attention Required"
        ];

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private static readonly ConcurrentDictionary<string, CachedDomain> CookieCache = new();
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(25);
        private static bool _tlsLibraryLoaded;

        public RequestWebLoader()
        {
            Policy = Polly.Policy
                .HandleResult<RestResponse>(
                    (response) => (response.StatusCode == 0 || response.StatusCode == HttpStatusCode.TooManyRequests)
                                  && response.ResponseStatus != ResponseStatus.TimedOut).WaitAndRetry(
                    2,
                    retryAttempt => TimeSpan.FromSeconds(2));

            FlareSolverrUrl = Environment.GetEnvironmentVariable("FLARESOLVERR_URL") ?? "http://localhost:8191";
        }

        private RetryPolicy<RestResponse> Policy { get; set; }
        private string FlareSolverrUrl { get; }

        public virtual async Task<string> GetPageContentAsync(string uri)
        {
            var isCreate = Uri.TryCreate(uri, UriKind.Absolute, out var target);

            if (!isCreate)
                throw new ArgumentException("uri is invalid");

            var host = target.Host;

            // Try with cached cookies via TLS client
            if (CookieCache.TryGetValue(host, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
            {
                var html = await FetchWithTlsClient(uri, cached.Cookies, cached.UserAgent);
                if (html != null && !IsCloudflareContent(html))
                {
                    Console.WriteLine($"[Fetch] Using cached cookies for {host}");
                    return html;
                }

                Console.WriteLine($"[Fetch] Cached cookies rejected for {host}, re-solving");
                CookieCache.TryRemove(host, out _);
            }

            // Regular request via RestSharp
            var rc = new RestClient();
            var req = new RestRequest(target);
            var resp = rc.ExecuteWitHeaders(req, Policy);

            if (!IsCloudflareChallenge(resp))
                return resp.Content;

            Console.WriteLine($"[Fetch] Cloudflare detected, solving via FlareSolverr: {uri}");

            var (htmlFs, cookies, userAgent) = await SolveViaFlareSolverr(uri);

            if (cookies != null)
            {
                CookieCache[host] = new CachedDomain(cookies, userAgent, DateTime.UtcNow + CacheTtl);
                Console.WriteLine($"[Fetch] Cached cookies for {host} (TTL {CacheTtl.TotalMinutes}min)");
            }

            return htmlFs;
        }

        private static async Task<string> FetchWithTlsClient(string uri, string cookies, string userAgent)
        {
            try
            {
                if (!_tlsLibraryLoaded)
                {
                    await TlsLibraryLoader.EnsureLibraryExistsAsync();
                    _tlsLibraryLoaded = true;
                }

                var handler = new TlsClientHandler
                {
                    TlsClientIdentifier = ClientIdentifier.Chrome133
                };

                using var http = new HttpClient(handler);
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.TryAddWithoutValidation("Cookie", cookies);
                request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
                request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");

                var response = await http.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fetch] TLS client error: {ex.Message}");
                return null;
            }
        }

        private async Task<(string Html, string Cookies, string UserAgent)> SolveViaFlareSolverr(string uri)
        {
            try
            {
                var payload = new
                {
                    cmd = "request.get",
                    url = uri,
                    maxTimeout = 60000
                };

                using var http = new HttpClient();
                var json = JsonSerializer.Serialize(payload, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{FlareSolverrUrl}/v1", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Fetch] FlareSolverr error: {response.StatusCode}");
                    return (null, null, null);
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FlareSolverrResponse>(responseBody, JsonOptions);

                if (result?.Status != "ok" || result.Solution?.Response == null)
                {
                    Console.WriteLine($"[Fetch] FlareSolverr failed: status={result?.Status}");
                    return (null, null, null);
                }

                var cookieHeader = result.Solution.Cookies != null
                    ? string.Join("; ", result.Solution.Cookies.Select(c => $"{c.Name}={c.Value}"))
                    : null;

                Console.WriteLine($"[Fetch] FlareSolverr solved, got {result.Solution.Response.Length} chars");
                return (result.Solution.Response, cookieHeader, result.Solution.UserAgent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fetch] FlareSolverr unavailable: {ex.Message}");
                return (null, null, null);
            }
        }

        private static bool IsCloudflareChallenge(RestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return true;

            if (response.StatusCode == HttpStatusCode.Forbidden && response.Content != null)
                return true;

            if (response.Content == null)
                return false;

            return IsCloudflareContent(response.Content);
        }

        private static bool IsCloudflareContent(string content)
        {
            if (content == null) return false;

            foreach (var indicator in CfIndicators)
            {
                if (content.Contains(indicator, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private record CachedDomain(string Cookies, string UserAgent, DateTime ExpiresAt);
    }

    file record FlareSolverrResponse(string Status, FlareSolverrSolution Solution);
    file record FlareSolverrSolution(string Url, int Status, List<FlareSolverrCookie> Cookies, string UserAgent, string Response);
    file record FlareSolverrCookie(string Name, string Value, string Domain, string Path);
}
