// Helpers/AlbumNfoParser.cs

using System.Xml.Linq;
using MusicLibraryScanner.Models;

namespace MusicLibraryScanner.Helpers {
    public static class AlbumNfoParser {
        /// <summary>
        /// Parse an album.nfo file into an Album object.
        /// Returns null if the file doesn't exist or parsing fails.
        /// </summary>
        public static Album? Parse(string nfoPath, int artistId) {
            if (!File.Exists(nfoPath))
                return null;

            try {
                var doc = XDocument.Load(nfoPath);
                var albumElement = doc.Element("album");
                if (albumElement == null) return null;

                var title = albumElement.Element("title")?.Value?.Trim();
                var yearText = albumElement.Element("year")?.Value?.Trim();
                var discogsIdText = albumElement.Element("discogsreleaseid")?.Value?.Trim();

                int? year = int.TryParse(yearText, out var parsedYear) ? parsedYear : null;
                int? discogsReleaseId = int.TryParse(discogsIdText, out var parsedDiscogs) ? parsedDiscogs : null;

                return new Album {
                    ArtistID = artistId,
                    Title = title ?? "Unknown Album",
                    Year = year,
                    DiscogsReleaseId = discogsReleaseId
                };
            }
            catch (Exception ex) {
                // TODO: inject logging if you want detailed info
                Console.WriteLine($"[WARN] Failed to parse album NFO '{nfoPath}': {ex.Message}");
                return null;
            }
        }
    }
}