using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PanAudioServer.Data;
using PanAudioServer.Helper;

namespace PanAudioServer.Tests.Helper
{
    [TestFixture]
    public class ConfigHelperTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private SqliteHelper _sqliteHelper;
        private ConfigHelper _configHelper;

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

            _sqliteHelper = new SqliteHelper(_context);
            _configHelper = new ConfigHelper(_sqliteHelper);
        }

        [TearDown]
        public async Task TearDown()
        {
            _connection?.Dispose();
            _context?.Dispose();
        }

        [Test]
        public async Task GetPlaybackReportingTime_ReturnsDefaultFive_WhenNoConfigValue()
        {
            int result = await _configHelper.GetPlaybackReportingTime();

            Assert.AreEqual(5, result);
        }

        [Test]
        public async Task GetPlaybackReportingTime_ReturnsSetValue_WhenConfigValueExists()
        {
            _configHelper.SetPlaybackReportingTime("10");

            int result = await _configHelper.GetPlaybackReportingTime();

            Assert.AreEqual(10, result);
        }

        [Test]
        public async Task SetPlaybackReportingTime_DoesNotThrow()
        {
            Assert.DoesNotThrowAsync(async () => await _configHelper.SetPlaybackReportingTime("10"));
        }

        [Test]
        public async Task GetArtistPictures_ReturnsFalse_WhenNoConfigValue()
        {
            bool result = await _configHelper.GetArtistPictures();

            Assert.IsFalse(result);
        }

        [Test]
        public async Task GetArtistPictures_ReturnsTrue_WhenConfigValueIsTrue()
        {
            _configHelper.SetArtistPictures(true);

            bool result = await _configHelper.GetArtistPictures();

            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetArtistPictures_ReturnsFalse_WhenConfigValueIsFalse()
        {
            _configHelper.SetArtistPictures(false);

            bool result = await _configHelper.GetArtistPictures();

            Assert.IsFalse(result);
        }

        [Test]
        public async Task SetArtistPictures_DoesNotThrow()
        {
            Assert.DoesNotThrowAsync(async () => await _configHelper.SetArtistPictures(true));
            Assert.DoesNotThrowAsync(async () => await _configHelper.SetArtistPictures(false));
        }

        [Test]
        public async Task GetListenBrainzToken_ReturnsEmpty_WhenNoConfigValue()
        {
            string result = await _configHelper.GetListenBrainzToken();

            Assert.AreEqual("", result);
        }

        [Test]
        public async Task GetListenBrainzToken_ReturnsSetValue_WhenConfigValueExists()
        {
            await _configHelper.SetListenBrainzToken("test-token-123");

            string result = await _configHelper.GetListenBrainzToken();

            Assert.AreEqual("test-token-123", result);
        }

        [Test]
        public async Task SetListenBrainzToken_DoesNotThrow()
        {
            Assert.DoesNotThrowAsync(async () => await _configHelper.SetListenBrainzToken("test-token"));
        }
    }
}
