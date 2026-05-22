namespace PanAudioServer.Models
{
    public class UpdateAlbumRequest
    {
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public int? Year { get; set; }
    }
}
