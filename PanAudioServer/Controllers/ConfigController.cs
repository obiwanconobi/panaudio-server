using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;

namespace PanAudioServer.Controllers;

[ApiController]
[Route("api")]
public class ConfigController : Controller
{
    private ConfigHelper _configHelper;

    public ConfigController()
    {
        _configHelper = new ConfigHelper();
    }
    
    [HttpGet("getPlaybackTimeConfig")]
    public int GetPlaybackTime()
    {
        return _configHelper.GetPlaybackReportingTime();
    }
    
    [HttpPost("setPlaybackTimeConfig")]
    public void SetPlaybackTime(int time)
    {
        _configHelper.SetPlaybackReportingTime(time.ToString());
    }

}