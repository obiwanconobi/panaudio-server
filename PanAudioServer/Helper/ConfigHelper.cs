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
    
    public int GetPlaybackReportingTime()
    {
        var value = sqliteHelper.GetConfigValue("PlaybackReportingTime");
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
    
    public bool GetArtistPictures()
    {
        var value = sqliteHelper.GetConfigValue("ArtistPictures");
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
    
}