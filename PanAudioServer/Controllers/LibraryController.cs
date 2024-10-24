﻿using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api")]
    public class LibraryController : Controller
    {
        SqliteHelper sqliteHelper = new SqliteHelper();

        [HttpGet("artists")]
        public List<Artists> GetArtists()
        {
            return sqliteHelper.GetAllArtists();
        }

        [HttpGet("songs")]
        public List<Songs> GetSongs() 
        {
            return sqliteHelper.GetAllSongs();
        }

    }
}
