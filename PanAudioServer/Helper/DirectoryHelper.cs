using PanAudioServer.Models;
using System.Reflection;

namespace PanAudioServer.Helper
{
    public class DirectoryHelper
    {
       // private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
       SqliteHelper sqliteHelper  = new SqliteHelper();

        public void getDirectory(String directory, int depth = 0)
        {
            string _totalPath = directory;

            var directories = Directory.GetDirectories(_totalPath);

            int count = 0;

            foreach (var d in directories)
            {
                // var directories = Directory.GetDirectories(d);
                var dd = Directory.GetDirectories(d);

                if (depth == 0)
                {
                   
                }

                if (dd.Length > 1)
                {
                    getDirectory(dd[count], 1);
                }
                else
                {
                    getSongs(d); //maybe need to do a dd[count]??
                }

                Console.WriteLine();
            }
        }


        public async Task<Artists> getArtist(string artist) 
        {
            return await sqliteHelper.GetArtist(artist);
        }


        public async void getSongs(String directory)
        {
            var files = Directory.GetFiles(directory);
            String albumId = Guid.NewGuid().ToString();
            foreach (var f in files)
            {
                try
                {


                    // Load the file
                    var file = TagLib.File.Create(f);
                    String songId = Guid.NewGuid().ToString();
                    string artistId = "";
                    var artist = await sqliteHelper.GetArtist(file.Tag.AlbumArtists[0]);

                    if(artist == null )
                    {
                        artistId = Guid.NewGuid().ToString();
                        //set artistId,
                        sqliteHelper.UploadArtist(new Artists(id: artistId, name: file.Tag.AlbumArtists[0], picture: ""));
                        
                    }
                    else
                    {
                        artistId = artist.Id;
                    }


                    var album = await sqliteHelper.GetAlbum(file.Tag.AlbumArtists[0], file.Tag.Album);
                    
                    if(album == null)
                    {
                        sqliteHelper.UploadAlbum(new Album(id: albumId, title: file.Tag.Album, artist: file.Tag.AlbumArtists[0], picture: ""));
                    }


                    var song = await sqliteHelper.GetSong(file.Tag.AlbumArtists[0], file.Tag.Album, file.Tag.Title);
                    if (song == null)
                    {
                         var songAdd = new Songs
                          (
                              id: songId,
                              title: file.Tag.Title,
                              trackNumber: Convert.ToInt32(file.Tag.Track),
                              album: file.Tag.Album,
                              albumId: albumId,
                              artist: file.Tag.AlbumArtists[0],
                              artistId: artistId,
                              albumPicture: "",
                              favourite: false,
                              length: "",
                              path: f.ToString()


                          );

                        sqliteHelper.UploadSong(songAdd);
                    }

                  

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}
