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
        // Process-wide lock so only one library scan runs at a time. DirectoryHelper
        // is Scoped (one instance per request), so without this two concurrent
        // /sync/all requests each walk the tree with their own in-memory dedup state
        // and insert the full library twice.
        private static readonly SemaphoreSlim _scanLock = new SemaphoreSlim(1, 1);

        private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private DirectoryHelper dirHelper;
        private DatabaseHelper dbHelper;

        public SyncController(DirectoryHelper dirHelper, DatabaseHelper dbHelper)
        {
            this.dirHelper = dirHelper;
            this.dbHelper = dbHelper;
        }
      
        [HttpGet("all")]
        public async Task<IActionResult> Sync()
        {
            if (!await _scanLock.WaitAsync(TimeSpan.Zero))
            {
                return Conflict("A library scan is already in progress.");
            }

            try
            {
                string _totalPath = _basePath + @"/Music/";

                await dirHelper.directoryGetter(_totalPath);
                await dirHelper.saveData();
                return Ok();
            }
            finally
            {
                _scanLock.Release();
            }
        }

        [HttpGet("clear")]
        public async Task<IActionResult> Clear()
        {
            await dbHelper.clearAll();
            return Ok();
        }

       

       
    }
}
