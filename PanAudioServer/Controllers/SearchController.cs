using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : Controller
{
    private SqliteHelper sqliteHelper;

    public SearchController(SqliteHelper sqliteHelper)
    {
        this.sqliteHelper = sqliteHelper;
    }

    [HttpGet("songs")]
    public async Task<List<Songs>> SearchSongs(string query)
    {
        return await sqliteHelper.SearchSongs(query);
    }

    [HttpGet("albums")]
    public async Task<List<Album>> SearchAlbums(string query)
    {
        return await sqliteHelper.SearchAlbums(query);
    }

    [HttpGet("artists")]
    public async Task<List<Artists>> SearchArtists(string query)
    {
        return await sqliteHelper.SearchArtists(query);
    }
}
