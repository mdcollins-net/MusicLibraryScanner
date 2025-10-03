// Helpers/ArtistNfoParser.cs

using System.Xml.Linq;
using MusicLibraryScanner.Models;

namespace MusicLibraryScanner.Helpers {
    public static class ArtistNfoParser {
        public static Artist? Parse(string nfoPath) {
            if (!File.Exists(nfoPath))
                return null;

            try {
                var doc = XDocument.Load(nfoPath);
                var root = doc.Element("artist");
                if (root == null) return null;

                var artist = new Artist {
                    Name = root.Element("title")?.Value ?? string.Empty,
                    Biography = root.Element("biography")?.Value?.Trim()
                };

                // Discogs ID
                if (int.TryParse(root.Element("discogsartistid")?.Value, out var discogsId)) {
                    artist.DiscogsArtistId = discogsId;
                }

                // MusicBrainz ID
                var mbid = root.Element("musicbrainzartistid")?.Value;
                if (!string.IsNullOrWhiteSpace(mbid) && Guid.TryParse(mbid, out var guid)) {
                    artist.MusicBrainzArtistId = guid;
                }

                // AudioDB ID
                if (int.TryParse(root.Element("audiodbartistid")?.Value, out var audioDbId)) {
                    artist.AudioDbArtistId = audioDbId;
                }

                return artist;
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to parse {nfoPath}: {ex.Message}");
                return null;
            }
        }
    }
}