using ATL.Playlist;
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
            String test = Environment.GetEnvironmentVariable("SqliteDB");


            optionsBuilder.UseSqlite(test);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Songs>().HasKey(x => x.Id);
            modelBuilder.Entity<Album>().HasKey(x => x.Id);
            modelBuilder.Entity<Artists>().HasKey(x => x.Id);
            modelBuilder.Entity<Playlists>().HasKey(x => x.PlaylistId);
            modelBuilder.Entity<PlaylistItems>().HasKey(x => x.PlaylistItemId);
            modelBuilder.Entity<PlaybackHistory>().HasKey(x => x.PlaybackId);
            modelBuilder.Entity<Config>().HasKey(x => x.ConfigId);
            modelBuilder.Entity<Playlists>(entity =>
            {
                entity.HasKey(e => e.PlaylistId);

                entity.HasMany(e => e.PlaylistItems)
                      .WithOne(e => e.Playlist)
                      .HasForeignKey(e => e.PlaylistId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlaylistItems>(entity =>
            {
                entity.HasKey(e => e.PlaylistItemId);
            });


        }

        public DbSet<Songs> Songs { get; set; }
        public DbSet<Album> Album { get; set; }
        public DbSet<Artists> Artists { get; set; }
        public DbSet<Playlists> Playlists { get; set; }
        public DbSet<PlaylistItems> PlaylistItems { get; set; }
        public DbSet<PlaybackHistory> PlaybackHistory { get; set; }
        public DbSet<Config> Config { get; set; }
    }
}
