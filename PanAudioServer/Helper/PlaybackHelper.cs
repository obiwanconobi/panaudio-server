namespace PanAudioServer.Helper;

public class PlaybackHelper
{
    private SqliteHelper sqliteHelper;

    public PlaybackHelper()
    {
        sqliteHelper = new SqliteHelper();
    }

    public PlaybackHelper(SqliteHelper sqliteHelper)
    {
        this.sqliteHelper = sqliteHelper;
    }
    
}