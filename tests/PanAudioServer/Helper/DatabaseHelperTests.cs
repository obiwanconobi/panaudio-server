using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PanAudioServer.Data;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Tests.Helper
{
    [TestFixture]
    public class DatabaseHelperTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private DatabaseHelper _helper;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SqliteContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new SqliteContext(options);
            _context.Database.EnsureCreated();

            var sqliteHelper = new SqliteHelper(_context);
            _helper = new DatabaseHelper(sqliteHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
            _context?.Dispose();
        }

        [Test]
        public void clearAll_DeletesAllRecords()
        {
            _context.Album.Add(new Album("album-1", "Test Album", "Test Artist", albumPath: "/music/test"));
            _context.Artists.Add(new Artists("artist-1", "Test Artist", "/music/artists/test"));
            _context.Songs.Add(new Songs
            {
                Id = "song-1",
                Title = "Test Song",
                Album = "Test Album",
                AlbumId = "album-1",
                Artist = "Test Artist",
                ArtistId = "artist-1",
                AlbumPicture = "",
                Path = "/music/test/song.mp3",
                DiscNumber = 1,
                Codec = "mp3",
                BitRate = "320",
                BitDepth = "16",
                SampleRate = "44100",
                Length = "240"
            });
            _context.Playlists.Add(new Playlists { PlaylistId = "playlist-1", PlaylistName = "Test Playlist" });
            _context.PlaybackHistory.Add(new PlaybackHistory
            {
                SongId = "song-1",
                PlaybackStart = DateTime.UtcNow
            });
            _context.SaveChanges();

            _helper.clearAll();

            Assert.AreEqual(0, _context.Album.Count());
            Assert.AreEqual(0, _context.Artists.Count());
            Assert.AreEqual(0, _context.Songs.Count());
            Assert.AreEqual(0, _context.Playlists.Count());
            Assert.AreEqual(0, _context.PlaybackHistory.Count());
        }

        [Test]
        public void clearAll_DoesNotThrow_WhenDbIsEmpty()
        {
            Assert.DoesNotThrowAsync(async () => await _helper.clearAll());
        }

        [Test]
        public void clearAll_ClearsOnlyPopulatedTables()
        {
            _context.Album.Add(new Album("album-1", "Test Album", "Test Artist", albumPath: "/music/test"));
            _context.SaveChanges();

            _helper.clearAll();

            Assert.AreEqual(0, _context.Album.Count());
            Assert.AreEqual(0, _context.Artists.Count());
        }
    }
}
