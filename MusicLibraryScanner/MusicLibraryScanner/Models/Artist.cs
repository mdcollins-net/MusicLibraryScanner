// Models/Artist.cs

namespace MusicLibraryScanner.Models {
    public class Artist {
        public int ID { get; set; } // PK
        public string Name { get; init; }
        public int? DiscogsArtistId { get; set; }
        public Guid? MusicBrainzArtistId { get; set; }
        public int? AudioDbArtistId { get; set; }
        public string? Biography { get; init; }
    }
}