using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace PanAudioServer.Data
{
    public class SqliteContextFactory : IDesignTimeDbContextFactory<SqliteContext>
    {
        public SqliteContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<SqliteContext> optionsBuilder = new DbContextOptionsBuilder<SqliteContext>();

         //   var test = Environment.GetEnvironmentVariable("panaudio");
         //   Console.WriteLine("DB location: " + test.ToString());

            //  string test = @"Data\FoxessWebbus.db";
            // string connectionString = string.Format("Data Source={0};", test);
            // optionsBuilder.UseSqlite(connectionString);

            optionsBuilder.UseSqlite(@"Data Source = Data\\panaudio.db");
            return new SqliteContext();
        }
    }
}
