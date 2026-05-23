using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PanAudioServer.Data;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Tests.Helper
{
    [TestFixture]
    public class SqliteHelperTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private SqliteHelper _helper;

        [SetUp]
        public async Task SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SqliteContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new SqliteContext(options);
            _context.Database.EnsureCreated();

            _helper = new SqliteHelper(_context);
        }

        [TearDown]
        public async Task TearDown()
        {
            _connection?.Dispose();
            _context?.Dispose();
        }

        private Album SeedAlbum(string id = null, string title = "Test Album", string artist = "Test Artist",
            string albumPath = "/music/test", string picture = null, bool favourite = false, int? year = null,
            DateTime dateAdded = default, string musicBrainzId = null)
        {
            var album = new Album(id ?? Guid.NewGuid().ToString(), title, artist, year: year,
                picture: picture, albumPath: albumPath, dateAdded: dateAdded == default ? DateTime.UtcNow : dateAdded,
                favourite: favourite, musicBrainzId: musicBrainzId);
            _context.Album.Add(album);
            return album;
        }

        private Artists SeedArtist(string id = null, string name = "Test Artist",
            string artistPath = "/music/artists/test", string picture = null, bool favourite = false,
            string musicBrainzId = null)
        {
            var artist = new Artists(id ?? Guid.NewGuid().ToString(), name, artistPath,
                picture, favourite, musicBrainzId);
            _context.Artists.Add(artist);
            return artist;
        }

        private Songs SeedSong(string id = null, string title = "Test Song", string artist = "Test Artist",
            string album = "Test Album", string albumId = null, string artistId = null,
            string path = "/music/test/song.mp3", bool favourite = false, int? trackNumber = null,
            string length = "240")
        {
            var song = new Songs
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Title = title,
                Artist = artist,
                Album = album,
                AlbumId = albumId ?? Guid.NewGuid().ToString(),
                ArtistId = artistId ?? Guid.NewGuid().ToString(),
                AlbumPicture = "",
                Path = path,
                Favourite = favourite,
                TrackNumber = trackNumber,
                Length = length,
                DiscNumber = 1,
                Codec = "mp3",
                BitRate = "320",
                BitDepth = "16",
                SampleRate = "44100"
            };
            _context.Songs.Add(song);
            return song;
        }

        // ─── Album Tests ───────────────────────────────────────────────

        [Test]
        public async Task GetAlbumById_ReturnsAlbum_WhenExists()
        {
            var album = SeedAlbum("alb-1", "My Album", "My Artist");
            _context.SaveChanges();

            var result = await _helper.GetAlbumById("alb-1");

            Assert.AreEqual("My Album", result.Title);
            Assert.AreEqual("My Artist", result.Artist);
        }

        [Test]
        public async Task GetAlbum_ReturnsAlbum_WhenArtistAndTitleMatch()
        {
            SeedAlbum("alb-1", "Unique Album", "Unique Artist");
            _context.SaveChanges();

            var result = await _helper.GetAlbum("Unique Artist", "Unique Album");

            Assert.AreEqual("alb-1", result.Id);
        }

        [Test]
        public async Task GetAlbum_ReturnsNull_WhenNoMatch()
        {
            var result = await _helper.GetAlbum("NonExistent", "NonExistent");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllAblums_ReturnsAllOrderedByTitle()
        {
            SeedAlbum("a", "Z Album", "Artist");
            SeedAlbum("b", "A Album", "Artist");
            _context.SaveChanges();

            var result = await _helper.GetAllAblums();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("A Album", result[0].Title);
            Assert.AreEqual("Z Album", result[1].Title);
        }

        [Test]
        public async Task GetAllAblums_ReturnsEmptyList_WhenNoAlbums()
        {
            _context.SaveChanges();

            var result = await _helper.GetAllAblums();

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetFavouriteAblums_ReturnsOnlyFavourites()
        {
            SeedAlbum("a", "Regular", "Artist", favourite: false);
            SeedAlbum("b", "Favourite", "Artist", favourite: true);
            _context.SaveChanges();

            var result = await _helper.GetFavouriteAblums();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Favourite", result[0].Title);
        }

        [Test]
        public async Task GetRecentAblums_ReturnsUpToTwentyByDateAdded()
        {
            for (int i = 0; i < 25; i++)
                SeedAlbum(title: $"Album {i:D2}", artist: "Artist", dateAdded: DateTime.UtcNow.AddDays(-i));
            _context.SaveChanges();

            var result = await _helper.GetRecentAblums();

            Assert.AreEqual(20, result.Count);
        }

        [Test]
        public async Task GetRecentReleasedAlbums_ReturnsAlbumsWithNonZeroYear()
        {
            SeedAlbum("a", "Old Album", "Artist", year: 2020);
            SeedAlbum("b", "Recent Album", "Artist", year: 2024);
            SeedAlbum("c", "No Year Album", "Artist", year: 0);
            _context.SaveChanges();

            var result = await _helper.GetRecentReleasedAlbums();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Recent Album", result[0].Title);
        }

        [Test]
        public async Task GetAllAblumsForArtist_ReturnsAlbumsForGivenArtist()
        {
            SeedAlbum("a", "Album One", "Artist A");
            SeedAlbum("b", "Album Two", "Artist B");
            SeedAlbum("c", "Album Three", "Artist A");
            _context.SaveChanges();

            var result = await _helper.GetAllAblumsForArtist("Artist A");

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task UploadAlbum_PersistsToDatabase()
        {
            var album = new Album("alb-1", "New Album", "New Artist", albumPath: "/music/new");
            _context.SaveChanges();

            await _helper.UploadAlbum(album);

            var saved = _context.Album.Find("alb-1");
            Assert.IsNotNull(saved);
            Assert.AreEqual("New Album", saved.Title);
        }

        [Test]
        public async Task UpdateAlbum_PersistsChanges()
        {
            var album = SeedAlbum("alb-1", "Original", "Artist");
            _context.SaveChanges();

            _context.Entry(album).State = EntityState.Detached;
            var updated = _context.Album.Find("alb-1");
            updated.Title = "Changed";
            updated.Picture = "cover.jpg";
            await _helper.UpdateAlbum(updated);

            _context.ChangeTracker.Clear();
            var result = _context.Album.Find("alb-1");
            Assert.AreEqual("Changed", result.Title);
            Assert.AreEqual("cover.jpg", result.Picture);
        }

        // ─── Song Tests ────────────────────────────────────────────────

        [Test]
        public async Task GetSongById_ReturnsSongWithPlayCount()
        {
            var song = SeedSong("song-1", "Track 1", "Artist", "Album");
            _context.PlaybackHistory.Add(new PlaybackHistory
            {
                SongId = "song-1",
                PlaybackStart = DateTime.UtcNow,
                Seconds = 120
            });
            _context.PlaybackHistory.Add(new PlaybackHistory
            {
                SongId = "song-1",
                PlaybackStart = DateTime.UtcNow.AddDays(-1),
                Seconds = 180
            });
            _context.SaveChanges();

            var result = await _helper.GetSongById("song-1");

            Assert.AreEqual("Track 1", result.Title);
            Assert.AreEqual(2, result.PlayCount);
        }

        [Test]
        public async Task GetSong_ReturnsSongByArtistAlbumTitle()
        {
            SeedSong("song-1", "Specific Song", "Specific Artist", "Specific Album");
            _context.SaveChanges();

            var result = await _helper.GetSong("Specific Artist", "Specific Album", "Specific Song");

            Assert.AreEqual("song-1", result.Id);
        }

        [Test]
        public async Task GetAllSongs_ReturnsAllWithPlayCounts()
        {
            var song1 = SeedSong("song-1", "Song A", "Artist", "Album");
            var song2 = SeedSong("song-2", "Song B", "Artist", "Album");
            _context.PlaybackHistory.Add(new PlaybackHistory
            {
                SongId = "song-1",
                PlaybackStart = DateTime.UtcNow,
                Seconds = 100
            });
            _context.SaveChanges();

            var result = await _helper.GetAllSongs();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result.First(s => s.Id == "song-1").PlayCount);
            Assert.AreEqual(0, result.First(s => s.Id == "song-2").PlayCount);
        }

        [Test]
        public async Task GetAllSongs_ReturnsEmptyList_WhenNoSongs()
        {
            _context.SaveChanges();

            var result = await _helper.GetAllSongs();

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetFavouriteSongs_ReturnsOnlyFavourites()
        {
            SeedSong("song-1", "Regular", "Artist", "Album", favourite: false);
            SeedSong("song-2", "Fave", "Artist", "Album", favourite: true);
            _context.SaveChanges();

            var result = await _helper.GetFavouriteSongs();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Fave", result[0].Title);
        }

        [Test]
        public async Task UploadSong_PersistsToDatabase()
        {
            var song = new Songs
            {
                Id = "song-1",
                Title = "New Song",
                Artist = "New Artist",
                Album = "New Album",
                AlbumId = "alb-1",
                ArtistId = "art-1",
                AlbumPicture = "",
                Path = "/music/new/song.mp3",
                Length = "200",
                DiscNumber = 1,
                Codec = "mp3",
                BitRate = "320",
                BitDepth = "16",
                SampleRate = "44100"
            };
            _context.SaveChanges();

            await _helper.UploadSong(song);

            var saved = _context.Songs.Find("song-1");
            Assert.IsNotNull(saved);
            Assert.AreEqual("New Song", saved.Title);
        }

        [Test]
        public async Task UpdateSong_PersistsChanges()
        {
            var song = SeedSong("song-1", "Original Title", "Artist", "Album");
            _context.SaveChanges();

            _context.Entry(song).State = EntityState.Detached;
            var updated = _context.Songs.Find("song-1");
            updated.Title = "Updated Title";
            updated.Favourite = true;
            await _helper.UpdateSong(updated);

            _context.ChangeTracker.Clear();
            var result = _context.Songs.Find("song-1");
            Assert.AreEqual("Updated Title", result.Title);
            Assert.IsTrue(result.Favourite);
        }

        [Test]
        public async Task UploadSongs_PersistsMultipleAtOnce()
        {
            _context.ChangeTracker.Clear();
            var songs = new List<Songs>
            {
                new Songs { Id = Guid.NewGuid().ToString(), Title = "Song 1", Artist = "A", Album = "B", AlbumId = "a1", ArtistId = "r1",
                    AlbumPicture = "", Path = "/p1", DiscNumber = 1, Codec = "mp3", BitRate = "320", BitDepth = "16", SampleRate = "44100", Length = "200" },
                new Songs { Id = Guid.NewGuid().ToString(), Title = "Song 2", Artist = "A", Album = "B", AlbumId = "a1", ArtistId = "r1",
                    AlbumPicture = "", Path = "/p2", DiscNumber = 1, Codec = "mp3", BitRate = "320", BitDepth = "16", SampleRate = "44100", Length = "200" }
            };
            await _helper.UploadSongs(songs);

            Assert.AreEqual(2, _context.Songs.Count());
        }

        // ─── Artist Tests ──────────────────────────────────────────────

        [Test]
        public async Task GetArtist_ReturnsArtistByName()
        {
            var artist = SeedArtist("art-1", "Specific Artist");
            _context.SaveChanges();

            var result = await _helper.GetArtist("Specific Artist");

            Assert.AreEqual("art-1", result.Id);
        }

        [Test]
        public async Task GetArtistById_ReturnsArtist_WhenExists()
        {
            var artist = SeedArtist("art-1", "An Artist");
            _context.SaveChanges();

            var result = await _helper.GetArtistById("art-1");

            Assert.AreEqual("An Artist", result.Name);
        }

        [Test]
        public async Task GetArtistById_ReturnsNull_WhenNotFound()
        {
            _context.SaveChanges();

            var result = await _helper.GetArtistById("nonexistent");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllArtists_ReturnsAllOrderedByName()
        {
            SeedArtist("a", "Z Artist");
            SeedArtist("b", "A Artist");
            _context.SaveChanges();

            var result = await _helper.GetAllArtists();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("A Artist", result[0].Name);
            Assert.AreEqual("Z Artist", result[1].Name);
        }

        [Test]
        public async Task GetFavouriteArtists_ReturnsOnlyFavourites()
        {
            SeedArtist("a", "Regular", favourite: false);
            SeedArtist("b", "Fave", favourite: true);
            _context.SaveChanges();

            var result = await _helper.GetFavouriteArtists();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Fave", result[0].Name);
        }

        [Test]
        public async Task UploadArtist_PersistsToDatabase()
        {
            var artist = new Artists("art-1", "New Artist", "/music/new");

            await _helper.UploadArtist(artist);

            var saved = _context.Artists.Find("art-1");
            Assert.IsNotNull(saved);
            Assert.AreEqual("New Artist", saved.Name);
        }

        [Test]
        public async Task UpdateArtist_PersistsChanges()
        {
            var artist = SeedArtist("art-1", "Original Name");
            _context.SaveChanges();

            _context.Entry(artist).State = EntityState.Detached;
            var updated = _context.Artists.Find("art-1");
            updated.Name = "Updated Name";
            updated.Favourite = true;
            await _helper.UpdateArtist(updated);

            _context.ChangeTracker.Clear();
            var result = _context.Artists.Find("art-1");
            Assert.AreEqual("Updated Name", result.Name);
            Assert.IsTrue(result.Favourite);
        }

        [Test]
        public async Task UploadArtists_PersistsMultipleAtOnce()
        {
            var artists = new List<Artists>
            {
                new Artists("a1", "Artist One", "/path1"),
                new Artists("a2", "Artist Two", "/path2")
            };
            await _helper.UploadArtists(artists);

            Assert.AreEqual(2, _context.Artists.Count());
        }

        // ─── Playlist Tests ────────────────────────────────────────────

        [Test]
        public async Task CreateNewPlaylist_CreatesWithAutoId()
        {
            await _helper.CreateNewPlaylist("My Playlist");

            var playlists = _context.Playlists.ToList();
            Assert.AreEqual(1, playlists.Count);
            Assert.AreEqual("My Playlist", playlists[0].PlaylistName);
            Assert.IsNotEmpty(playlists[0].PlaylistId);
        }

        [Test]
        public async Task GetPlaylists_ReturnsAllPlaylists()
        {
            _context.Playlists.Add(new Playlists { PlaylistId = "p1", PlaylistName = "List 1" });
            _context.Playlists.Add(new Playlists { PlaylistId = "p2", PlaylistName = "List 2" });
            _context.SaveChanges();

            var result = await _helper.GetPlaylists();

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetPlaylist_ReturnsPlaylistWithItems()
        {
            _context.Playlists.Add(new Playlists { PlaylistId = "p1", PlaylistName = "Test List" });
            SeedSong("s-1", "Song 1", "Artist", "Album", path: "/music/s1.mp3");
            _context.PlaylistItems.Add(new PlaylistItems
            {
                PlaylistItemId = Guid.NewGuid().ToString(),
                PlaylistId = "p1",
                SongId = "s-1"
            });
            _context.SaveChanges();

            var result = await _helper.GetPlaylist("p1");

            Assert.AreEqual("Test List", result.PlaylistName);
            Assert.AreEqual(1, result.PlaylistItems.Count);
            Assert.AreEqual("Song 1", result.PlaylistItems.First().Song.Title);
        }

        [Test]
        public async Task DeletePlaylist_RemovesCascade()
        {
            _context.Playlists.Add(new Playlists { PlaylistId = "p1", PlaylistName = "To Delete" });
            SeedSong("s-1", "A Song", "Artist", "Album");
            _context.PlaylistItems.Add(new PlaylistItems
            {
                PlaylistItemId = Guid.NewGuid().ToString(),
                PlaylistId = "p1",
                SongId = "s-1"
            });
            _context.SaveChanges();

            await _helper.DeletePlaylist("p1");

            Assert.AreEqual(0, _context.Playlists.Count());
            Assert.AreEqual(0, _context.PlaylistItems.Count());
        }

        [Test]
        public async Task AddSongToPlaylist_AddsItem()
        {
            _context.Playlists.Add(new Playlists { PlaylistId = "p1", PlaylistName = "List" });
            SeedSong("song-1", "A Song", "Artist", "Album");
            _context.SaveChanges();

            await _helper.AddSongToPlaylist("p1", "song-1");

            var item = _context.PlaylistItems.FirstOrDefault();
            Assert.IsNotNull(item);
            Assert.AreEqual("p1", item.PlaylistId);
            Assert.AreEqual("song-1", item.SongId);
        }

        [Test]
        public async Task DeleteSongFromPlaylist_RemovesItem()
        {
            _context.Playlists.Add(new Playlists { PlaylistId = "p1", PlaylistName = "List" });
            SeedSong("song-1", "A Song", "Artist", "Album");
            _context.PlaylistItems.Add(new PlaylistItems
            {
                PlaylistItemId = "pi-1",
                PlaylistId = "p1",
                SongId = "song-1"
            });
            _context.SaveChanges();

            await _helper.DeleteSongFromPlaylist("p1", "song-1");

            Assert.AreEqual(0, _context.PlaylistItems.Count());
        }

        // ─── Config Tests ──────────────────────────────────────────────

        [Test]
        public async Task GetConfigValue_ReturnsNull_WhenNotSet()
        {
            var result = await _helper.GetConfigValue("NonExistentKey");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetConfigValue_ReturnsValue_WhenSet()
        {
            await _helper.SetConfigValue("MyKey", "MyValue");

            var result = await _helper.GetConfigValue("MyKey");

            Assert.AreEqual("MyValue", result);
        }

        [Test]
        public async Task SetConfigValue_UpdatesExistingEntry()
        {
            await _helper.SetConfigValue("MyKey", "Original");
            await _helper.SetConfigValue("MyKey", "Updated");

            var result = await _helper.GetConfigValue("MyKey");

            Assert.AreEqual("Updated", result);
        }

        // ─── Misc Tests ────────────────────────────────────────────────

        [Test]
        public async Task GetMusicBrainzUrl_ReturnsNull_WhenAlbumNotFound()
        {
            var result = await _helper.GetMusicBrainzUrl("Unknown", "Unknown");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetMusicBrainzUrl_ReturnsMusicBrainzId_WhenSet()
        {
            SeedAlbum("alb-1", "MB Album", "MB Artist", musicBrainzId: "mbid-12345");
            _context.SaveChanges();

            var result = await _helper.GetMusicBrainzUrl("MB Artist", "MB Album");

            Assert.AreEqual("mbid-12345", result);
        }

        [Test]
        public async Task GetMusicBrainzUrl_ReturnsEmpty_WhenMusicBrainzIdIsNull()
        {
            SeedAlbum("alb-1", "MB Album", "MB Artist", musicBrainzId: null);
            _context.SaveChanges();

            var result = await _helper.GetMusicBrainzUrl("MB Artist", "MB Album");

            Assert.AreEqual("", result);
        }

        // ─── GetSongsByAlbumId Tests ────────────────────────────────────

        [Test]
        public async Task GetSongsByAlbumId_ReturnsSongsOrderedByDiscAndTrack()
        {
            var albumId = "alb-1";
            var song1 = SeedSong("song-1", "Track 2", "Artist A", "Album A", albumId: albumId, trackNumber: 2);
            song1.DiscNumber = 1;
            var song2 = SeedSong("song-2", "Track 1", "Artist A", "Album A", albumId: albumId, trackNumber: 1);
            song2.DiscNumber = 1;
            var song3 = SeedSong("song-3", "Track 1", "Artist A", "Album A", albumId: albumId, trackNumber: 1);
            song3.DiscNumber = 2;
            _context.SaveChanges();

            var result = await _helper.GetSongsByAlbumId(albumId);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("song-2", result[0].Id);
            Assert.AreEqual("song-1", result[1].Id);
            Assert.AreEqual("song-3", result[2].Id);
        }

        [Test]
        public async Task GetSongsByAlbumId_ReturnsEmptyList_WhenNoSongsMatch()
        {
            var result = await _helper.GetSongsByAlbumId("nonexistent");

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetSongsByAlbumId_ReturnsOnlyMatchingAlbumSongs()
        {
            var albumId1 = "alb-1";
            var albumId2 = "alb-2";
            SeedSong("song-1", "Track 1", "Artist", "Album", albumId: albumId1);
            SeedSong("song-2", "Track 2", "Artist", "Album 2", albumId: albumId2);
            _context.SaveChanges();

            var result = await _helper.GetSongsByAlbumId(albumId1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("song-1", result[0].Id);
        }

        [Test]
        public async Task GetSongsByAlbumId_IncludesPlayCount()
        {
            var albumId = "alb-1";
            SeedSong("song-1", "Track 1", "Artist", "Album", albumId: albumId);
            _context.PlaybackHistory.Add(new PlaybackHistory { SongId = "song-1", PlaybackStart = DateTime.UtcNow, Seconds = 120 });
            _context.PlaybackHistory.Add(new PlaybackHistory { SongId = "song-1", PlaybackStart = DateTime.UtcNow.AddDays(-1), Seconds = 180 });
            _context.SaveChanges();

            var result = await _helper.GetSongsByAlbumId(albumId);

            Assert.AreEqual(2, result[0].PlayCount);
        }

        // ─── GetAlbumsByArtistId Tests ──────────────────────────────────

        [Test]
        public async Task GetAlbumsByArtistId_ReturnsAlbumsForArtist()
        {
            var artistId = "art-1";
            SeedArtist(artistId, "My Artist");
            SeedAlbum("alb-1", "Album One", "My Artist");
            SeedAlbum("alb-2", "Album Two", "My Artist");
            _context.SaveChanges();

            var result = await _helper.GetAlbumsByArtistId(artistId);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(a => a.Title == "Album One"));
            Assert.IsTrue(result.Any(a => a.Title == "Album Two"));
        }

        [Test]
        public async Task GetAlbumsByArtistId_ReturnsEmptyList_WhenArtistNotFound()
        {
            var result = await _helper.GetAlbumsByArtistId("nonexistent");

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetAlbumsByArtistId_ReturnsEmptyList_WhenNoAlbumsForArtist()
        {
            var artistId = "art-1";
            SeedArtist(artistId, "Lonely Artist");
            _context.SaveChanges();

            var result = await _helper.GetAlbumsByArtistId(artistId);

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetAlbumsByArtistId_ExcludesOtherArtistAlbums()
        {
            var artistId = "art-1";
            SeedArtist(artistId, "My Artist");
            SeedArtist("art-2", "Other Artist");
            SeedAlbum("alb-1", "My Album", "My Artist");
            SeedAlbum("alb-2", "Other Album", "Other Artist");
            _context.SaveChanges();

            var result = await _helper.GetAlbumsByArtistId(artistId);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("My Album", result[0].Title);
        }

        // ─── SearchSongs Tests ──────────────────────────────────────────

        [Test]
        public async Task SearchSongs_ReturnsMatchingSongs()
        {
            SeedSong("song-1", "Hello World", "Artist", "Album");
            SeedSong("song-2", "Goodbye Moon", "Artist", "Album");
            SeedSong("song-3", "Hello Sunshine", "Artist", "Album");
            _context.SaveChanges();

            var result = await _helper.SearchSongs("Hello");

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(s => s.Id == "song-1"));
            Assert.IsTrue(result.Any(s => s.Id == "song-3"));
        }

        [Test]
        public async Task SearchSongs_ReturnsEmptyList_WhenNoMatch()
        {
            SeedSong("song-1", "Hello World", "Artist", "Album");
            _context.SaveChanges();

            var result = await _helper.SearchSongs("zzzzz");

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task SearchSongs_IsCaseInsensitive()
        {
            SeedSong("song-1", "Hello World", "Artist", "Album");
            _context.SaveChanges();

            var result = await _helper.SearchSongs("hello");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Hello World", result[0].Title);
        }

        [Test]
        public async Task SearchSongs_IncludesPlayCount()
        {
            SeedSong("song-1", "Hello World", "Artist", "Album");
            _context.PlaybackHistory.Add(new PlaybackHistory { SongId = "song-1", PlaybackStart = DateTime.UtcNow, Seconds = 60 });
            _context.SaveChanges();

            var result = await _helper.SearchSongs("Hello");

            Assert.AreEqual(1, result[0].PlayCount);
        }

        // ─── SearchAlbums Tests ─────────────────────────────────────────

        [Test]
        public async Task SearchAlbums_ReturnsMatchingByTitle()
        {
            SeedAlbum("alb-1", "Dark Side", "Pink Floyd");
            SeedAlbum("alb-2", "The Wall", "Pink Floyd");
            SeedAlbum("alb-3", "Random", "Other Artist");
            _context.SaveChanges();

            var result = await _helper.SearchAlbums("Dark");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Dark Side", result[0].Title);
        }

        [Test]
        public async Task SearchAlbums_ReturnsMatchingByArtist()
        {
            SeedAlbum("alb-1", "Dark Side", "Pink Floyd");
            SeedAlbum("alb-2", "The Wall", "Pink Floyd");
            SeedAlbum("alb-3", "Random", "Other Artist");
            _context.SaveChanges();

            var result = await _helper.SearchAlbums("Pink");

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task SearchAlbums_ReturnsEmptyList_WhenNoMatch()
        {
            SeedAlbum("alb-1", "Dark Side", "Pink Floyd");
            _context.SaveChanges();

            var result = await _helper.SearchAlbums("zzzzz");

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task SearchAlbums_IsCaseInsensitive()
        {
            SeedAlbum("alb-1", "Dark Side", "Pink Floyd");
            _context.SaveChanges();

            var result = await _helper.SearchAlbums("dark");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Dark Side", result[0].Title);
        }

        // ─── SearchArtists Tests ────────────────────────────────────────

        [Test]
        public async Task SearchArtists_ReturnsMatchingArtists()
        {
            SeedArtist("art-1", "Pink Floyd");
            SeedArtist("art-2", "Pink");
            SeedArtist("art-3", "Red Hot Chili Peppers");
            _context.SaveChanges();

            var result = await _helper.SearchArtists("Pink");

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task SearchArtists_ReturnsEmptyList_WhenNoMatch()
        {
            SeedArtist("art-1", "Pink Floyd");
            _context.SaveChanges();

            var result = await _helper.SearchArtists("zzzzz");

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task SearchArtists_IsCaseInsensitive()
        {
            SeedArtist("art-1", "Pink Floyd");
            _context.SaveChanges();

            var result = await _helper.SearchArtists("pink");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Pink Floyd", result[0].Name);
        }
    }
}
