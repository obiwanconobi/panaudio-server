using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class PlaylistController : Controller
    {
        SqliteHelper sqliteHelper = new SqliteHelper();
        [HttpGet("playlists")]
        public List<Playlists> GetPlaylists()
        {
            return sqliteHelper.GetPlaylists();
        }

        [HttpGet("playlist")]
        public Playlists GetPlaylist(string playlistId)
        {
            return sqliteHelper.GetPlaylist(playlistId);
        }

        [HttpPut("playlist")]
        public async Task CreateNewPlaylist(string playlistName)
        {
            await sqliteHelper.CreateNewPlaylist(playlistName);
        }

        [HttpPut("addSong")]
        public async Task AddSongToPlaylist(string playlistId, string songId)
        {
            await sqliteHelper.AddSongToPlaylist(playlistId, songId);
        }

        [HttpPut("deleteSong")]
        public async Task DeleteSongFromPlaylist(string playlistId, string songId)
        {
            await sqliteHelper.DeleteSongFromPlaylist(playlistId, songId);
        }

        [HttpDelete("playlist")]
        public async Task DeletePlaylist(string playlistId)
        {
            await sqliteHelper.DeletePlaylist(playlistId);
        }
    }
}
