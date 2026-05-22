using System;
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

            var sqliteHelper = new SqliteHelper(_context);
            _helper = new ImageHelper(sqliteHelper);
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
        public void SetImage_UpdatesAlbumPicture()
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

            _helper.SetImage(albumId, "new-image.png");

            var updated = _context.Album.Find(albumId);
            Assert.AreEqual("new-image.png", updated.Picture);
        }

        [Test]
        public void SetImage_DoesNotThrow_WhenAlbumExists()
        {
            var albumId = Guid.NewGuid().ToString();
            var album = new Album(
                id: albumId,
                title: "Test Album",
                artist: "Test Artist",
                albumPath: "/music/albums/test"
            );
            _context.Album.Add(album);
            _context.SaveChanges();

            Assert.DoesNotThrow(() => _helper.SetImage(albumId, "cover.jpg"));
        }

        [Test]
        public void ArtistImagePath_ReturnsCorrectPath_WhenArtistExists()
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

            Assert.AreEqual("/music/artists/test/artist.jpg", result);
        }

        [Test]
        public void ArtistImagePath_ReturnsPathWithoutPicture_WhenPictureIsNull()
        {
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
        public void ArtistImagePath_ReturnsPathWithoutPicture_WhenPictureIsEmpty()
        {
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
        public void ArtistImagePath_DoesNotThrow_WhenArtistPathIsNull()
        {
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
    }
}
