using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class PlaylistController : Controller
    {
        private SqliteHelper sqliteHelper;

        public PlaylistController(SqliteHelper sqliteHelper)
        {
            this.sqliteHelper = sqliteHelper;
        }
        [HttpGet("playlists")]
        public async Task<List<Playlists>> GetPlaylists()
        {
            return await sqliteHelper.GetPlaylists();
        }

        [HttpGet("playlist")]
        public async Task<Playlists> GetPlaylist(string playlistId)
        {
            return await sqliteHelper.GetPlaylist(playlistId);
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
