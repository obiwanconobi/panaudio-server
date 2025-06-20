﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PanAudioServer.Data;

#nullable disable

namespace PanAudioServer.Migrations
{
    [DbContext(typeof(SqliteContext))]
    [Migration("20250613134458_ConfigTable")]
    partial class ConfigTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("PanAudioServer.Models.Album", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("AlbumPath")
                        .HasColumnType("TEXT");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Favourite")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MusicBrainzId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Picture")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Album");
                });

            modelBuilder.Entity("PanAudioServer.Models.Artists", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ArtistPath")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Favourite")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MusicBrainzId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Picture")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("PanAudioServer.Models.Config", b =>
                {
                    b.Property<Guid>("ConfigId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConfigName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConfigValue")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ConfigId");

                    b.ToTable("Config");
                });

            modelBuilder.Entity("PanAudioServer.Models.PlaybackHistory", b =>
                {
                    b.Property<int>("PlaybackId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("PlaybackEnd")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("PlaybackStart")
                        .HasColumnType("TEXT");

                    b.Property<int>("Seconds")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SongId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("PlaybackId");

                    b.ToTable("PlaybackHistory");
                });

            modelBuilder.Entity("PanAudioServer.Models.PlaylistItems", b =>
                {
                    b.Property<string>("PlaylistItemId")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlaylistId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SongId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("PlaylistItemId");

                    b.HasIndex("PlaylistId");

                    b.HasIndex("SongId");

                    b.ToTable("PlaylistItems");
                });

            modelBuilder.Entity("PanAudioServer.Models.Playlists", b =>
                {
                    b.Property<string>("PlaylistId")
                        .HasColumnType("TEXT");

                    b.Property<string>("PlaylistName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("PlaylistId");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("PanAudioServer.Models.Songs", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Album")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AlbumId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AlbumPicture")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ArtistId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BitDepth")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BitRate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Codec")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DiscNumber")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("Favourite")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Length")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("MusicBrainzId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PlayCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SampleRate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("TrackNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("PanAudioServer.Models.PlaylistItems", b =>
                {
                    b.HasOne("PanAudioServer.Models.Playlists", "Playlist")
                        .WithMany("PlaylistItems")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PanAudioServer.Models.Songs", "Song")
                        .WithMany()
                        .HasForeignKey("SongId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Playlist");

                    b.Navigation("Song");
                });

            modelBuilder.Entity("PanAudioServer.Models.Playlists", b =>
                {
                    b.Navigation("PlaylistItems");
                });
#pragma warning restore 612, 618
        }
    }
}
