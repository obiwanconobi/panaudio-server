using ATL;
using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class ImageController : Controller
    {
        ImageHelper imageHelper = new ImageHelper();

        [HttpGet("albumArt")]
        public IActionResult GetAlbumArt(string albumId)
        {
            string albumArtPath = imageHelper.ImagePath(albumId);
            // Check if file exists
            if (!System.IO.File.Exists(albumArtPath))
            {
                // return NotFound($"Album art not found for ID: {albumId}");
                return extractImageFromFile(albumArtPath);
            }

            // Read the image file
            var imageBytes = System.IO.File.ReadAllBytes(albumArtPath);

            // Determine content type based on file extension
            var contentType = GetContentType(Path.GetExtension(albumArtPath));

            // Return the file with proper content type
            return File(imageBytes, contentType);
        }


        [HttpGet("artistArt")]
        public IActionResult GetArtistArt(string artistId)
        {
            string albumArtPath = imageHelper.ArtistImagePath(artistId);
            // Check if file exists
            if (!System.IO.File.Exists(albumArtPath))
            {
                return NotFound($"Artist art not found for ID: {artistId}");
                //return extractImageFromFile(albumArtPath);
            }

            // Read the image file
            var imageBytes = System.IO.File.ReadAllBytes(albumArtPath);

            // Determine content type based on file extension
            var contentType = GetContentType(Path.GetExtension(albumArtPath));

            // Return the file with proper content type
            return File(imageBytes, contentType);
        }


        private IActionResult extractImageFromFile(string path)
        {
            var files = Directory.GetFiles(path);
            Track file = new Track(files.Where(x => x.EndsWith(".flac")).FirstOrDefault());
            var embeddedPictures = file.EmbeddedPictures;
            if (embeddedPictures != null && embeddedPictures.Count > 0)
            {
                PictureInfo firstPicture = embeddedPictures[0];
                return File(firstPicture.PictureData, firstPicture.MimeType.ToString());
            }
            return null;
        }


        private string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
