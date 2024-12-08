using SixLabors.ImageSharp.PixelFormats;

namespace PanAudioServer.Models
{
    public class PlaybackArtists
    {
        public string ArtistId { get; set; }
        public string ArtistName { get; set; }
        public int PlayCount { get; set; }
        public int TotalSeconds { get; set; }
    }
}
