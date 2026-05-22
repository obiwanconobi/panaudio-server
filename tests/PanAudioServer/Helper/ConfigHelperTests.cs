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
    }
}
