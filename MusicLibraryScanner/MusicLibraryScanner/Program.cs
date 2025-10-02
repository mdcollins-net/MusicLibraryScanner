// Program.cs

using log4net;
using log4net.Config;
using System.Reflection;
using MusicLibraryScanner.Data;
using MusicLibraryScanner.Repositories;
using MusicLibraryScanner.Services;

namespace MusicLibraryScanner {
    internal class Program {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static async Task Main(string[] args) {
            // Configure log4net
            var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));

            Log.Info("Application starting...");

            const string connectionString =
                "Host=localhost;Port=5432;Username=postgres;Password=password;Database=Music";
            var connFactory = new PostgresConnectionFactory(connectionString);

            var artistRepo = new ArtistRepository(connFactory);
            var albumRepo = new AlbumRepository(connFactory);
            var trackRepo = new TrackRepository(connFactory);

            var scanner = new MusicScanner(artistRepo, albumRepo, trackRepo);

            const string musicRoot = @"C:\Projects\test\test-lib-01\Music\Artists"; // Adjust path
            //const string musicRoot = @"C:\Music\Artists"; // Adjust path
            await scanner.ScanAsync(musicRoot);

            Log.Info("Application finished.");
        }
    }
}