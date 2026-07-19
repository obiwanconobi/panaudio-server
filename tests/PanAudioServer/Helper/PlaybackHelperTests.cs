using System;
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
    public class PlaybackHelperTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private SqliteHelper _sqliteHelper;
        private PlaybackHelper _playbackHelper;

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

            _sqliteHelper = new SqliteHelper(_context);
            _playbackHelper = new PlaybackHelper(_sqliteHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
            _context?.Dispose();
        }

        private Songs SeedSong(string id = null, string title = "Test Song", string artist = "Test Artist",
            string album = "Test Album", string albumId = null, string artistId = null,
            string path = "/music/test/song.mp3", bool favourite = false, int? trackNumber = null,
            string length = "240", string musicBrainzId = null)
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
                SampleRate = "44100",
                MusicBrainzId = musicBrainzId
            };
            _context.Songs.Add(song);
            _context.SaveChanges();
            return song;
        }

        [Test]
        public async Task StartRecordPlayback_FirstSongEver_InsertsPendingRowOnly()
        {
            var song = SeedSong("song-1", "First Song");

            await _playbackHelper.StartRecordPlayback(song.Id);

            var history = _context.PlaybackHistory.ToList();
            Assert.AreEqual(1, history.Count);
            Assert.AreEqual("song-1", history[0].SongId);
            Assert.AreEqual(0, history[0].Seconds);
        }

        [Test]
        public async Task StartRecordPlayback_SecondSong_UpdatesPreviousSongSeconds()
        {
            var songA = SeedSong("song-a", "Song A", length: "240");
            var songB = SeedSong("song-b", "Song B", length: "180");

            await _playbackHelper.StartRecordPlayback(songA.Id);

            var aRow = _context.PlaybackHistory.Where(x => x.SongId == "song-a").First();
            aRow.PlaybackStart = DateTime.UtcNow.AddSeconds(-120);
            _context.SaveChanges();

            await _playbackHelper.StartRecordPlayback(songB.Id);

            var updatedA = _context.PlaybackHistory.Where(x => x.SongId == "song-a").First();
            Assert.GreaterOrEqual(updatedA.Seconds, 119);

            var history = _context.PlaybackHistory.ToList();
            Assert.AreEqual(2, history.Count);
        }

        [Test]
        public async Task StartRecordPlayback_HandlesColonDelimitedDuration()
        {
            var songA = SeedSong("song-cd-a", "Colon Song", length: "4:05");
            var songB = SeedSong("song-cd-b", "Next Song", length: "3:30");

            await _playbackHelper.StartRecordPlayback(songA.Id);

            var aRow = _context.PlaybackHistory.Where(x => x.SongId == "song-cd-a").First();
            aRow.PlaybackStart = DateTime.UtcNow.AddSeconds(-30);
            _context.SaveChanges();

            Assert.DoesNotThrowAsync(async () => await _playbackHelper.StartRecordPlayback(songB.Id));
        }

        [Test]
        public async Task StartRecordPlayback_DurationCalculation_UsesTotalSeconds()
        {
            var songA = SeedSong("song-ts-a", "TotalSec A", length: "240");
            var songB = SeedSong("song-ts-b", "TotalSec B", length: "180");

            await _playbackHelper.StartRecordPlayback(songA.Id);

            var aRow = _context.PlaybackHistory.Where(x => x.SongId == "song-ts-a").First();
            aRow.PlaybackStart = DateTime.UtcNow.AddSeconds(-125);
            _context.SaveChanges();

            await _playbackHelper.StartRecordPlayback(songB.Id);

            var updatedA = _context.PlaybackHistory.Where(x => x.SongId == "song-ts-a").First();
            Assert.GreaterOrEqual(updatedA.Seconds, 124);
        }

        [Test]
        public async Task StartRecordPlayback_WithFullSongElapsed_GivesFullLength()
        {
            var fullSongId = Guid.NewGuid().ToString();
            var songA = SeedSong(fullSongId, "Full Song", length: "90");
            var songB = SeedSong("song-b", "Next One", length: "180");

            await _playbackHelper.StartRecordPlayback(songA.Id);

            var aRow = _context.PlaybackHistory.Where(x => x.SongId == fullSongId).First();
            aRow.PlaybackStart = DateTime.UtcNow.AddSeconds(-100);
            _context.SaveChanges();

            await _playbackHelper.StartRecordPlayback(songB.Id);

            var updatedA = _context.PlaybackHistory.Where(x => x.SongId == fullSongId).First();
            Assert.AreEqual(90, updatedA.Seconds);
        }
    }
}
