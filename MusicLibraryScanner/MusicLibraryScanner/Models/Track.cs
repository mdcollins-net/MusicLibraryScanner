// Models/Track.cs

namespace MusicLibraryScanner.Models {
    public class Track {
        public int ID { get; set; }
        public int AlbumId { get; set; }
        public int ArtistId { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public int Year { get; set; }
        public string Title { get; set; }
        public int TrackNumber { get; set; }
        public int Duration { get; set; }
        public DateTime? DateTagged { get; set; }
        public string? MusicBrainzTrackId { get; set; }
        public string? MusicBrainzReleaseId { get; set; }
        public string? MusicBrainzArtistId { get; set; }
    }
}