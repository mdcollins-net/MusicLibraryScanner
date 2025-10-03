using System.Reflection;
using System.Text;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;

using MusicLibraryScanner.Data;
using MusicLibraryScanner.Helpers;
using MusicLibraryScanner.Repositories;
using MusicLibraryScanner.Services;

class Program {
    private static async Task Main(string[] args) {
        // Check for help flag first
        if (args.Any(a => a.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                          a.Equals("-h", StringComparison.OrdinalIgnoreCase))) {
            PrintUsage();
            return;
        }

        // ✅ Check for version flag
        if (args.Any(a => a.Equals("--version", StringComparison.OrdinalIgnoreCase) ||
                          a.Equals("-v", StringComparison.OrdinalIgnoreCase))) {
            PrintVersion();
            return;
        }

        // Register legacy encodings (fixes log4net error with codepage 437)
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Configure log4net
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        // Load configuration (supports appsettings.Development.json + env vars)
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Default root path (can override via args)
        var rootPath = args.Length > 0 && !args[0].StartsWith("-")
            ? args[0]
            : @"C:\Projects\test\test-lib-01\Music\Artists";

        // Check for flags
        var logOnly = args.Any(a =>
            a.Equals("--log-only", StringComparison.OrdinalIgnoreCase) ||
            a.Equals("-q", StringComparison.OrdinalIgnoreCase));

        // Warn if root path is missing
        if (!Directory.Exists(rootPath)) {
            var message = $"Root path '{rootPath}' does not exist.";
            if (!logOnly) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[WARN] " + message);
                Console.ResetColor();
            }
            log4net.LogManager.GetLogger(typeof(Program)).Warn(message);
            return;
        }

        // ✅ Get connection string from config
        var connString = config.GetConnectionString("Postgres");
        if (string.IsNullOrEmpty(connString)) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] No Postgres connection string found in config.");
            Console.ResetColor();
            return;
        }

        var connFactory = new PostgresConnectionFactory(connString);

        // Pass it to repositories
        IArtistRepository artistRepo = new ArtistRepository(connFactory);
        IAlbumRepository albumRepo = new AlbumRepository(connFactory);
        ITrackRepository trackRepo = new TrackRepository(connFactory);

        var scanner = new MusicScanner(artistRepo, albumRepo, trackRepo);

        var stats = new ProcessingStats {
            LogOnly = logOnly
        };

        await scanner.ScanAsync(rootPath);
    }

    private static void PrintUsage() {
        Console.WriteLine();

        // Title
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("MusicLibraryScanner - Usage");
        Console.WriteLine("============================");
        Console.ResetColor();

        Console.WriteLine("dotnet run -- [rootPath] [options]");
        Console.WriteLine();

        // Arguments
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Arguments:");
        Console.ResetColor();
        Console.WriteLine("  rootPath        Path to the music library folder (default: C:\\Music)");
        Console.WriteLine();

        // Options
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Options:");
        Console.ResetColor();
        Console.WriteLine("  --log-only, -q   Suppress console output, write only to logs (quiet mode)");
        Console.WriteLine("  --help, -h       Show this usage information");
        Console.WriteLine("  --version, -v    Show program version");
        Console.WriteLine();

        // Configuration
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Configuration:");
        Console.ResetColor();
        Console.WriteLine("  The application reads settings from appsettings.json by default.");
        Console.WriteLine("  You can override with environment-specific files (e.g. appsettings.Development.json).");
        Console.WriteLine("  Set DOTNET_ENVIRONMENT=Development to load appsettings.Development.json.");
        Console.WriteLine("  Environment variables can also override settings, e.g.:");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("    ConnectionStrings__Postgres=\"Host=db;Port=5432;Database=music;Username=postgres;Password=secret\"");
        Console.ResetColor();
        Console.WriteLine();

        // Examples
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Examples:");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  dotnet run -- \"C:\\Music\"");
        Console.WriteLine("  dotnet run -- \"D:\\FLAC\" --log-only");
        Console.WriteLine("  dotnet run -- \"D:\\FLAC\" -q");
        Console.WriteLine("  DOTNET_ENVIRONMENT=Development dotnet run -- \"C:\\Music\"");
        Console.ResetColor();

        Console.WriteLine();
    }

    private static void PrintVersion() {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var version = informationalVersion ?? assembly.GetName().Version?.ToString() ?? "unknown";

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"MusicLibraryScanner version {version}");
        Console.ResetColor();
    }
}
