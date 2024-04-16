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


        public async Task<Artists> GetArtist(string artist)
        {
            _context = new SqliteContext();
            return _context.Artists.FirstOrDefault(x => x.Name == artist);
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
