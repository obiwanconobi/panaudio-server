using System.Text.Json;
using System.Text.RegularExpressions;
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

        [HttpPost("upload-playlist")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> UploadPlaylist(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            if (!file.FileName.EndsWith(".jspf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("File must be a .jspf file");

            var playlistName = ExtractDateFromFilename(file.FileName) ?? DateTime.Now.ToString("yyyy-MM-dd");

            string json;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                json = await reader.ReadToEndAsync();
            }

            using var doc = JsonDocument.Parse(json);
            var playlistElement = doc.RootElement.GetProperty("playlist");
            var tracks = playlistElement.GetProperty("track").EnumerateArray();

            var matchedSongIds = new List<string>();
            foreach (var track in tracks)
            {
                var title = track.GetProperty("title").GetString() ?? "";
                var creator = track.GetProperty("creator").GetString() ?? "";
                var mbid = "";
                if (track.TryGetProperty("identifier", out var identifiers) && identifiers.GetArrayLength() > 0)
                {
                    var firstId = identifiers[0].GetString() ?? "";
                    var match = Regex.Match(firstId, @"recording/([a-f0-9-]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                        mbid = match.Groups[1].Value;
                }

                string? songId = null;
                bool matchedByMbid = false;
                if (!string.IsNullOrEmpty(mbid))
                {
                    songId = await sqliteHelper.GetSongIdByMusicBrainzId(mbid);
                    matchedByMbid = songId != null;
                }

                if (songId == null)
                    songId = await sqliteHelper.GetSongIdByTitleAndArtist(title, creator);

                if (songId != null && !matchedByMbid && !string.IsNullOrEmpty(mbid))
                    await sqliteHelper.SetSongMusicBrainzId(songId, mbid);

                if (songId != null && !matchedSongIds.Contains(songId))
                    matchedSongIds.Add(songId);
            }

            if (matchedSongIds.Count == 0)
                return Ok(new { playlistId = "", matchedCount = 0, totalCount = 0 });

            var playlist = await sqliteHelper.CreatePlaylistFromJspf(playlistName, matchedSongIds);
            return Ok(playlist);
        }

        private static string? ExtractDateFromFilename(string filename)
        {
            var match = Regex.Match(Path.GetFileNameWithoutExtension(filename), @"(\d{4}-\d{2}-\d{2})");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
