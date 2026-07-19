using System;
using System.Net;
using System.Net.Http;
using System.Threading;
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
    public class ListenBrainzClientTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private FakeLBHttpMessageHandler _httpHandler;
        private HttpClient _httpClient;
        private SqliteHelper _sqliteHelper;
        private ConfigHelper _configHelper;
        private ListenBrainzClient _client;

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

            _httpHandler = new FakeLBHttpMessageHandler();
            _httpClient = new HttpClient(_httpHandler);

            _sqliteHelper = new SqliteHelper(_context);
            _configHelper = new ConfigHelper(_sqliteHelper);
            _client = new ListenBrainzClient(_httpClient, _configHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            _httpHandler?.Dispose();
            _connection?.Dispose();
            _context?.Dispose();
        }

        private Songs MakeSong(string title = "Test Song", string artist = "Test Artist",
            string album = "Test Album", string length = "240", string musicBrainzId = null)
        {
            return new Songs
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Artist = artist,
                Album = album,
                AlbumId = Guid.NewGuid().ToString(),
                ArtistId = Guid.NewGuid().ToString(),
                AlbumPicture = "",
                Length = length,
                DiscNumber = 1,
                Codec = "mp3",
                BitRate = "320",
                BitDepth = "16",
                SampleRate = "44100",
                MusicBrainzId = musicBrainzId,
                Path = "/music/test.mp3"
            };
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_WhenNoTokenConfigured()
        {
            var song = MakeSong();
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-60), 60);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task SubmitListen_ReturnsTrue_WhenBelowThreshold()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            var song = MakeSong(length: "240");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-10), 10);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task SubmitListen_ReturnsTrue_WhenMeetsThreshold_AndServerAccepts()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "240");
            var playbackStart = DateTime.UtcNow.AddSeconds(-180);
            var result = await _client.SubmitListenAsync(song, playbackStart, 180);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task SubmitListen_SendsCorrectAuthorizationHeader()
        {
            await _configHelper.SetListenBrainzToken("my-secret-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "240");
            await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.AreEqual("Token my-secret-token", _httpHandler.LastAuthHeader);
        }

        [Test]
        public async Task SubmitListen_IncludesRecordingMbid_WhenPresent()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "240", musicBrainzId: "mb-recording-123");
            await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsTrue(_httpHandler.LastRequestBody.Contains("mb-recording-123"));
        }

        [Test]
        public async Task SubmitListen_OmitsRecordingMbid_WhenNull()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "240");
            await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsFalse(_httpHandler.LastRequestBody.Contains("recording_mbid"));
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_OnNetworkError()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(null);

            var song = MakeSong(length: "240");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_OnHttp401()
        {
            await _configHelper.SetListenBrainzToken("bad-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var song = MakeSong(length: "240");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_OnHttp400()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var song = MakeSong(length: "240");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task SubmitListen_HandlesColonDelimitedDuration()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "4:05");
            await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsTrue(_httpHandler.LastRequestBody.Contains("\"duration_ms\":245000"));
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_WhenDurationIsZero()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            var song = MakeSong(length: "");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);
            Assert.IsFalse(result);
        }
    }

    public class FakeLBHttpMessageHandler : HttpMessageHandler
    {
        public string LastAuthHeader { get; private set; }
        public string LastRequestBody { get; private set; }
        private HttpResponseMessage _response;

        public void SetResponse(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastAuthHeader = request.Headers.Authorization?.ToString();
            LastRequestBody = request.Content?.ReadAsStringAsync().Result ?? "";

            if (_response == null)
                throw new HttpRequestException("Network error");

            return Task.FromResult(_response);
        }
    }
}
