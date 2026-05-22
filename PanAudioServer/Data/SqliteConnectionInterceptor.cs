using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace PanAudioServer.Data
{
    public class SqliteConnectionInterceptor : DbConnectionInterceptor
    {
        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA busy_timeout=5000;";
            cmd.ExecuteNonQuery();
        }
    }
}
