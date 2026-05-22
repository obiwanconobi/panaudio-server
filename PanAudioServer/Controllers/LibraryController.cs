using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class LibraryController : Controller
    {
        private SqliteHelper sqliteHelper;

        public LibraryController(SqliteHelper sqliteHelper)
        {
            this.sqliteHelper = sqliteHelper;
        }

        [HttpGet("artists")]
        public List<Artists> GetArtists()
        {
            return sqliteHelper.GetAllArtists();
        }

        [HttpGet("albums-by-id")]
        public Album GetAlbumById(string albumId)
        {
            return sqliteHelper.GetAlbumById(albumId);
        }

        [HttpGet("albums")]
        public List<Album> GetAlbums()
        {
            return sqliteHelper.GetAllAblums();
        }

        [HttpGet("recent-albums")]
        public List<Album> GetRecentAlbums()
        {
            return sqliteHelper.GetRecentAblums();
        }

        [HttpGet("recent-released-albums")]
        public List<Album> GetRecentReleasedAlbums()
        {
            return sqliteHelper.GetRecentReleasedAlbums();
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


        [HttpGet("albums-for-artist")]
        public List<Album> GetAlbumsForArtist(string artistName)
        {
            return sqliteHelper.GetAllAblumsForArtist(artistName);
        }

        [HttpPost("favourite")]
        public async Task<IActionResult> SetFavourite(string songId, bool favourite)
        {
            var song = sqliteHelper.GetSongById(songId);
            song.Favourite = favourite;
            await sqliteHelper.UpdateSong(song);
            return Ok();
        }

        [HttpPost("favourite-album")]
        public async Task<IActionResult> SetAlbumFavourite(string albumId, bool favourite)
        {
            var album = sqliteHelper.GetAlbumById(albumId);
            album.Favourite = favourite;
            await sqliteHelper.UpdateAlbum(album);
            return Ok();
        }

        [HttpPost("favourite-artist")]
        public async Task<IActionResult> SetArtistFavourite(string artistId, bool favourite)
        {
            var artist = sqliteHelper.GetArtistById(artistId);
            artist.Favourite = favourite;
            await sqliteHelper.UpdateArtist(artist);
            return Ok();
        }

        [HttpGet("favourite-albums")]
        public List<Album> GetFavouriteAlbums()
        {
            return sqliteHelper.GetFavouriteAblums();
        }

        [HttpGet("favourite-artists")]
        public List<Artists> GetFavouriteArtists()
        {
            return sqliteHelper.GetFavouriteArtists();
        }

        [HttpGet("favourite-songs")]
        public List<Songs> GetFavouriteSongs()
        {
            return sqliteHelper.GetFavouriteSongs();
        }

        [HttpPost("update-song")]
        public async Task<IActionResult> UpdateSong(string songId, string songTitle, string songPath, int songNumber)
        {
            var song = sqliteHelper.GetSongById(songId);
            song.Id = songId;
            song.Title = songTitle;
            song.Path = songPath;
            song.TrackNumber = songNumber;
            await sqliteHelper.UpdateSong(song);
            return Ok();
        }
        
        [HttpPost("delete-song")]
        public async Task<IActionResult> DeleteSong(string songId)
        {
          await sqliteHelper.DeleteSong(songId);
          return Ok();
        }

        [HttpPost("delete-artist")]
        public async Task<IActionResult> DeleteArtist(string artistId)
        {
            await sqliteHelper.DeleteArtist(artistId);
            return Ok();
        }

        [HttpPost("delete-album")]
        public async Task<IActionResult> DeleteAlbum(string albumId)
        {
           await sqliteHelper.DeleteAlbum(albumId);
           return Ok();
        }


    }

}
