using PanAudioServer.Models;
using System.Reflection;

namespace PanAudioServer.Helper
{
    public class DirectoryHelper
    {
       // private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
       SqliteHelper sqliteHelper  = new SqliteHelper();
        List<Songs> songs = new List<Songs>();
        List<Album> albums = new List<Album>();
        List<Artists> artists = new List<Artists>();

        public string[]? getDirectories(String path)
        {
            var directories = Directory.GetDirectories(path);
            if(directories.Length > 0)
            {
                return directories;
            }

            return null;
        }


        public async Task directoryGetter(String directory)
        {
            try
            {
                await getDirectory(directory);
            }
            catch (Exception e)
            {
                saveData();
            }
        }

        public async Task getDirectory(String directory, int depth = 0)
        {
            string _totalPath = directory;

            var directories = getDirectories(_totalPath);

            if(directories == null)
            {
                //get songs
                getSongs(directory);

            }
            else
            {
                Console.WriteLine("Total number of directories witin: " + directories.Length + " within " + directory);
                int count = 0;

                foreach (var d in directories)
                {
                    // var directories = Directory.GetDirectories(d);
                    var dd = getDirectories(d);

                    if (dd == null)
                    {
                        //get songs
                        getSongs(d);
                        continue;
                    }

                    if (dd.Length > 0)
                    {
                        await getDirectory(dd[count], 1);
                    }

                    Console.WriteLine();
                }


            }
            
        }

        //remove
        public async Task<Artists> getArtist(string artist) 
        {
            return await sqliteHelper.GetArtist(artist);
        }

        public void saveData()
        {
            try
            {
                Console.WriteLine("Trying To Save Artists count: " + artists.Count);
                sqliteHelper.UploadArtists(artists);
                Console.WriteLine("Trying To Save Album count: " + albums.Count);
                sqliteHelper.UploadAlbums(albums);
                Console.WriteLine("Trying To Save Songs count: " + songs.Count);
                sqliteHelper.UploadSongs(songs);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR SAVING DATA: " + e.Message);
            }
           
        }


        public async void getSongs(String directory)
        {
            var files = Directory.GetFiles(directory);
            String albumId = Guid.NewGuid().ToString();

            Console.WriteLine("Getting Files in directory: " + directory);

            foreach (var f in files)
            {
                try
                {
                    
                    // Load the file
                    var file = TagLib.File.Create(f);
                    if(file.Properties.MediaTypes == TagLib.MediaTypes.Audio)
                    {
                        String songId = Guid.NewGuid().ToString();
                        string artistId = "";
                        string artistName = file.Tag.FirstAlbumArtist ?? file.Tag.FirstPerformer;
                        var artist = await sqliteHelper.GetArtist(artistName) ?? artists.Where(x => x.Name == artistName).FirstOrDefault();

                        if (artist == null)
                        {
                            artistId = Guid.NewGuid().ToString();
                            //set artistId,
                           // sqliteHelper.UploadArtist(new Artists(id: artistId, name: artistName, picture: ""));
                            artists.Add(new Artists(id: artistId, name: artistName, picture: ""));
                            Console.WriteLine("Info: Inserted Artist: " + artistName);
                        }
                        else
                        {
                            artistId = artist.Id;
                           // Console.WriteLine("Info: Artist already existed: " + artistName);
                        }


                        var album = await sqliteHelper.GetAlbum(artistName, file.Tag.Album) ?? albums.Where(x => x.Artist == artistName && x.Title == file.Tag.Album).FirstOrDefault();

                        if (album == null)
                        {
                            //sqliteHelper.UploadAlbum(new Album(id: albumId, title: file.Tag.Album, artist: artistName, picture: ""));
                            albums.Add(new Album(id: albumId, title: file.Tag.Album, artist: artistName, picture: ""));
                            Console.WriteLine("Info: Inserted Album: " + file.Tag.Album);
                        }


                        if(file.Tag.Title == null)
                        {
                            continue;
                        }
                        
                        var song = await sqliteHelper.GetSong(artistName, file.Tag.Album, file.Tag.Title);

                        
                        if (song == null)
                        {
                            var songAdd = new Songs
                             (
                                 id: songId,
                                 title: file.Tag.Title,
                                 trackNumber: Convert.ToInt32(file.Tag.Track),
                                 album: file.Tag.Album,
                                 albumId: albumId,
                                 artist: artistName,
                                 artistId: artistId,
                                 albumPicture: "",
                                 favourite: false,
                                 length: file.Properties.Duration.TotalSeconds.ToString(),
                                 path: f.ToString()


                             );
                            songs.Add(songAdd);
                            //sqliteHelper.UploadSong(songAdd);
                            Console.WriteLine("Info: Inserted Song:" + songAdd.Title + " : " + songAdd.Artist );
                        }
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
