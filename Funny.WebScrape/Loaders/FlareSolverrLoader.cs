using System.Text.Json;

namespace Funny.WebScrape.Loaders
{
    public class FlareSolverrLoader : IWebLoader
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly string _flareSolverrUrl;

        public FlareSolverrLoader()
        {
            _flareSolverrUrl = Environment.GetEnvironmentVariable("FLARESOLVERR_URL") ?? "http://localhost:8191";
        }

        public virtual async Task<string> GetPageContentAsync(string uri)
        {
            var payload = new
            {
                cmd = "request.get",
                url = uri,
                maxTimeout = 60000
            };

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await http.PostAsync($"{_flareSolverrUrl}/v1", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"FlareSolverr error: {response.StatusCode}");

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FlareSolverrResponse>(responseBody, JsonOptions);

            if (result?.Status != "ok" || result.Solution?.Response == null)
                throw new Exception($"FlareSolverr failed: status={result?.Status}");

            return result.Solution.Response;
        }
    }

    file record FlareSolverrResponse(string Status, FlareSolverrSolution Solution);
    file record FlareSolverrSolution(string Url, int Status, string Response);
}
