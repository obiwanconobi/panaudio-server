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

    public void SetPlaybackReportingTime(string time)
    {
        sqliteHelper.SetConfigValue("PlaybackReportingTime", time);
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
    
    public void SetArtistPictures(bool value)
    {
        sqliteHelper.SetConfigValue("ArtistPictures", value.ToString());
    }
    
}