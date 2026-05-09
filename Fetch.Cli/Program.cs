using System.Net.Http.Json;

namespace Fetch.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            PrintHelp();
            return 0;
        }

        var url = args[0];
        var loadingType = GetOption(args, "--loading-type", "-l") ?? "HttpRequest";
        var server = GetOption(args, "--server", "-s") ?? "http://localhost:5020";
        var mode = GetOption(args, "--mode", "-m") ?? "article";

        var conversionMode = mode.Equals("full-page", StringComparison.OrdinalIgnoreCase)
            ? "FullPage"
            : "Article";

        try
        {
            var apiUrl = $"{server.TrimEnd('/')}/parse/url?url={Uri.EscapeDataString(url)}&loadingType={loadingType}&mode={conversionMode}";

            using var http = new HttpClient();
            var response = await http.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"Server error ({response.StatusCode}): {error}");
                return 1;
            }

            var result = await response.Content.ReadFromJsonAsync<ParseResponse>();
            if (result?.Content == null)
            {
                Console.Error.WriteLine("Server returned empty response");
                return 1;
            }

            Console.Write(result.Content);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static string? GetOption(string[] args, string longName, string shortName)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == longName || args[i] == shortName)
                return args[i + 1];
        }
        return null;
    }

    static void PrintHelp()
    {
        Console.WriteLine("Usage: fetch <url> [options]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <url>                   URL of the page to parse");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -m, --mode <mode>       Conversion mode: article (default) or full-page");
        Console.WriteLine("  -l, --loading-type <t>  Loader type: HttpRequest (default) or Selenium");
        Console.WriteLine("  -s, --server <url>      Backend server URL (default: http://localhost:5020)");
        Console.WriteLine("  -h, --help              Show this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  fetch https://example.com");
        Console.WriteLine("  fetch https://example.com -m full-page");
        Console.WriteLine("  fetch https://example.com -l Selenium -s http://myserver:5020");
    }
}

file record ParseResponse(string Content);
