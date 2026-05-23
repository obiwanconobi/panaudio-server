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
        private TaggingService _taggingService;

        public LibraryController(SqliteHelper sqliteHelper, TaggingService taggingService)
        {
            this.sqliteHelper = sqliteHelper;
            _taggingService = taggingService;
        }

        [HttpGet("artists")]
        public async Task<List<Artists>> GetArtists()
        {
            return await sqliteHelper.GetAllArtists();
        }

        [HttpGet("artist/{artistId}")]
        public async Task<Artists> GetArtistById(string artistId)
        {
            return await sqliteHelper.GetArtistById(artistId);
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

        [HttpGet("album/{albumId}/songs")]
        public async Task<List<Songs>> GetAlbumSongs(string albumId)
        {
            return await sqliteHelper.GetSongsByAlbumId(albumId);
        }

        [HttpGet("artist/{artistId}/albums")]
        public async Task<List<Album>> GetArtistAlbums(string artistId)
        {
            return await sqliteHelper.GetAlbumsByArtistId(artistId);
        }

        [HttpPut("song/{songId}")]
        public async Task<IActionResult> EditSong(string songId, [FromBody] UpdateSongRequest request)
        {
            var song = await sqliteHelper.GetSongById(songId);
            var changedFields = new Dictionary<string, object>();

            if (request.Artist != null)
            {
                var artist = await sqliteHelper.GetArtistByName(request.Artist);
                if (artist == null)
                {
                    artist = new Artists(Guid.NewGuid().ToString(), request.Artist);
                    await sqliteHelper.UploadArtist(artist);
                }
                song.Artist = artist.Name;
                song.ArtistId = artist.Id;
                changedFields["artist"] = request.Artist;
            }

            if (request.Album != null)
            {
                var album = await sqliteHelper.GetAlbum(song.Artist, request.Album);
                if (album == null)
                {
                    album = new Album(Guid.NewGuid().ToString(), request.Album, song.Artist);
                    await sqliteHelper.UploadAlbum(album);
                }
                song.Album = album.Title;
                song.AlbumId = album.Id;
                changedFields["album"] = request.Album;
            }

            if (request.Title != null) { song.Title = request.Title; changedFields["title"] = request.Title; }
            if (request.TrackNumber.HasValue) { song.TrackNumber = request.TrackNumber; changedFields["trackNumber"] = request.TrackNumber.Value; }
            if (request.DiscNumber.HasValue) { song.DiscNumber = request.DiscNumber.Value; changedFields["discNumber"] = request.DiscNumber.Value; }
            if (request.Favourite.HasValue) { song.Favourite = request.Favourite; }

            await sqliteHelper.UpdateSong(song);

            await _taggingService.WriteTagsAsync(song.Path, changedFields);

            return Ok(song);
        }

        [HttpPut("album/{albumId}")]
        public async Task<IActionResult> EditAlbum(string albumId, [FromBody] UpdateAlbumRequest request)
        {
            var album = await sqliteHelper.GetAlbumById(albumId);
            var originalTitle = album.Title;
            var originalArtist = album.Artist;

            if (request.Title != null)
            {
                album.Title = request.Title;
            }
            if (request.Artist != null)
            {
                album.Artist = request.Artist;
            }
            if (request.Year.HasValue)
            {
                album.Year = request.Year;
            }

            await sqliteHelper.UpdateAlbum(album);

            if (request.Title != null && request.Title != originalTitle)
            {
                await sqliteHelper.UpdateAlbumTitleOnSongs(albumId, request.Title);
            }
            if (request.Artist != null && request.Artist != originalArtist)
            {
                await sqliteHelper.UpdateArtistNameOnSongsByAlbumId(albumId, request.Artist);
                var artist = await sqliteHelper.GetArtistByName(request.Artist);
                if (artist == null)
                {
                    artist = new Artists(Guid.NewGuid().ToString(), request.Artist);
                    await sqliteHelper.UploadArtist(artist);
                }
            }

            return Ok(album);
        }

        [HttpPut("artist/{artistId}")]
        public async Task<IActionResult> EditArtist(string artistId, [FromBody] UpdateArtistRequest request)
        {
            var artist = await sqliteHelper.GetArtistById(artistId);
            if (artist == null)
                return NotFound();

            var originalName = artist.Name;

            if (request.Name != null)
            {
                artist.Name = request.Name;
            }

            await sqliteHelper.UpdateArtist(artist);

            if (request.Name != null && request.Name != originalName)
            {
                await sqliteHelper.UpdateArtistNameOnSongsByArtistId(artistId, request.Name);
                await sqliteHelper.UpdateArtistNameOnAlbums(originalName, request.Name);
            }

            return Ok(artist);
        }

    }

}
