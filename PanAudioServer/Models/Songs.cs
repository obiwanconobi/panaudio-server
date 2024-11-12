namespace PanAudioServer.Models
{
    public class Songs
    {
        public string Id { get; set;}
        public int? TrackNumber { get; set;}
        public string Title { get; set;}
        public string Album { get; set;}
        public string AlbumId { get; set;}
        public string Artist { get; set;}
        public string ArtistId { get; set;}
        public string AlbumPicture { get; set;}
        public bool? Favourite { get; set;}
        public string Length { get; set;}
        public string Path { get; set; }
        public string MusicBrainzId { get; set;}

        public Songs(string id = null, int? trackNumber = null, string title = null, string album = null,
                     string albumId = null, string artist = null, string artistId = null,
                     string albumPicture = null, bool? favourite = null, string length = null, string path = null)
        {
            Id = id;
            TrackNumber = trackNumber;
            Title = title;
            Album = album;
            AlbumId = albumId;
            Artist = artist;
            ArtistId = artistId;
            AlbumPicture = albumPicture;
            Favourite = favourite;
            Length = length;
            Path = path;
        }
    }

}
