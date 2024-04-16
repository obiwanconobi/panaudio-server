using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using System.Reflection;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("sync")]
    public class SyncController : Controller
    {
        private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      
        [HttpGet("all")]
        public void Sync()
        {
            string _totalPath = _basePath + @"\Media\";
            DirectoryHelper dirHelper = new DirectoryHelper();

            dirHelper.getDirectory(_totalPath);
            try
            {


                // Load the file
                var file = TagLib.File.Create(_totalPath);

                // Access metadata properties
                Console.WriteLine("Title: " + file.Tag.Title);
                Console.WriteLine("Artist: " + file.Tag.Performers[0]);
                Console.WriteLine("Album: " + file.Tag.Album);
                Console.WriteLine("Year: " + file.Tag.Year);
                Console.WriteLine("Duration: " + file.Properties.Duration.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

       

       
    }
}
