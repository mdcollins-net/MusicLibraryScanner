// Helpers/AlbumNfoParser.cs

using System.Xml.Linq;
using MusicLibraryScanner.Models;

namespace MusicLibraryScanner.Helpers {
    public static class AlbumNfoParser {
        public static Album? Parse(string nfoPath) {
            if (!File.Exists(nfoPath))
                return null;

            try {
                var doc = XDocument.Load(nfoPath);
                var root = doc.Element("album");
                if (root == null) return null;

                var album = new Album {
                    Title = root.Element("title")?.Value ?? string.Empty,
                };

                // Discogs ID
                if (int.TryParse(root.Element("discogsreleaseid")?.Value, out var discogsId)) {
                    album.DiscogsArtistId = discogsId;
                }
                return album;
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to parse {nfoPath}: {ex.Message}");
                return null;
            }
        }
    }
}