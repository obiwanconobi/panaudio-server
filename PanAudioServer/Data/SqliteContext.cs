using Microsoft.EntityFrameworkCore;
using PanAudioServer.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PanAudioServer.Data
{
    public class SqliteContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
         //   var test = Environment.GetEnvironmentVariable("SqliteDB");
            //  Console.WriteLine("DB location: " + test.ToString());
            //    var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            //  string curdir = Directory.GetCurrentDirectory();
            //    string relativePath = @"Data\FoxessWebbus.db";

            // string test = string.Format("Data Source={0};", relativePath);



            optionsBuilder.UseSqlite(@"Data Source=Data\\panaudio.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Songs>().HasKey(x => x.Id);
            modelBuilder.Entity<Album>().HasKey(x => x.Id);
            modelBuilder.Entity<Artists>().HasKey(x => x.Id);
        }

        //public DbSet<Employee> Employees{ get; set; }
        public DbSet<Songs> Songs { get; set; }
        public DbSet<Album> Album { get; set; }
        public DbSet<Artists> Artists { get; set; }
    }
}
