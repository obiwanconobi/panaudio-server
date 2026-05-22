using System;
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
    public class ImageHelperTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private SqliteHelper _sqliteHelper;
        private ImageHelper _helper;

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
            _helper = new ImageHelper(_sqliteHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
            _context?.Dispose();
        }

        [Test]
        public void ImagePath_ReturnsCorrectPath_WhenAlbumExists()
        {
            var albumId = Guid.NewGuid().ToString();
            var album = new Album(
                id: albumId,
                title: "Test Album",
                artist: "Test Artist",
                albumPath: "/music/albums/test",
                picture: "cover.jpg"
            );
            _context.Album.Add(album);
            _context.SaveChanges();

            var result = _helper.ImagePath(albumId);

            Assert.AreEqual("/music/albums/test/cover.jpg", result);
        }

        [Test]
        public void ImagePath_ReturnsPathWithoutPicture_WhenPictureIsNull()
        {
            var albumId = Guid.NewGuid().ToString();
            var album = new Album(
                id: albumId,
                title: "Test Album",
                artist: "Test Artist",
                albumPath: "/music/albums/test",
                picture: null
            );
            _context.Album.Add(album);
            _context.SaveChanges();

            var result = _helper.ImagePath(albumId);

            Assert.AreEqual("/music/albums/test", result);
        }

        [Test]
        public void ImagePath_ReturnsPathWithoutPicture_WhenPictureIsEmpty()
        {
            var albumId = Guid.NewGuid().ToString();
            var album = new Album(
                id: albumId,
                title: "Test Album",
                artist: "Test Artist",
                albumPath: "/music/albums/test",
                picture: ""
            );
            _context.Album.Add(album);
            _context.SaveChanges();

            var result = _helper.ImagePath(albumId);

            Assert.AreEqual("/music/albums/test", result);
        }

        [Test]
        public async Task SetImage_UpdatesAlbumPicture()
        {
            var albumId = Guid.NewGuid().ToString();
            var album = new Album(
                id: albumId,
                title: "Test Album",
                artist: "Test Artist",
                albumPath: "/music/albums/test",
                picture: "old.jpg"
            );
            _context.Album.Add(album);
            _context.SaveChanges();

            await _helper.SetImage(albumId, "new-image.png");

            var updated = _context.Album.Find(albumId);
            Assert.AreEqual("new-image.png", updated.Picture);
        }

        [Test]
        public async Task SetImage_DoesNotThrow_WhenAlbumExists()
        {
            var albumId = Guid.NewGuid().ToString();
            var album = new Album(
                id: albumId,
                title: "Test Album",
                artist: "Test Artist",
                albumPath: "/music/albums/test",
                picture: "image.png"
            );
            _context.Album.Add(album);
            _context.SaveChanges();

            Assert.DoesNotThrowAsync(async () => await _helper.SetImage(albumId, "cover.jpg"));
        }

        [Test]
        public async Task ArtistImagePath_ReturnsCorrectPath_WhenArtistExists()
        {
            await _sqliteHelper.SetConfigValue("ArtistPictures", "True");

            var artistId = Guid.NewGuid().ToString();
            var artist = new Artists(
                id: artistId,
                name: "Test Artist",
                artistPath: "/music/artists/test",
                picture: "artist.jpg"
            );
            _context.Artists.Add(artist);
            _context.SaveChanges();

            var result = _helper.ArtistImagePath(artistId);

            Assert.AreEqual("/music/artists/test/artist.jpg", result);
        }

        [Test]
        public async Task ArtistImagePath_ReturnsPathWithoutPicture_WhenPictureIsNull()
        {
            await _sqliteHelper.SetConfigValue("ArtistPictures", "True");

            var artistId = Guid.NewGuid().ToString();
            var artist = new Artists(
                id: artistId,
                name: "Test Artist",
                artistPath: "/music/artists/test",
                picture: null
            );
            _context.Artists.Add(artist);
            _context.SaveChanges();

            var result = _helper.ArtistImagePath(artistId);

            Assert.AreEqual("/music/artists/test", result);
        }

        [Test]
        public async Task ArtistImagePath_ReturnsPathWithoutPicture_WhenPictureIsEmpty()
        {
            await _sqliteHelper.SetConfigValue("ArtistPictures", "True");

            var artistId = Guid.NewGuid().ToString();
            var artist = new Artists(
                id: artistId,
                name: "Test Artist",
                artistPath: "/music/artists/test",
                picture: ""
            );
            _context.Artists.Add(artist);
            _context.SaveChanges();

            var result = _helper.ArtistImagePath(artistId);

            Assert.AreEqual("/music/artists/test", result);
        }

        [Test]
        public async Task ArtistImagePath_DoesNotThrow_WhenArtistPathIsNull()
        {
            await _sqliteHelper.SetConfigValue("ArtistPictures", "True");

            var artistId = Guid.NewGuid().ToString();
            var artist = new Artists(
                id: artistId,
                name: "Test Artist",
                artistPath: null,
                picture: "photo.jpg"
            );
            _context.Artists.Add(artist);
            _context.SaveChanges();

            Assert.DoesNotThrow(() => _helper.ArtistImagePath(artistId));
        }

        [Test]
        public void ArtistImagePath_ReturnsEmpty_WhenArtistPicturesDisabled()
        {
            var artistId = Guid.NewGuid().ToString();
            var artist = new Artists(
                id: artistId,
                name: "Test Artist",
                artistPath: "/music/artists/test",
                picture: "artist.jpg"
            );
            _context.Artists.Add(artist);
            _context.SaveChanges();

            var result = _helper.ArtistImagePath(artistId);

            Assert.AreEqual("", result);
        }

        [Test]
        public void ArtistImagePath_ReturnsEmpty_WhenArtistPicturesDisabledAndNoArtist()
        {
            var result = _helper.ArtistImagePath("nonexistent");

            Assert.AreEqual("", result);
        }
    }
}
