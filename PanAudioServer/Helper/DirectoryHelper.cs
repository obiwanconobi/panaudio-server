using PanAudioServer.Models;
using System.Reflection;
using ATL.AudioData;
using ATL;

namespace PanAudioServer.Helper
{
    public class DirectoryHelper
    {
       // private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
       SqliteHelper sqliteHelper  = new SqliteHelper();
        List<Songs> songs = new List<Songs>();
        List<Album> albums = new List<Album>();
        List<Artists> artists = new List<Artists>();

        List<Songs> dbSongs = new List<Songs>();
        List<Album> dbAlbums = new List<Album>();
        List<Artists> dbArtists = new List<Artists>();

        public DirectoryHelper()
        {
            dbSongs = sqliteHelper.GetAllSongs();
            dbArtists = sqliteHelper.GetAllArtists();
            dbAlbums = sqliteHelper.GetAllAblums();
        }

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
                await getDirectory(directory, 1);
            }
            catch (Exception e)
            {
               // await saveData();
            }
        }

        public async Task getDirectory(String directory, int depth)
        {
            string _totalPath = directory;

            var directories = getDirectories(_totalPath);

            if(directories == null)
            {
                //get songs
                 await getSongs(directory);

            }
            else
            {
                Console.WriteLine("Total number of directories witin: " + directories.Length + " within " + directory);

                foreach (var d in directories)
                {
                    // var directories = Directory.GetDirectories(d);
                    var dd = getDirectories(d);

                    
                        //get songs
                        await getSongs(d);
                    if(dd != null)
                    {
                        foreach (var dir in dd)
                        {
                            await getDirectory(dir, depth++);
                        }
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

        public async Task saveData()
        {
            try
            {
                Console.WriteLine("Trying To Save Artists count: " + artists.Count);
                await sqliteHelper.UploadArtists(artists);
                Console.WriteLine("Trying To Save Album count: " + albums.Count);
                await sqliteHelper.UploadAlbums(albums);
                Console.WriteLine("Trying To Save Songs count: " + songs.Count);
                await sqliteHelper.UploadSongs(songs);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR SAVING DATA: " + e.Message);
            }
           
        }


        public async Task getSongs(String directory)
        {
            var files = Directory.GetFiles(directory);
            String albumId = "";
            Console.WriteLine("Getting Files in directory: " + directory);

            foreach (var f in files)
            {
                try
                {

                    // Load the file

                    Track file = new Track(f);
                    
                    //var file = TagLib.File.Create(f);
                    if(file.AudioFormat.Name != "Unknown")
                    {
                        String songId = Guid.NewGuid().ToString();
                        string artistId = "";
                        string artistName = file.AlbumArtist;
                        if(artistName == "")
                        {
                            artistName = file.Artist;
                        }
                        var artist = dbArtists.Where(x => x.Name == artistName).FirstOrDefault() ?? artists.Where(x => x.Name == artistName).FirstOrDefault();

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


                        var album = dbAlbums.Where(x => x.Artist == artistName && x.Title == file.Album).FirstOrDefault() ?? albums.Where(x => x.Artist == artistName && x.Title == file.Album).FirstOrDefault();

                        if (album == null)
                        {

                            if(file.Album == "Gusher")
                            {
                                Console.WriteLine();
                            }
                            //sqliteHelper.UploadAlbum(new Album(id: albumId, title: file.Tag.Album, artist: artistName, picture: ""));
                            albumId = Guid.NewGuid().ToString();
                            albums.Add(new Album(id: albumId, title: file.Album, artist: artistName, picture: ""));
                            Console.WriteLine("Info: Inserted Album: " + file.Album);
                        }
                        else
                        {
                            albumId = album.Id;
                        }


                        if(file.Title == null)
                        {
                            continue;
                        }
                        
                    //    var song = await sqliteHelper.GetSong(artistName, file.Tag.Album, file.Tag.Title);
                           var song = dbSongs.Where(x => x.Artist == artistName && x.Title == file.Title && x.Album == file.Album).FirstOrDefault();

                        if (song == null)
                        {
                            var songAdd = new Songs
                             (
                                 id: songId,
                                 title: file.Title,
                                 trackNumber: Convert.ToInt32(file.TrackNumber),
                                 album: file.Album,
                                 albumId: albumId,
                                 artist: artistName,
                                 artistId: artistId,
                                 albumPicture: "",
                                 favourite: false,
                                 length: file.Duration.ToString(),
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
