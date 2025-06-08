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
        public void SetFavourite(string songId, bool favourite)
        {
            var song = sqliteHelper.GetSongById(songId);
            song.Favourite = favourite;
            sqliteHelper.UpdateSong(song);
        }

        [HttpPost("favourite-album")]
        public void SetAlbumFavourite(string albumId, bool favourite)
        {
            var album = sqliteHelper.GetAlbumById(albumId);
            album.Favourite = favourite;
            sqliteHelper.UpdateAlbum(album);
        }

        [HttpPost("favourite-artist")]
        public void SetArtistFavourite(string artistId, bool favourite)
        {
            var artist = sqliteHelper.GetArtistById(artistId);
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
