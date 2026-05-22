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
    public class MusicBrainzHelperTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private FakeHttpMessageHandler _httpHandler;
        private HttpClient _httpClient;
        private MusicBrainzHelper _helper;

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

            _httpHandler = new FakeHttpMessageHandler();
            _httpClient = new HttpClient(_httpHandler);

            var sqliteHelper = new SqliteHelper(_context);
            var imageHelper = new ImageHelper(sqliteHelper);
            _helper = new MusicBrainzHelper(_httpClient, sqliteHelper, imageHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            _httpHandler?.Dispose();
            _connection?.Dispose();
            _context?.Dispose();
        }

        [Test]
        public async Task getArtistIdAsync_ReturnsArtistId_FromApiResponse()
        {
            _httpHandler.SetResponse("http://musicbrainz.org", new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"created\":\"2024-01-01T00:00:00Z\",\"count\":1,\"offset\":0," +
                    "\"artists\":[{\"id\":\"abc-123-mbid\",\"type\":\"Group\",\"typeid\":\"type-id\"," +
                    "\"score\":100,\"name\":\"Test Artist\",\"sortname\":\"Test Artist\"," +
                    "\"country\":\"US\",\"isnis\":[],\"aliases\":[],\"tags\":[],\"disambiguation\":\"\"}]}"
                )
            });

            var result = await _helper.getArtistIdAsync("Test Artist");

            Assert.AreEqual("abc-123-mbid", result);
        }

        [Test]
        public async Task getArtistIdAsync_ReturnsEmptyString_WhenIdIsNull()
        {
            _httpHandler.SetResponse("http://musicbrainz.org", new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"created\":\"2024-01-01T00:00:00Z\",\"count\":1,\"offset\":0," +
                    "\"artists\":[{\"id\":null,\"type\":\"Group\",\"name\":\"Test Artist\"," +
                    "\"sortname\":\"Test Artist\",\"area\":null,\"beginarea\":null," +
                    "\"isnis\":[],\"aliases\":[],\"tags\":[],\"disambiguation\":\"\"}]}"
                )
            });

            var result = await _helper.getArtistIdAsync("Test Artist");

            Assert.AreEqual("", result);
        }

        [Test]
        public async Task getArtistIdAsync_Throws_WhenApiReturnsError()
        {
            _httpHandler.SetResponse("http://musicbrainz.org", new HttpResponseMessage(HttpStatusCode.InternalServerError));

            Assert.ThrowsAsync<HttpRequestException>(async () => await _helper.getArtistIdAsync("Test Artist"));
        }

        [Test]
        public async Task getAlbumArtAsync_ReturnsImageUrl_FromApiResponse()
        {
            _httpHandler.SetResponse("https://coverartarchive.org", new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"images\":[{\"id\":\"img-1\",\"image\":\"https://coverartarchive.org/release/img.jpg\"," +
                    "\"front\":true,\"back\":false,\"approved\":true,\"edit\":1,\"types\":[\"Front\"]," +
                    "\"thumbnails\":{\"small\":\"\",\"large\":\"\"}}],\"release\":\"https://musicbrainz.org/release/abc\"}"
                )
            });

            var result = await _helper.getAlbumArtAsync("abc-release");

            Assert.AreEqual("https://coverartarchive.org/release/img.jpg", result);
        }

        [Test]
        public async Task getAlbumArtAsync_Throws_WhenApiReturnsError()
        {
            _httpHandler.SetResponse("https://coverartarchive.org", new HttpResponseMessage(HttpStatusCode.NotFound));

            Assert.ThrowsAsync<HttpRequestException>(async () => await _helper.getAlbumArtAsync("bad-id"));
        }

        [Test]
        public async Task setArtistId_UpdatesArtistMusicBrainzId()
        {
            var artistId = Guid.NewGuid().ToString();
            var artist = new Artists(
                id: artistId,
                name: "Test Artist",
                artistPath: "/music/artists/test"
            );
            _context.Artists.Add(artist);
            _context.SaveChanges();

            _httpHandler.SetResponse("http://musicbrainz.org", new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"created\":\"2024-01-01T00:00:00Z\",\"count\":1,\"offset\":0," +
                    "\"artists\":[{\"id\":\"xyz-789-mbid\",\"type\":\"Group\",\"typeid\":\"type-id\"," +
                    "\"score\":100,\"name\":\"Test Artist\",\"sortname\":\"Test Artist\"," +
                    "\"country\":\"US\",\"isnis\":[],\"aliases\":[],\"tags\":[],\"disambiguation\":\"\"}]}"
                )
            });

            await _helper.setArtistId("Test Artist");

            var updated = _context.Artists.Find(artistId);
            Assert.AreEqual("xyz-789-mbid", updated.MusicBrainzId);
        }

        [Test]
        public async Task setArtistId_Throws_WhenApiFails()
        {
            var artistId = Guid.NewGuid().ToString();
            var artist = new Artists(
                id: artistId,
                name: "Test Artist",
                artistPath: "/music/artists/test"
            );
            _context.Artists.Add(artist);
            _context.SaveChanges();

            _httpHandler.SetResponse("http://musicbrainz.org", new HttpResponseMessage(HttpStatusCode.InternalServerError));

            Assert.ThrowsAsync<HttpRequestException>(async () => await _helper.setArtistId("Test Artist"));
        }
    }

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private string _matchUrlPrefix;
        private HttpResponseMessage _response;

        public void SetResponse(string urlPrefix, HttpResponseMessage response)
        {
            _matchUrlPrefix = urlPrefix;
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri != null && request.RequestUri.ToString().Contains(_matchUrlPrefix))
                return Task.FromResult(_response);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
