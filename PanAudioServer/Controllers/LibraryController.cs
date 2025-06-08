using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Data;
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
        public async Task<List<Songs>> GetSongs() 
        {
            return await sqliteHelper.GetAllSongs();
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
        public void SetFavourite(string songId, bool favourite)
        {
            var song = sqliteHelper.GetSongById(songId);
            song.Favourite = favourite;
            sqliteHelper.UpdateSong(song);
        }

        [HttpPost("favourite-album")]
        public async Task SetAlbumFavourite(string albumId, bool favourite)
        {
            var album = await sqliteHelper.GetAlbumById(albumId);
            album.Favourite = favourite;
            sqliteHelper.UpdateAlbum(album);
        }

        [HttpPost("favourite-artist")]
        public async Task SetArtistFavourite(string artistId, bool favourite)
        {
            var artist =await sqliteHelper.GetArtistById(artistId);
            artist.Favourite = favourite;
            sqliteHelper.UpdateArtist(artist);
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
    }

}
