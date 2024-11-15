using Microsoft.EntityFrameworkCore;
using PanAudioServer.Data;
using PanAudioServer.Models;
using System.Diagnostics.Contracts;

namespace PanAudioServer.Helper
{
    public class SqliteHelper
    {
        private SqliteContext? _context;

        public async Task<Album> GetAlbum(string artist, string album)
        {
            _context = new SqliteContext();
            return _context.Album.FirstOrDefault(x => x.Title == album && x.Artist == artist);
        }

        public Album GetAlbumById(string albumId)
        {
            _context = new SqliteContext();
            return _context.Album.First(x => x.Id == albumId);
        }

        public async void Clear()
        {
            _context = new SqliteContext();
            _context.Album.ExecuteDelete();
            _context.Artists.ExecuteDelete();
            _context.Songs.ExecuteDelete();
        }


        public async Task UploadAlbums(List<Album> album)
        {
            _context = new SqliteContext();
            using (var context = new SqliteContext())
            {
                try
                {
                    _context.Album.AddRange(album);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public async void UploadAlbum(Album album)
        {
            _context = new SqliteContext();
            using(var context  = new SqliteContext())
            {
                try
                {
                    _context.Album.Add(album);
                    await _context.SaveChangesAsync();
                }catch(Exception ex)
                {

                }
            }
        }

        public List<Album> GetAllAblumsForArtist(string artistName)
        {
            _context = new SqliteContext();
            return _context.Album.Where(x => x.Artist == artistName).ToList();
        }

        public List<Album> GetAllAblums()
        {
            _context = new SqliteContext();
            return _context.Album.OrderBy(x => x.Title).ToList();
        }

        public List<Album> GetFavouriteAblums()
        {
            _context = new SqliteContext();
            return _context.Album.Where(x => x.Favourite == true).ToList();
        }

        public List<Album> GetRecentAblums()
        {
            _context = new SqliteContext();
            return _context.Album.OrderBy(x => x.DateAdded).Take(20).ToList();
        }

        public List<Album> GetRecentReleasedAlbums()
        {
            _context = new SqliteContext();
            return _context.Album.Where(x => x.Year != 0).OrderByDescending(x => x.Year).Take(40).ToList();
        }

        public List<Songs> GetAllSongs()
        {
            _context = new SqliteContext();
            return _context.Songs.OrderBy(x => x.Title).ToList();
        }

        public List<Songs> GetFavouriteSongs()
        {
            _context = new SqliteContext();
            return _context.Songs.Where(x => x.Favourite == true).ToList();
        }

        public List<Artists> GetAllArtists()
        {
            _context =  new SqliteContext();
            return _context.Artists.OrderBy(x => x.Name).ToList();
        }


        public Artists GetArtist(string artist)
        {
            _context = new SqliteContext();
            return _context.Artists.First(x => x.Name == artist);
        }

        public Artists GetArtistById(string artistId)
        {
            _context = new SqliteContext();
            return _context.Artists.First(x => x.Name == artistId);
        }


        public List<Artists> GetFavouriteArtists()
        {
            _context = new SqliteContext();
            return _context.Artists.Where(x => x.Favourite == true).ToList();
        }

        public Songs GetSongById(string songId)
        {
            _context = new SqliteContext();
            var song = _context.Songs.First(x => x.Id == songId);
            song.PlayCount = _context.PlaybackHistory.Count(x => x.SongId == songId);
            return song;
        }

        public Songs GetSong(string artist, string album, string title)
        {
            _context = new SqliteContext();
            return _context.Songs.First(x => x.Artist == artist && x.Album == album  && x.Title == title);
        }

        public async Task UploadArtists(List<Artists> artists)
        {
            _context = new SqliteContext();
            using (var context = new SqliteContext())
            {
                try
                {
                    context.Artists.AddRange(artists);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        public async void UploadArtist(Artists artists)
        {
            _context = new SqliteContext();
            using(var context = new SqliteContext())
            {
                try
                {
                    context.Artists.Add(artists);
                    await context.SaveChangesAsync();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public void UpdateArtist(Artists artist)
        {
            _context ??= new SqliteContext();
            using (var context = new SqliteContext())
            {

                try
                {
                    context.Artists.Update(artist);
                    context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                }

            }

        }

        public void UpdateAlbum(Album album)
        {
            _context ??= new SqliteContext();
            using (var context = new SqliteContext())
            {

                try
                {
                    context.Album.Update(album);
                    context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                }

            }

        }

        public void UpdateSong(Songs song)
        {
            _context ??= new SqliteContext();
            using (var context = new SqliteContext())
            {

                try
                {
                    context.Songs.Update(song);
                    context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                }

            }

        }


        public async Task UploadSongs(List<Songs> song)
        {
            _context ??= new SqliteContext();
            using (var context = new SqliteContext())
            {

                try
                {
                    context.Songs.AddRange(song);
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                }

            }

        }

        public async void UploadSong(Songs song)
        {
            _context ??= new SqliteContext();
            using (var context = new SqliteContext())
            {

                try
                {
                    context.Songs.Add(song);
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                   
                    Console.WriteLine(ex.ToString());
                }

            }

        }

        public async Task CreateNewPlaylist(string playlistTitle)
        {
            _context ??= new SqliteContext();
            using (var context = new SqliteContext())
            {

                try
                {
                    context.Playlists.Add(new Playlists() { PlaylistId = Guid.NewGuid().ToString(), PlaylistName = playlistTitle });
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                }

            }
        }

        public Playlists GetPlaylist(string playlistId)
        {
            _context ??= new SqliteContext();
            return _context.Playlists.Where(x => x.PlaylistId == playlistId).Include(x => x.PlaylistItems).ThenInclude(p => p.Song).FirstOrDefault();
        }

        public List<Playlists> GetPlaylists()
        {
            _context ??= new SqliteContext();
            return _context.Playlists.ToList();
        }
        public async Task DeletePlaylist(string playlistId)
        {
            _context ??= new SqliteContext();
            try
            {
                _context.Playlists.Where(x => x.PlaylistId == playlistId).ExecuteDelete();
                await _context.SaveChangesAsync();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task AddSongToPlaylist(string playlistId, string songId)
        {
            _context ??= new SqliteContext();
            try
            {
                _context.PlaylistItems.Add(new PlaylistItems() { PlaylistId = playlistId, PlaylistItemId = Guid.NewGuid().ToString(), SongId = songId });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task DeleteSongFromPlaylist(string playlistId, string songId)
        {
            _context ??= new SqliteContext();
            try
            {
                _context.PlaylistItems.Where(x => x.PlaylistId == playlistId).Where(y => y.SongId == songId).ExecuteDelete();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        //Playback
        public async Task StartRecordPlayback(string songId)
        {
            _context ??= new SqliteContext();
            DateTime playbackStartTime = DateTime.Now;
            try
            {
                await _context.PlaybackHistory.AddAsync(new PlaybackHistory() { SongId = songId, PlaybackStart = playbackStartTime });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<List<PlaybackCounts>> GetPlaybackHistory()
        {
            _context ??= new SqliteContext();
            var songPlaybackCounts = _context.PlaybackHistory
                 .GroupBy(x => x.SongId)
                 .Select(g => new PlaybackCounts{ SongId = g.Key, PlaybackCount = g.Count() })
                 .OrderByDescending(x => x.PlaybackCount)
                 .ToList();
            return songPlaybackCounts;
        }
    }
}
