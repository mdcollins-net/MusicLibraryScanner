namespace MusicLibraryScanner.Models
{
    public class Track
    {
        public int ID { get; set; }        // PK
        public string Album { get; set; }
        public int AlbumID { get; set; }   // FK -> Albums.ID
        public string Artist { get; set; }
        public int ArtistID { get; set; }  // FK -> Artists.ID
        public int? Year { get; set; }
        public string Title { get; set; }
        public int? TrackNumber { get; set; }
        public int? Length { get; set; }
    }
}