using System;
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
            _configHelper = new ConfigHelper(_sqliteHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
            _context?.Dispose();
        }

        [Test]
        public void GetPlaybackReportingTime_ReturnsDefaultFive_WhenNoConfigValue()
        {
            int result = _configHelper.GetPlaybackReportingTime();

            Assert.AreEqual(5, result);
        }

        [Test]
        public void GetPlaybackReportingTime_ReturnsSetValue_WhenConfigValueExists()
        {
            _configHelper.SetPlaybackReportingTime("10");

            int result = _configHelper.GetPlaybackReportingTime();

            Assert.AreEqual(10, result);
        }

        [Test]
        public void SetPlaybackReportingTime_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _configHelper.SetPlaybackReportingTime("10"));
        }

        [Test]
        public void GetArtistPictures_ReturnsFalse_WhenNoConfigValue()
        {
            bool result = _configHelper.GetArtistPictures();

            Assert.IsFalse(result);
        }

        [Test]
        public void GetArtistPictures_ReturnsTrue_WhenConfigValueIsTrue()
        {
            _configHelper.SetArtistPictures(true);

            bool result = _configHelper.GetArtistPictures();

            Assert.IsTrue(result);
        }

        [Test]
        public void GetArtistPictures_ReturnsFalse_WhenConfigValueIsFalse()
        {
            _configHelper.SetArtistPictures(false);

            bool result = _configHelper.GetArtistPictures();

            Assert.IsFalse(result);
        }

        [Test]
        public void SetArtistPictures_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _configHelper.SetArtistPictures(true));
            Assert.DoesNotThrow(() => _configHelper.SetArtistPictures(false));
        }
    }
}
