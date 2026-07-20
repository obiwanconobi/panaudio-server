using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        private FakeLBHttpMessageHandler _httpHandler;
        private HttpClient _httpClient;
        private ConfigHelper _configHelper;
        private PlaybackHelper _playbackHelperWithLB;

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

            _httpHandler = new FakeLBHttpMessageHandler();
            _httpClient = new HttpClient(_httpHandler);
            _configHelper = new ConfigHelper(_sqliteHelper);
            var lbClient = new ListenBrainzClient(_httpClient, _configHelper);
            _playbackHelperWithLB = new PlaybackHelper(_sqliteHelper, lbClient);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            _httpHandler?.Dispose();
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

        [Test]
        public async Task Backfill_ReturnsAllZeros_WhenNoTokenConfigured()
        {
            SeedSong("song-bf-1", "Backfill Song", length: "240");
            SeedPlaybackRow("song-bf-1", DateTime.UtcNow.AddHours(-1), 180);

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(0, result.Submitted);
            Assert.AreEqual(0, result.Skipped);
            Assert.AreEqual(0, result.Failed);
        }

        [Test]
        public async Task Backfill_ReturnsAllZeros_WhenNoRecordsInRange()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(0, result.Submitted);
            Assert.AreEqual(0, result.Skipped);
            Assert.AreEqual(0, result.Failed);
        }

        [Test]
        public async Task Backfill_CountsSubmitted_ForRecordsMeetingThreshold()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            SeedSong("song-bf-sub", "Submit Song", length: "240");
            SeedPlaybackRow("song-bf-sub", DateTime.UtcNow.AddHours(-1), 180);

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(1, result.Submitted);
            Assert.AreEqual(0, result.Skipped);
            Assert.AreEqual(0, result.Failed);
        }

        [Test]
        public async Task Backfill_CountsSkipped_ForRecordsBelowThreshold()
        {
            await _configHelper.SetListenBrainzToken("test-token");

            SeedSong("song-bf-skip", "Skip Song", length: "240");
            SeedPlaybackRow("song-bf-skip", DateTime.UtcNow.AddHours(-1), 10);

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(0, result.Submitted);
            Assert.AreEqual(1, result.Skipped);
            Assert.AreEqual(0, result.Failed);
        }

        [Test]
        public async Task Backfill_Threshold_IsMinOfHalfOr240()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            SeedSong("song-bf-long", "Long Song", length: "600");
            SeedPlaybackRow("song-bf-long", DateTime.UtcNow.AddHours(-1), 240);

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(1, result.Submitted);
            Assert.AreEqual(0, result.Skipped);
            Assert.AreEqual(0, result.Failed);
        }

        [Test]
        public async Task Backfill_CountsFailed_ForMissingSong()
        {
            await _configHelper.SetListenBrainzToken("test-token");

            _context.PlaybackHistory.Add(new PlaybackHistory
            {
                SongId = "nonexistent-song",
                PlaybackStart = DateTime.UtcNow.AddHours(-1),
                Seconds = 180
            });
            _context.SaveChanges();

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(0, result.Submitted);
            Assert.AreEqual(0, result.Skipped);
            Assert.AreEqual(1, result.Failed);
        }

        [Test]
        public async Task Backfill_CountsFailed_OnServerError()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.BadRequest));

            SeedSong("song-bf-fail", "Fail Song", length: "240");
            SeedPlaybackRow("song-bf-fail", DateTime.UtcNow.AddHours(-1), 180);

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(0, result.Submitted);
            Assert.AreEqual(0, result.Skipped);
            Assert.AreEqual(1, result.Failed);
        }

        [Test]
        public async Task Backfill_IgnoresRows_WithZeroSeconds()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            SeedSong("song-bf-zero", "Zero Sec Song", length: "240");
            _context.PlaybackHistory.Add(new PlaybackHistory
            {
                SongId = "song-bf-zero",
                PlaybackStart = DateTime.UtcNow.AddHours(-1),
                Seconds = 0
            });
            _context.SaveChanges();

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(0, result.Submitted);
            Assert.AreEqual(0, result.Skipped);
            Assert.AreEqual(0, result.Failed);
        }

        [Test]
        public async Task Backfill_UsesOriginalPlaybackStart()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var originalTime = new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc);
            SeedSong("song-bf-time", "Time Song", length: "240");
            SeedPlaybackRow("song-bf-time", originalTime, 180);

            await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                originalTime.AddDays(-1), originalTime.AddDays(1));

            var unixTime = new DateTimeOffset(originalTime).ToUnixTimeSeconds();
            Assert.IsTrue(_httpHandler.LastRequestBody.Contains(unixTime.ToString()));
        }

        [Test]
        public async Task Backfill_MixedResults_ReturnsCorrectCounts()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            SeedSong("song-mix-sub", "Mix Submit", length: "240");
            SeedSong("song-mix-skip", "Mix Skip", length: "240");

            SeedPlaybackRow("song-mix-sub", DateTime.UtcNow.AddHours(-1), 180);
            SeedPlaybackRow("song-mix-skip", DateTime.UtcNow.AddHours(-1), 10);

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(1, result.Submitted);
            Assert.AreEqual(1, result.Skipped);
            Assert.AreEqual(0, result.Failed);
        }

        [Test]
        public async Task Backfill_ColonDelimitedDuration_MeetsThreshold()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            SeedSong("song-bf-colon", "Colon Duration", length: "4:05");
            SeedPlaybackRow("song-bf-colon", DateTime.UtcNow.AddHours(-1), 180);

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(1, result.Submitted);
            Assert.AreEqual(0, result.Skipped);
            Assert.AreEqual(0, result.Failed);
        }

        [Test]
        public async Task Backfill_UsesImportListenType()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            SeedSong("song-bf-type", "Import Type", length: "240");
            SeedPlaybackRow("song-bf-type", DateTime.UtcNow.AddHours(-1), 180);

            await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.IsTrue(_httpHandler.LastRequestBody.Contains("\"listen_type\":\"import\""));
        }

        [Test]
        public async Task Backfill_BatchesMultipleSongsInOneRequest()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            SeedSong("song-bf-b1", "Batch 1", length: "240");
            SeedSong("song-bf-b2", "Batch 2", length: "240");
            SeedPlaybackRow("song-bf-b1", DateTime.UtcNow.AddHours(-1), 180);
            SeedPlaybackRow("song-bf-b2", DateTime.UtcNow.AddHours(-1), 180);

            var result = await _playbackHelperWithLB.BackfillHistoricalListensAsync(
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

            Assert.AreEqual(2, result.Submitted);
            Assert.IsTrue(_httpHandler.LastRequestBody.Contains("Batch 1"));
            Assert.IsTrue(_httpHandler.LastRequestBody.Contains("Batch 2"));
        }

        private void SeedPlaybackRow(string songId, DateTime playbackStart, int seconds)
        {
            _context.PlaybackHistory.Add(new PlaybackHistory
            {
                SongId = songId,
                PlaybackStart = playbackStart,
                Seconds = seconds
            });
            _context.SaveChanges();
        }
    }
}
