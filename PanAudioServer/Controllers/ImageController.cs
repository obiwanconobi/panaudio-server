using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;

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
            
           // string albumArtPath = @"\\192.168.1.15\ubuntu_media\nextcloud_2\conner\files\Music\Antarctigo Vespucci\2015 - Leavin' La Vida Loca\cover.jpg";
            //   var albumArtPath = PathGetter(albumId);
            string albumArtPath = imageHelper.ImagePath(albumId);
            // Check if file exists
            if (!System.IO.File.Exists(albumArtPath))
            {
                return NotFound($"Album art not found for ID: {albumId}");
            }

            // Read the image file
            var imageBytes = System.IO.File.ReadAllBytes(albumArtPath);

            // Determine content type based on file extension
            var contentType = GetContentType(Path.GetExtension(albumArtPath));

            // Return the file with proper content type
            return File(imageBytes, contentType);
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
