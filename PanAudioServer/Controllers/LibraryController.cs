using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class LibraryController : Controller
    {
        SqliteHelper sqliteHelper = new SqliteHelper();

        [HttpGet("artists")]
        public List<Artists> GetArtists()
        {
            return sqliteHelper.GetAllArtists();
        }

        [HttpGet("albums")]
        public List<Album> GetAlbums()
        {
            return sqliteHelper.GetAllAblums();
        }

        [HttpGet("songs")]
        public List<Songs> GetSongs() 
        {
            return sqliteHelper.GetAllSongs();
        }

        [HttpGet("song")]
        public Songs GetSong(string songId)
        {
            return sqliteHelper.GetSongById(songId);
        }


        [HttpGet]
        public List<Album> GetAlbumsForArtist(string artistName)
        {
            return sqliteHelper.GetAllAblumsForArtist(artistName);
        }

        [HttpPost("favourite")]
        public void SetFavourite(string songId, bool favourite)
        {
            var song = sqliteHelper.GetSongById(songId);
            song.Favourite = favourite;
            sqliteHelper.UpdateSong(song);
        }

    }
}
