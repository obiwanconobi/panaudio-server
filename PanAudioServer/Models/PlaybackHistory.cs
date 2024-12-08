namespace PanAudioServer.Models
{
    public class PlaybackHistory
    {
        public int PlaybackId { get; set; }
        public string SongId { get; set; }
        public String? UserId { get; set; }
        public DateTime PlaybackStart { get; set; }
        public int Seconds { get; set; }
        public DateTime? PlaybackEnd { get; set; }

    }
}
