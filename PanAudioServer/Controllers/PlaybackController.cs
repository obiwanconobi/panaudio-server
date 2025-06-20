﻿using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api/playback")]
    public class PlaybackController : Controller
    {
        SqliteHelper sqliteHelper = new SqliteHelper();
        ConfigHelper configHelper = new ConfigHelper();

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

        [HttpGet("playbackday")]
        public async Task<List<PlaybackHistory>> GetPlaybackDay(DateOnly day)
        {
            return await sqliteHelper.GetPlaybackHistoryForDay(day);
        }

        [HttpGet("historyByDate")]
        public async Task<List<PlaybackCounts>> GetPlaybackByDate(DateTime startDate, DateTime endDate)
        {
            var playbackTime = configHelper.GetPlaybackReportingTime();
            return await sqliteHelper.GetPlaybackHistoryByDate(startDate, endDate, playbackTime);
        }

        [HttpGet("historyartists")]
        public async Task<List<PlaybackArtists>> GetPlaybackArtists(DateTime startDate, DateTime endDate)
        {
            return await sqliteHelper.GetPlaybackHistoryArtists(startDate, endDate);
        }

        [HttpGet("historydays")]
        public async Task<List<PlaybackDays>> GetPlaybackArtists(DateOnly startDate, DateOnly endDate)
        {
            return await sqliteHelper.GetPlaybackByDays(startDate, endDate);
        }

    }
}
