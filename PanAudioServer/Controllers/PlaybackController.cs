using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api/playback")]
    public class PlaybackController : Controller
    {
        SqliteHelper sqliteHelper = new SqliteHelper();

        [HttpPut("start")]
        public async Task StartPlayback(string songId)
        {
            await sqliteHelper.StartRecordPlayback(songId);
        }

        [HttpGet("history")]
        public async Task<List<PlaybackCounts>> GetPlayback()
        {
           return await sqliteHelper.GetPlaybackHistory();
        }

        [HttpGet("historyByDate")]
        public async Task<List<PlaybackCounts>> GetPlaybackByDate(DateTime startDate, DateTime endDate)
        {
            return await sqliteHelper.GetPlaybackHistoryByDate(startDate, endDate);
        }

    }
}
