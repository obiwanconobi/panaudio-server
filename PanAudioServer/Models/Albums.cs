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

        public Album(string id = null, string title = null, string artist = null, int? year = null, string picture = null, string albumPath = null)
        {
            Id = id;
            Title = title;
            Artist = artist;
            Year = year;
            Picture = picture;
            AlbumPath = albumPath;
        }
    }

}
