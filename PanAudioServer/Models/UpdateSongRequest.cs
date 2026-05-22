namespace PanAudioServer.Models
{
    public class UpdateSongRequest
    {
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public int? TrackNumber { get; set; }
        public int? DiscNumber { get; set; }
        public bool? Favourite { get; set; }
    }
}
