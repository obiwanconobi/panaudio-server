using ATL.Playlist;

namespace PanAudioServer.Models
{
    public class PlaylistItems
    {
        public required string PlaylistItemId { get; set; }
        public required string PlaylistId { get; set; }
        public required string SongId { get; set; }
        public virtual Playlists Playlist { get; set; }  
    }
}
