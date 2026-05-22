using Microsoft.EntityFrameworkCore;
using PanAudioServer.Data;
using PanAudioServer.Models;

namespace PanAudioServer.Helper
{
    public class SqliteHelper
    {
        private SqliteContext? _context;

        public SqliteHelper()
        {
            _context = new SqliteContext();
        }

        public SqliteHelper(SqliteContext context)
        {
            _context = context;
        }


        public async Task DeleteArtist(string artistId){
            _context.Artists.Where(x => x.Id == artistId).ExecuteDelete();
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAlbum(string albumId){
            _context.Album.Where(x => x.Id == albumId).ExecuteDelete();
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSong(string songId){
            _context.Songs.Where(x => x.Id == songId).ExecuteDelete();
            await _context.SaveChangesAsync();
        }


        public async Task<Album> GetAlbum(string artist, string album)
        {
            var ff = "Get Album for " + artist;
            SentrySdk.CaptureEvent(new SentryEvent(new Exception(ff)));
            return _context.Album.FirstOrDefault(x => x.Title == album && x.Artist == artist);
        }

        public async Task<Album> GetAlbumById(string albumId)
        {
            
            return await _context.Album.FirstAsync(x => x.Id == albumId);
        }

        public async Task Clear()
        {
            
            _context.PlaybackHistory.ExecuteDelete();
            _context.Playlists.ExecuteDelete();
            _context.PlaylistItems.ExecuteDelete();
            _context.Album.ExecuteDelete();
            _context.Artists.ExecuteDelete();
            _context.Songs.ExecuteDelete();
        }


        public async Task UploadAlbums(List<Album> album)
        {
            
          
                try
                {
                    _context.Album.AddRange(album);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            
        }

        public async Task UploadAlbum(Album album)
        {
            
         
                try
                {
                    _context.Album.Add(album);
                    await _context.SaveChangesAsync();
                }catch(Exception ex)
                {
                    SentrySdk.CaptureException(ex);
               }
            
        }

        public async Task<List<Album>> GetAllAblumsForArtist(string artistName)
        {
            
            return await _context.Album.Where(x => x.Artist == artistName).ToListAsync();
        }

        public async Task<List<Album>> GetAllAblums()
        {
            
            return await _context.Album.OrderBy(x => x.Title).ToListAsync();
        }

        public async Task<List<Album>> GetFavouriteAblums()
        {
            
            return await _context.Album.Where(x => x.Favourite == true).ToListAsync();
        }

        public async Task<List<Album>> GetRecentAblums()
        {
            
            return await _context.Album.OrderBy(x => x.DateAdded).Take(20).ToListAsync();
        }

        public async Task<List<Album>> GetRecentReleasedAlbums()
        {
            
            return await _context.Album.Where(x => x.Year != 0).OrderByDescending(x => x.Year).Take(40).ToListAsync();
        }

        public async Task<List<Songs>> GetAllSongs()
        {
            
            return await _context.Songs
                .Select(s => new Songs
                    {
                        Id = s.Id,
                        TrackNumber = s.TrackNumber ?? null,
                        Title = s.Title ?? "",
                        Album = s.Album ?? "",
                        AlbumId = s.AlbumId ?? "",
                        Artist = s.Artist ?? "",
                        ArtistId = s.ArtistId ?? "",
                        AlbumPicture = s.AlbumPicture ?? "",
                        Favourite = s.Favourite,
                        DiscNumber = s.DiscNumber,
                        Length = s.Length ?? "",
                        Path = s.Path ?? "",
                        MusicBrainzId = s.MusicBrainzId ?? "",
                        BitDepth = s.BitDepth ?? "",
                        BitRate = s.BitRate ?? "",
                        SampleRate = s.SampleRate ?? "",
                        Codec = s.Codec ?? "",
                        PlayCount = _context.PlaybackHistory.Count(p => p.SongId == s.Id)
                    })
                .OrderBy(x => x.Title)
                .ToListAsync();
        }

        public async Task<List<Songs>> GetFavouriteSongs()
        {
            
            return await _context.Songs.Where(x => x.Favourite == true).ToListAsync();
        }

        public async Task<List<Artists>> GetAllArtists()
        {
            
            return await _context.Artists.OrderBy(x => x.Name).ToListAsync();
        }


        public async Task<Artists> GetArtist(string artist)
        {
            
            return await _context.Artists.FirstAsync(x => x.Name == artist);
        }

        public async Task<Artists?> GetArtistById(string artistId)
        {
            
            return await _context.Artists.FirstOrDefaultAsync(x => x.Id == artistId);
        }


        public async Task<List<Artists>> GetFavouriteArtists()
        {
            
            return await _context.Artists.Where(x => x.Favourite == true).ToListAsync();
        }

        public async Task<Songs> GetSongById(string songId)
        {
            
            var song = await _context.Songs.FirstAsync(x => x.Id == songId);
            song.PlayCount = await _context.PlaybackHistory.CountAsync(x => x.SongId == songId);
            return song;
        }

        public async Task<Songs> GetSong(string artist, string album, string title)
        {
            
            return await _context.Songs.FirstAsync(x => x.Artist == artist && x.Album == album  && x.Title == title);
        }

        public async Task UploadArtists(List<Artists> artists)
        {
            
     
                try
                {
                    _context.Artists.AddRange(artists);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Console.WriteLine(ex.ToString());
                }
            
        }


        public async Task UploadArtist(Artists artists)
        {
            
        
                try
                {
                    _context.Artists.Add(artists);
                    await _context.SaveChangesAsync();
                }catch(Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Console.WriteLine(ex.ToString());
                }
            
        }

        public async Task UpdateArtist(Artists artist)
        {
            
        
                try
                {
                    _context.Artists.Update(artist);
                    await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Console.WriteLine(ex.ToString());
                }

            

        }

        public async Task UpdateAlbum(Album album)
        {
            
         

                try
                {
                   _context.Album.Update(album);
                   await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Console.WriteLine(ex.ToString());
                }

            

        }

        public async Task UpdateSong(Songs song)
        {
            
         
                try
                {
                    _context.Songs.Update(song);
                    await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    SentrySdk.CaptureMessage(song.Title + " - " + song.Artist);
                    Console.WriteLine(ex.ToString());
                }

            

        }


        public async Task UploadSongs(List<Songs> song)
        {


                try
                {
                    _context.Songs.AddRange(song);
                    await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Console.WriteLine(ex.ToString());
                }

            

        }

        public async Task UploadSong(Songs song)
        {
            
       

                try
                {
                    _context.Songs.Add(song);
                    await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Console.WriteLine(ex.ToString());
                }

            

        }

        public async Task CreateNewPlaylist(string playlistTitle)
        {
           
                try
                {
                    _context.Playlists.Add(new Playlists() { PlaylistId = Guid.NewGuid().ToString(), PlaylistName = playlistTitle });
                    await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Console.WriteLine(ex.ToString());
                }
            
        }

        public async Task<Playlists> GetPlaylist(string playlistId)
        {
            
            return await _context.Playlists.Where(x => x.PlaylistId == playlistId).Include(x => x.PlaylistItems).ThenInclude(p => p.Song).FirstOrDefaultAsync();
        }

        public async Task<List<Playlists>> GetPlaylists()
        {
            
            return await _context.Playlists.ToListAsync();
        }
        public async Task DeletePlaylist(string playlistId)
        {
            
            try
            {
                _context.Playlists.Where(x => x.PlaylistId == playlistId).ExecuteDelete();
                await _context.SaveChangesAsync();
            }catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task AddSongToPlaylist(string playlistId, string songId)
        {
            
            try
            {
                _context.PlaylistItems.Add(new PlaylistItems() { PlaylistId = playlistId, PlaylistItemId = Guid.NewGuid().ToString(), SongId = songId });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task DeleteSongFromPlaylist(string playlistId, string songId)
        {
            
            try
            {
                _context.PlaylistItems.Where(x => x.PlaylistId == playlistId).Where(y => y.SongId == songId).ExecuteDelete();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Console.WriteLine(ex.ToString());
            }
        }




        public async Task UpdateLastPlayback(PlaybackHistory playback, int Seconds)
        {

            try
            {
                playback.Seconds = Seconds;
                _context.PlaybackHistory.Update(playback);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Console.WriteLine(ex.ToString());
            }

        }

        //Playback
        public async Task StartRecordPlayback(string songId)
        {
            
            DateTime playbackStartTime = DateTime.UtcNow;
            
            try
            {

                await _context.Database.BeginTransactionAsync();
                var lastSong = await GetLastPlaySong();
                var song = await GetSongById(songId);
                int songLength = int.Parse(song.Length);
                if (lastSong != null)
                {
                    var fullSong = await GetSongById(lastSong.SongId);
                    if (DateTime.UtcNow < lastSong.PlaybackStart.AddSeconds(int.Parse(fullSong.Length)))
                    {

                        //update last song with seconds
                        var secondsLength = DateTime.UtcNow - lastSong.PlaybackStart;
                        await UpdateLastPlayback(lastSong, secondsLength.Seconds);
                        Console.WriteLine("Updated last Playback for: " + fullSong.Title + " With Seconds: " + secondsLength);
                        //add new song
                        await _context.PlaybackHistory.AddAsync(new PlaybackHistory() { SongId = songId, PlaybackStart = playbackStartTime });
                        Console.WriteLine("Playback logged for song: " + songId);

                    }
                    else
                    {
                        //Update the playback seconds for the last song
                        
                       await  UpdateLastPlayback(lastSong, int.Parse(fullSong.Length));
                        Console.WriteLine("Updated last Playback for: " + fullSong.Title + " With full song Length");

                        //add new song
                        await _context.PlaybackHistory.AddAsync(new PlaybackHistory() { SongId = songId, PlaybackStart = playbackStartTime });
                        Console.WriteLine("Playback logged for song: " + songId);
                    }
                }
                else
                {
                    await _context.PlaybackHistory.AddAsync(new PlaybackHistory() { SongId = songId, PlaybackStart = playbackStartTime});
                    Console.WriteLine("Playback logged for song: " + songId);
                }

                
               
                    
                 await _context.SaveChangesAsync();
                 await _context.Database.CommitTransactionAsync();
                 
               

            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Console.WriteLine("Error Logging Song: " + songId);
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<DateTime> GetLastPlayDate(string songId)
        {
            
            var value = await _context.PlaybackHistory.Where(x => x.SongId == songId).OrderByDescending(y => y.PlaybackStart).FirstOrDefaultAsync();
            if(value == null)return DateTime.UtcNow.AddDays(-1);
            return value.PlaybackStart;
        }
        public async Task<PlaybackHistory> GetLastPlaySong()
        {

            var value = await _context.PlaybackHistory.OrderByDescending(y => y.PlaybackStart).FirstOrDefaultAsync();
            
            return value;
        }

        public async Task<List<PlaybackCounts>> GetPlaybackHistory()
        {
            
            var songPlaybackCounts = _context.PlaybackHistory
                 .GroupBy(x => x.SongId)
                 .Select(g => new PlaybackCounts{ SongId = g.Key, PlaybackCount = g.Count(), TotalSeconds = g.Sum(x => x.Seconds) })
                 .OrderByDescending(x => x.PlaybackCount)
                 .ToList();
            return songPlaybackCounts;
        }

        public async Task<List<PlaybackHistory>> GetPlaybackHistoryForDay(DateOnly day)
        {
            var startDate = day.ToDateTime(new TimeOnly(hour: 0, minute: 0));
            var endDate = day.ToDateTime(new TimeOnly(hour: 23, minute: 59));
            var songPlaybackCounts = _context.PlaybackHistory
                 .Where(x => x.PlaybackStart >= startDate &&
                    x.PlaybackStart <= endDate)
                 .OrderByDescending(x => x.PlaybackStart)
                 .ToList();
            return songPlaybackCounts;
        }

        public async Task<List<PlaybackCounts>> GetPlaybackHistoryByDate(DateTime startDate, DateTime endDate, int playbackTime)
        {
            var songPlaybackCounts = _context.PlaybackHistory
                 .Where(x => x.PlaybackStart >= startDate &&
                    x.PlaybackStart <= endDate)
                 .Where(y => y.Seconds >= playbackTime)
                 .GroupBy(x => x.SongId)
                 .Select(g => new PlaybackCounts { SongId = g.Key, PlaybackCount = g.Count(), TotalSeconds = g.Sum(x => x.Seconds) })
                 .OrderByDescending(x => x.PlaybackCount)
                 .ToList();
            return songPlaybackCounts;
        }

        public async Task<List<PlaybackDays>> GetPlaybackByDays(DateOnly startDate, DateOnly endDate)
        {
            var playbackDays = _context.PlaybackHistory
                .Where(x => x.PlaybackStart.Date >= DateTime.Parse(startDate.ToString()) &&
                            x.PlaybackStart.Date <= DateTime.Parse(endDate.ToString()))
                .GroupBy(x => x.PlaybackStart.Date)
                .Select(g => new PlaybackDays
                {
                    Day = DateOnly.FromDateTime(g.Key),
                    TotalSeconds = g.Sum(x => x.Seconds)
                })
                .ToList();


            var totalDays = (endDate.DayNumber - startDate.DayNumber)+1;
            
            for (var idd = 0; idd < totalDays; idd++)
            {
                var result = playbackDays.Where(x => x.Day == startDate.AddDays(idd)).FirstOrDefault();
                if(result == null)
                {
                    playbackDays.Insert(idd, new PlaybackDays{Day = startDate.AddDays(idd), TotalSeconds = 0 });
                }
            }


            return playbackDays;
        }

        public async Task<List<PlaybackArtists>> GetPlaybackHistoryArtists(DateTime startDate, DateTime endDate)
        {

            var playbackArtists = _context.PlaybackHistory
                .Where(x => x.PlaybackStart >= startDate &&
                    x.PlaybackStart <= endDate)
                    .Join(
                        _context.Songs,
                        ph => ph.SongId,
                        s => s.Id,
                        (ph, s) => new {
                            ArtistId = s.ArtistId,
                            ArtistName = s.Artist,
                            Seconds = ph.Seconds
                        })
                    .GroupBy(x => new { x.ArtistId, x.ArtistName })
                    .Select(g => new PlaybackArtists
                    {
                        ArtistId = g.Key.ArtistId,
                        ArtistName = g.Key.ArtistName,
                        PlayCount = g.Count(),
                        TotalSeconds = g.Sum(x => x.Seconds)
                    })
                    .ToList();
            return playbackArtists;
        }


        public async Task<string> GetMusicBrainzUrl(string artist, string ablum)
        {
            
            var mbid = _context.Album.Where(x => x.Artist == artist).Where(y =>y.Title == ablum).FirstOrDefault();
            if (mbid == null) return null;
            return mbid.MusicBrainzId ?? "";
        }
        
        
        //CONFIG
        public async Task<string?> GetConfigValue(string configName)
        {
            try
            {
                var value = await _context.Config.FirstOrDefaultAsync(x => x.ConfigName == configName);
                if (value == null)
                {
                    return null;
                }

                return value.ConfigValue;
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                return null;
            }

            
        }

        public async Task SetConfigValue(String configName, String value)
        {
            var oldValue = _context.Config.Where(x => x.ConfigName == configName).FirstOrDefault();
            if (oldValue == null)
            {
                try
                {
                  _context.Config.Add(new Config {ConfigId = Guid.NewGuid() ,ConfigName = configName, ConfigValue = value });
                  await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    
                }
                   }
            else
            {
                oldValue.ConfigValue = value;
                _context.Config.Update(oldValue);
                await _context.SaveChangesAsync();
            }
            
        }
    }
}
