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
        public async Task<List<Artists>> GetArtists()
        {
            return await sqliteHelper.GetAllArtists();
        }

        [HttpGet("albums-by-id")]
        public async Task<Album> GetAlbumById(string albumId)
        {
            return await sqliteHelper.GetAlbumById(albumId);
        }

        [HttpGet("albums")]
        public async Task<List<Album>> GetAlbums()
        {
            return await sqliteHelper.GetAllAblums();
        }

        [HttpGet("recent-albums")]
        public async Task<List<Album>> GetRecentAlbums()
        {
            return await sqliteHelper.GetRecentAblums();
        }

        [HttpGet("recent-released-albums")]
        public async Task<List<Album>> GetRecentReleasedAlbums()
        {
            return await sqliteHelper.GetRecentReleasedAlbums();
        }
        
        [HttpGet("songs")]
        public async Task<List<Songs>> GetSongs() 
        {
            return await sqliteHelper.GetAllSongs();
        }

        [HttpGet("song")]
        public async Task<Songs> GetSong(string songId)
        {
            return await sqliteHelper.GetSongById(songId);
        }


        [HttpGet("albums-for-artist")]
        public async Task<List<Album>> GetAlbumsForArtist(string artistName)
        {
            return await sqliteHelper.GetAllAblumsForArtist(artistName);
        }

        [HttpPost("favourite")]
        public async Task<IActionResult> SetFavourite(string songId, bool favourite)
        {
            var song = await sqliteHelper.GetSongById(songId);
            song.Favourite = favourite;
            await sqliteHelper.UpdateSong(song);
            return Ok();
        }

        [HttpPost("favourite-album")]
        public async Task<IActionResult> SetAlbumFavourite(string albumId, bool favourite)
        {
            var album = await sqliteHelper.GetAlbumById(albumId);
            album.Favourite = favourite;
            await sqliteHelper.UpdateAlbum(album);
            return Ok();
        }

        [HttpPost("favourite-artist")]
        public async Task<IActionResult> SetArtistFavourite(string artistId, bool favourite)
        {
            var artist = await sqliteHelper.GetArtistById(artistId);
            artist.Favourite = favourite;
            await sqliteHelper.UpdateArtist(artist);
            return Ok();
        }

        [HttpGet("favourite-albums")]
        public async Task<List<Album>> GetFavouriteAlbums()
        {
            return await sqliteHelper.GetFavouriteAblums();
        }

        [HttpGet("favourite-artists")]
        public async Task<List<Artists>> GetFavouriteArtists()
        {
            return await sqliteHelper.GetFavouriteArtists();
        }

        [HttpGet("favourite-songs")]
        public async Task<List<Songs>> GetFavouriteSongs()
        {
            return await sqliteHelper.GetFavouriteSongs();
        }

        [HttpPost("update-song")]
        public async Task<IActionResult> UpdateSong(string songId, string songTitle, string songPath, int songNumber)
        {
            var song = await sqliteHelper.GetSongById(songId);
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
