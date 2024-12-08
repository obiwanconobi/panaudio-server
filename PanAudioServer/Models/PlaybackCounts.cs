namespace PanAudioServer.Models
{
    public class PlaybackCounts
    {
        public string SongId { get; set; }
        public int PlaybackCount { get; set; }
        public int TotalSeconds { get; set; }
    }
}
