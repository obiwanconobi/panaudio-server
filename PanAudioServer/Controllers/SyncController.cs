using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using System.Formats.Asn1;
using System.Reflection;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("sync")]
    public class SyncController : Controller
    {
        private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        private DatabaseHelper helper;
        private DirectoryHelper directoryHelper;
        public SyncController(DatabaseHelper helper, DirectoryHelper directoryHelper)
        {
            this.helper = helper;
            this.directoryHelper = directoryHelper;
        }
        
        [HttpGet("all")]
        public async void Sync()
        {
            //string _totalPath = @"\\192.168.1.15\ubuntu_media\nextcloud_2\conner\files\Music\";
          //  string _totalPath = Path.Combine(_basePath, "app", "Music");
            string _totalPath = _basePath + @"/Music/";
         //   DirectoryHelper dirHelper = new DirectoryHelper();

            await directoryHelper.directoryGetter(_totalPath);
            await directoryHelper.saveData();
         
        }

        [HttpGet("clear")]
        public async void Clear()
        {
           // DatabaseHelper helper = new DatabaseHelper();
            helper.clearAll();
        }

       

       
    }
}
