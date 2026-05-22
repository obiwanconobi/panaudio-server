using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;

namespace PanAudioServer.Controllers;

[ApiController]
[Route("api")]
public class ConfigController : Controller
{
    private ConfigHelper _configHelper;

    public ConfigController(ConfigHelper configHelper)
    {
        _configHelper = configHelper;
    }
    
    [HttpGet("getPlaybackTimeConfig")]
    public int GetPlaybackTime()
    {
        return _configHelper.GetPlaybackReportingTime();
    }
    
    [HttpPost("setPlaybackTimeConfig")]
    public async Task<IActionResult> SetPlaybackTime(int time)
    {
        await _configHelper.SetPlaybackReportingTime(time.ToString());
        return Ok();
    }
    
    [HttpGet("getArtistPictureConfig")]
    public bool GetArtistPictureConfig()
    {
        return _configHelper.GetArtistPictures();
    }
    
    [HttpPost("setArtistPictureConfig")]
    public async Task<IActionResult> SetArtistPictureConfig(bool value)
    {
        await _configHelper.SetArtistPictures(value);
        return Ok();
    }

}