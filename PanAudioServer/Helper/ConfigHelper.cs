namespace PanAudioServer.Helper;

public class ConfigHelper
{
    private SqliteHelper sqliteHelper;

    public ConfigHelper()
    {
        sqliteHelper = new SqliteHelper();
    }

    public ConfigHelper(SqliteHelper sqliteHelper)
    {
        this.sqliteHelper = sqliteHelper;
    }
    
    public async Task<int> GetPlaybackReportingTime()
    {
        var value = await sqliteHelper.GetConfigValue("PlaybackReportingTime");
        if (value == null)
        {
            return 5;
        }
        return Convert.ToInt16(value);
    }

    public async Task SetPlaybackReportingTime(string time)
    {
        await sqliteHelper.SetConfigValue("PlaybackReportingTime", time);
    }
    
    public async Task<bool> GetArtistPictures()
    {
        var value = await sqliteHelper.GetConfigValue("ArtistPictures");
        if(value == null)
        {
            return false;
        }
        return Convert.ToBoolean(value);
    }
    
    public async Task SetArtistPictures(bool value)
    {
        await sqliteHelper.SetConfigValue("ArtistPictures", value.ToString());
    }
    
    public async Task<bool> GetEnableTagWriting()
    {
        var value = await sqliteHelper.GetConfigValue("EnableTagWriting");
        if (value == null)
        {
            return false;
        }
        return Convert.ToBoolean(value);
    }

    public async Task SetEnableTagWriting(bool value)
    {
        await sqliteHelper.SetConfigValue("EnableTagWriting", value.ToString());
    }

    public async Task<string> GetListenBrainzToken()
    {
        var value = await sqliteHelper.GetConfigValue("ListenBrainzToken");
        return value ?? "";
    }

    public async Task SetListenBrainzToken(string token)
    {
        await sqliteHelper.SetConfigValue("ListenBrainzToken", token);
    }
    
}