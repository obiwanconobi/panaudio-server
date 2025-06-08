namespace PanAudioServer.Helper
{
    public class DatabaseHelper
    {

        //SqliteHelper sqliteHelper = new SqliteHelper();
        private SqliteHelper sqliteHelper;

        public DatabaseHelper(SqliteHelper sqliteHelper)
        {
            this.sqliteHelper = sqliteHelper;
        }

        public void clearAll()
        {
            sqliteHelper.Clear();
        }
    }
}
