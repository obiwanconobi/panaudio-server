namespace PanAudioServer.Models
{
        public class Playlists
        {
            public required string PlaylistId { get; set; }
            public required string PlaylistName { get; set; }
            public virtual  ICollection<PlaylistItems> PlaylistItems { get; set; } = new List<PlaylistItems>();
        }
}
