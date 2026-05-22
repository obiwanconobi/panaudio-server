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
        private DirectoryHelper dirHelper;
        private DatabaseHelper dbHelper;

        public SyncController(DirectoryHelper dirHelper, DatabaseHelper dbHelper)
        {
            this.dirHelper = dirHelper;
            this.dbHelper = dbHelper;
        }
      
        [HttpGet("all")]
        public async void Sync()
        {
            string _totalPath = _basePath + @"/Music/";

            await dirHelper.directoryGetter(_totalPath);
            await dirHelper.saveData();
         
        }

        [HttpGet("clear")]
        public async void Clear()
        {
            dbHelper.clearAll();
        }

       

       
    }
}
