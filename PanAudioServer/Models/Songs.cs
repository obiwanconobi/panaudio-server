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
        public string Codec { get; set; }
        public string BitRate { get; set;}
        public string BitDepth { get; set; }
        public string SampleRate { get; set; }
        public string Path { get; set; }
        public string? MusicBrainzId { get; set;}
        public int PlayCount { get; set;}

       
    }

}
