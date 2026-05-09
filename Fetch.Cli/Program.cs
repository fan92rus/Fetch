using System.Net.Http.Json;
using System.Text.Json;

namespace Fetch.Cli;

class Program
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "fetch.cli");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            PrintHelp();
            return 0;
        }

        if (args[0] == "config")
            return HandleConfig(args[1..]);

        var url = args[0];
        var loadingType = GetOption(args, "--loading-type", "-l") ?? "HttpRequest";
        var mode = GetOption(args, "--mode", "-m") ?? "article";

        var config = LoadConfig();
        var server = GetOption(args, "--server", "-s") ?? config.Server ?? "http://localhost:5020";

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

    static int HandleConfig(string[] args)
    {
        if (args.Length == 0)
        {
            var config = LoadConfig();
            Console.WriteLine($"server = {config.Server ?? "(not set)"}");
            Console.WriteLine($"Config file: {ConfigPath}");
            return 0;
        }

        if (args.Length == 2 && args[0] == "set" && args[1].StartsWith("server="))
        {
            var value = args[1]["server=".Length..];
            var config = LoadConfig();
            config.Server = value;
            SaveConfig(config);
            Console.WriteLine($"server = {value}");
            return 0;
        }

        if (args.Length == 2 && args[0] == "set" && args[1].StartsWith("server "))
        {
            var value = args[1]["server ".Length..];
            var config = LoadConfig();
            config.Server = value;
            SaveConfig(config);
            Console.WriteLine($"server = {value}");
            return 0;
        }

        Console.Error.WriteLine("Usage: fetch config [set server=<url>]");
        return 1;
    }

    static FetchConfig LoadConfig()
    {
        if (!File.Exists(ConfigPath)) return new FetchConfig();

        try
        {
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<FetchConfig>(json) ?? new FetchConfig();
        }
        catch
        {
            return new FetchConfig();
        }
    }

    static void SaveConfig(FetchConfig config)
    {
        Directory.CreateDirectory(ConfigDir);
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
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
        Console.WriteLine("       fetch config [set server=<url>]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  config                  Show current configuration");
        Console.WriteLine("  config set server=<url> Set default Fetch.Server URL");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <url>                   URL of the page to parse");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -m, --mode <mode>       Conversion mode: article (default) or full-page");
        Console.WriteLine("  -l, --loading-type <t>  Loader type: HttpRequest (default) or Selenium");
        Console.WriteLine("  -s, --server <url>      Override server URL (default: from config or http://localhost:5020)");
        Console.WriteLine("  -h, --help              Show this help");
        Console.WriteLine();
        Console.WriteLine("Config file: %APPDATA%\\fetch.cli\\config.json");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  fetch https://example.com");
        Console.WriteLine("  fetch https://example.com -m full-page");
        Console.WriteLine("  fetch config set server=http://myserver:5020");
    }
}

file record ParseResponse(string Content);

class FetchConfig
{
    public string? Server { get; set; }
}
