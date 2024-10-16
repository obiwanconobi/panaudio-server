using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using TagLib;
using PanAudioServer.Helper;


namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class AudioController : Controller
    {
        private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private readonly String _filePath = "\\Media\\Test\\2 - Neurotic.flac";
        SqliteHelper sqliteHelper = new SqliteHelper();

        [HttpGet("audio-dl")]
        public IActionResult DownloadAudio()
        {
            string _totalPath = _basePath + _filePath;

            try
            {
                var fileStream = new FileStream(_totalPath, FileMode.Open);
                var contentType = GetContentType(_totalPath);
                var fileName = Path.GetFileName(_totalPath);

                // Return FileStreamResult directly
                return new FileStreamResult(fileStream, contentType)
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                // Handle exception gracefully
                return StatusCode(500); // Internal Server Error
            }
        }




        [HttpGet("audio-stream")]
        public async Task<IActionResult> NewStreamAudio(string songId)
        {
            var song = sqliteHelper.GetSongById(songId).Result;
            string _totalPath = song.Path;
           // string _totalPath = _basePath + _filePath;
            var fileInfo = new FileInfo(song.Path);
            var rangeHeader = Request.Headers["Range"];

            if (rangeHeader.Count > 0)
            {
                var range = rangeHeader.ToString().Replace("bytes=", "").Split('-');
                long start = Convert.ToInt64(range[0]);
                long end = fileInfo.Length - 1;
                if (range.Length > 1 && !string.IsNullOrEmpty(range[1]))
                {
                    end = Convert.ToInt64(range[1]);
                }

                var length = end - start + 1;
                var fileStream = new FileStream(_totalPath, FileMode.Open);
                fileStream.Seek(start, SeekOrigin.Begin);

                Response.Headers.Add("Content-Length", length.ToString());
                Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileInfo.Length}");
                return File(fileStream, GetContentType(_totalPath));
            }
            else
            {
                var fileStream = new FileStream(_totalPath, FileMode.Open);
                var contentType = GetContentType(_totalPath);
                return File(fileStream, contentType);
            }
        }


        [HttpGet("audio")]
        public IActionResult StreamAudio()
        {
            string _totalPath = _basePath + _filePath;
            var fileStream = new FileStream(_totalPath, FileMode.Open);
            var contentType = GetContentType(_totalPath);
            return File(fileStream, contentType);
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path);
            switch (extension.ToLower())
            {
                case ".mp3":
                    return "audio/mpeg";
                case ".wav":
                    return "audio/wav";
                case ".flac":
                    return "audio/flac";
                default:
                    return "application.octet-stream";
            }
        }


    }
}
