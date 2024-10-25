namespace PanAudioServer.Helper
{
    public class DatabaseHelper
    {

        SqliteHelper sqliteHelper = new SqliteHelper();


        public void clearAll()
        {
            sqliteHelper.Clear();
        }
    }
}
