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
    public async Task<int> GetPlaybackTime()
    {
        return await _configHelper.GetPlaybackReportingTime();
    }
    
    [HttpPost("setPlaybackTimeConfig")]
    public async Task<IActionResult> SetPlaybackTime(int time)
    {
        await _configHelper.SetPlaybackReportingTime(time.ToString());
        return Ok();
    }
    
    [HttpGet("getArtistPictureConfig")]
    public async Task<bool> GetArtistPictureConfig()
    {
        return await _configHelper.GetArtistPictures();
    }
    
    [HttpPost("setArtistPictureConfig")]
    public async Task<IActionResult> SetArtistPictureConfig(bool value)
    {
        await _configHelper.SetArtistPictures(value);
        return Ok();
    }

    [HttpGet("getTagWritingConfig")]
    public async Task<bool> GetTagWritingConfig()
    {
        return await _configHelper.GetEnableTagWriting();
    }

    [HttpPost("setTagWritingConfig")]
    public async Task<IActionResult> SetTagWritingConfig(bool value)
    {
        await _configHelper.SetEnableTagWriting(value);
        return Ok();
    }

}