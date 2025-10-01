namespace MusicLibraryScanner.Models
{
    public class Album
    {
        public int ID { get; set; }     // PK
        public int ArtistID { get; set; }  // FK -> Artists.ID
        public int? Year { get; set; }
        public string Title { get; set; }
    }
}