namespace PanAudioServer.Helper;

public class ConfigHelper
{
    
    SqliteHelper sqliteHelper = new SqliteHelper();


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
    
}