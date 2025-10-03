using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using MusicLibraryScanner.Data;
using MusicLibraryScanner.Helpers;
using MusicLibraryScanner.Repositories;
using MusicLibraryScanner.Services;

class Program {
    static async Task Main(string[] args) {
        // Load configuration (JSON + environment variables)
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Check for help flag
        if (args.Any(a => a.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                          a.Equals("-h", StringComparison.OrdinalIgnoreCase))) {
            PrintUsage();
            return;
        }

        // Configure log4net
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        // Default root path (can override via args)
        var rootPath = args.Length > 0 && !args[0].StartsWith("-")
            ? args[0]
            : "C:\\Music";

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
        Console.WriteLine("MusicLibraryScanner - Usage");
        Console.WriteLine("============================");
        Console.WriteLine("dotnet run -- [rootPath] [options]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  rootPath        Path to the music library folder (default: C:\\Music)");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --log-only, -q  Suppress console output, write only to logs (quiet mode)");
        Console.WriteLine("  --help, -h      Show this usage information");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run -- \"C:\\Music\"");
        Console.WriteLine("  dotnet run -- \"D:\\FLAC\" --log-only");
        Console.WriteLine("  dotnet run -- \"D:\\FLAC\" -q");
        Console.WriteLine();
    }
}
