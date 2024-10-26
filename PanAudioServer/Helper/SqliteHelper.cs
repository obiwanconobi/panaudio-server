﻿using Microsoft.EntityFrameworkCore;
using PanAudioServer.Data;
using PanAudioServer.Models;
using System.Diagnostics.Contracts;

namespace PanAudioServer.Helper
{
    public class SqliteHelper
    {
        private SqliteContext? _context;



        public async Task<Album> GetAlbum(string artist, string album)
        {
            _context = new SqliteContext();
            return _context.Album.FirstOrDefault(x => x.Title == album && x.Artist == artist);
        }

        public async void Clear()
        {
            _context = new SqliteContext();
            _context.Album.ExecuteDelete();
            _context.Artists.ExecuteDelete();
            _context.Songs.ExecuteDelete();
        }


        public async Task UploadAlbums(List<Album> album)
        {
            _context = new SqliteContext();
            using (var context = new SqliteContext())
            {
                try
                {
                    _context.Album.AddRange(album);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public async void UploadAlbum(Album album)
        {
            _context = new SqliteContext();
            using(var context  = new SqliteContext())
            {
                try
                {
                    _context.Album.Add(album);
                    await _context.SaveChangesAsync();
                }catch(Exception ex)
                {

                }
            }
        }

        public List<Album> GetAllAblums()
        {
            _context = new SqliteContext();
            return _context.Album.OrderBy(x => x.Title).ToList();
        }

        public List<Songs> GetAllSongs()
        {
            _context = new SqliteContext();
            return _context.Songs.OrderBy(x => x.Title).ToList();
        }

        //public Songs GetSongById(String songId)
        //{
        //    _context = new SqliteContext();
        //    return _context.Songs.OrderBy(x => x.Id == songId).First();
        //}

        public List<Artists> GetAllArtists()
        {
            _context =  new SqliteContext();
            return _context.Artists.OrderBy(x => x.Name).ToList();
        }


        public Artists GetArtist(string artist)
        {
            _context = new SqliteContext();
            return _context.Artists.First(x => x.Name == artist);
        }


        public Songs GetSongById(string songId)
        {
            _context = new SqliteContext();
            return _context.Songs.First(x => x.Id == songId);
        }

        public Songs GetSong(string artist, string album, string title)
        {
            _context = new SqliteContext();
            return _context.Songs.First(x => x.Artist == artist && x.Album == album  && x.Title == title);
        }

        public async Task UploadArtists(List<Artists> artists)
        {
            _context = new SqliteContext();
            using (var context = new SqliteContext())
            {
                try
                {
                    context.Artists.AddRange(artists);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        public async void UploadArtist(Artists artists)
        {
            _context = new SqliteContext();
            using(var context = new SqliteContext())
            {
                try
                {
                    context.Artists.Add(artists);
                    await context.SaveChangesAsync();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        public async Task UploadSongs(List<Songs> song)
        {
            _context ??= new SqliteContext();
            using (var context = new SqliteContext())
            {

                try
                {
                    context.Songs.AddRange(song);
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                }

            }

        }

        public async void UploadSong(Songs song)
        {
            _context ??= new SqliteContext();
            using (var context = new SqliteContext())
            {

                try
                {
                    context.Songs.Add(song);
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                   
                    Console.WriteLine(ex.ToString());
                }

            }

        }

    }
}
