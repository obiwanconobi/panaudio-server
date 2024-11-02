namespace PanAudioServer.Models
{
    public class Album
    {
        public string Id { get; set;}
        public string Title { get; set;}
        public string Artist { get; set;}
        public int? Year { get; set;}
        public string? Picture { get; set;}
        public string? AlbumPath { get; set; }
        public DateTime DateAdded { get; set; }
        public bool Favourite { get; set; }

        public Album(string id = null, string title = null, string artist = null, int? year = null, string picture = null, string albumPath = null, DateTime dateAdded = default, bool favourite = false)
        {
            Id = id;
            Title = title;
            Artist = artist;
            Year = year;
            Picture = picture;
            AlbumPath = albumPath;
            DateAdded = dateAdded;
            Favourite = favourite;
        }
    }

}
