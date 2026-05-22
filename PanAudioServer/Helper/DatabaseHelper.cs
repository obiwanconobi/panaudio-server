namespace PanAudioServer.Helper
{
    public class DatabaseHelper
    {

        SqliteHelper sqliteHelper;

        public DatabaseHelper()
        {
            sqliteHelper = new SqliteHelper();
        }

        public DatabaseHelper(SqliteHelper sqliteHelper)
        {
            this.sqliteHelper = sqliteHelper;
        }


        public async Task clearAll()
        {
            await sqliteHelper.Clear();
        }
    }
}
