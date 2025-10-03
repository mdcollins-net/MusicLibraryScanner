// Config/Config.cs

namespace MusicLibraryScanner.Config {

    public class AppConfig {
        public ConnectionStringsConfig ConnectionStrings { get; set; } = new();
        public DiscogsConfig Discogs { get; set; } = new();
        public LastFmConfig LastFm { get; set; } = new();
        public SettingsConfig Settings { get; set; } = new();
    }

    public class ConnectionStringsConfig {
        public string Postgres { get; set; } = string.Empty;
    }

    public class DiscogsConfig {
        public string BaseUrl { get; set; } = "https://api.discogs.com";
        public string PersonalAccessToken { get; set; } = string.Empty;
        public string UserAgent { get; set; } = "MusicLibraryScanner/1.0";
    }

    public class LastFmConfig {
        public string BaseUrl { get; set; } = "http://ws.audioscrobbler.com/2.0/";
        public string ApiKey { get; set; } = string.Empty;
    }

    public class SettingsConfig {
        public string DefaultRootPath { get; set; } = @"C:\Projects\test\test-lib-01\Music\Artists";
        public int MaxConcurrentAlbums { get; set; } = 4;
        public int MaxConcurrentTracks { get; set; } = 8;
    }
}

