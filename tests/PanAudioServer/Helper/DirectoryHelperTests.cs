using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PanAudioServer.Data;
using PanAudioServer.Helper;
using System.IO;

namespace PanAudioServer.Tests.Helper
{
    [TestFixture]
    public class DirectoryHelperTests
    {
        private string _tempRoot;
        private DirectoryHelper _helper;
        private SqliteConnection _connection;
        private SqliteContext _context;

        [SetUp]
        public void SetUp()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempRoot);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SqliteContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new SqliteContext(options);
            _context.Database.EnsureCreated();

            var sqliteHelper = new SqliteHelper(_context);
            _helper = new DirectoryHelper(sqliteHelper);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, true);

            _connection?.Dispose();
            _context?.Dispose();
        }

        [Test]
        public void getDirectories_ReturnsArray_WhenSubdirectoriesExist()
        {
            var subDir = Path.Combine(_tempRoot, "sub");
            Directory.CreateDirectory(subDir);

            var result = _helper.getDirectories(_tempRoot);

            CollectionAssert.AreEqual(new[] { subDir }, result);
        }

        [Test]
        public void GetDirectories_ReturnsNull_WhenNoSubdirectories()
        {
            var result = _helper.getDirectories(_tempRoot);

            Assert.IsNull(result);
        }

        [Test]
        public void RemoveShittyCharacters_DoesNotThrow_and_ReturnsString()
        {
            var input = "artist\u2010name\u2019song\u2012\u2013\u2014\u2011";

            var cleaned = _helper.removeShittyCharacters(input);

            Assert.IsNotNull(cleaned);
            Assert.IsTrue(cleaned.All(c => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '\''));
        }

        [Test]
        public void IsImage_ReturnsTrue_ForSupportedExtensions()
        {
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            foreach (var ext in extensions)
            {
                Assert.IsTrue(_helper.IsImage(ext), $"Extension {ext} should be considered an image");
            }
        }

        [Test]
        public void IsImage_ReturnsFalse_ForUnsupportedExtensions()
        {
            var extensions = new[] { ".txt", ".pdf", ".exe", "." };
            foreach (var ext in extensions)
            {
                Assert.IsFalse(_helper.IsImage(ext), $"Extension {ext} should not be considered an image");
            }
        }

        [Test]
        public async Task getDirectory_DoesNotThrow_OnValidPath()
        {
            var subFolder = Path.Combine(_tempRoot, "a", "b");
            Directory.CreateDirectory(subFolder);

            await _helper.getDirectory(_tempRoot, 0);
        }

        [Test]
        public async Task getSongs_DoesNotThrow_OnEmptyDirectory()
        {
            var emptyFolder = Path.Combine(_tempRoot, "empty");
            Directory.CreateDirectory(emptyFolder);

            await _helper.getSongs(emptyFolder);
        }
    }
}
