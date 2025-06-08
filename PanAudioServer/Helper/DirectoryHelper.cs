using PanAudioServer.Models;
using System.Reflection;
using ATL.AudioData;
using ATL;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Components.Forms;

namespace PanAudioServer.Helper
{
    public class DirectoryHelper
    {
       // private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private SqliteHelper sqliteHelper;
        List<Songs> songs = new List<Songs>();
        List<Album> albums = new List<Album>();
        List<Artists> artists = new List<Artists>();

        List<Songs> dbSongs = new List<Songs>();
        List<Album> dbAlbums = new List<Album>();
        List<Artists> dbArtists = new List<Artists>();
        private MusicBrainzHelper musicBrainzHelper;

        public DirectoryHelper(SqliteHelper sqliteHelper, MusicBrainzHelper musicBrainzHelper)
        {
            this.sqliteHelper = sqliteHelper;
            this.musicBrainzHelper = musicBrainzHelper;
            dbSongs = sqliteHelper.GetAllSongs();
            dbArtists = sqliteHelper.GetAllArtists();
            dbAlbums = sqliteHelper.GetAllAblums();
        }

        public string[]? getDirectories(String path)
        {
            try
            {
                var directories = Directory.GetDirectories(path);
                Console.WriteLine("Getting current Path: " + path);
              
                if (directories.Length > 0)
                {
                    return directories;
                }

                return null;
            }catch(Exception e)
            {
                Console.WriteLine("Error Getting Directory. Probably Invalid Character");
                return null;
            }
          
        }


        public string removeShittyCharacters(string input)
        {
            if (input.Contains("blink"))
            {
                Console.WriteLine();
            }

            input.Replace('‐', '-');
            
            char HyphenMinus = '\u002D';        // Regular hyphen-minus: -
             char HyphenFigureDash = '\u2012';   // Figure dash: ‒
             char HyphenEnDash = '\u2013';       // En dash: –
             char HyphenEmDash = '\u2014';       // Em dash: —
             char HyphenNonBreakingHyphen = '\u2011'; // Non-breaking hyphen: ‑

            if (input.Contains(HyphenNonBreakingHyphen))
            {
                Console.WriteLine();
            }

            input.Replace("’", "'");

            input.Replace(HyphenFigureDash, HyphenMinus);
            input.Replace(HyphenEnDash, HyphenMinus);
            input.Replace(HyphenEmDash, HyphenMinus);
            input.Replace(HyphenNonBreakingHyphen, HyphenMinus);

            input.Replace("-", "-");

           
           // input.Replace('‐', '-');
            input.Replace("`", "'");
            return input;
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
            return sqliteHelper.GetArtist(artist);
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

        private bool IsImage(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return true;
                case ".png":
                    return true;
                case ".gif":
                    return true;
                case ".bmp":
                    return true;
                case ".webp":
                    return true;
                default:
                    return false;
            }
        }

        public string returnLikelyArtistImage(string path, string artistName)
        {

            List<string> imagesInArtistFolder = new List<string>();
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (IsImage(Path.GetExtension(file)))
                {
                    imagesInArtistFolder.Add(file);
                }
            }

            if (imagesInArtistFolder.Any(x => x.Contains(artistName, StringComparison.OrdinalIgnoreCase)))
                return imagesInArtistFolder.First(x => x.Contains(artistName, StringComparison.OrdinalIgnoreCase));

            if (imagesInArtistFolder.Any(x => x.Contains("fanart", StringComparison.OrdinalIgnoreCase)))
                return imagesInArtistFolder.First(x => x.Contains("fanart", StringComparison.OrdinalIgnoreCase));

            return null;
        }

        public string returnLikelyImage(List<string> imagesInFolder)
        {

            if (imagesInFolder.Any(x => x.Contains("cover", StringComparison.OrdinalIgnoreCase)))
                return imagesInFolder.First(x => x.Contains("cover", StringComparison.OrdinalIgnoreCase));

            if (imagesInFolder.Any(x => x.Contains("album", StringComparison.OrdinalIgnoreCase)))
                return imagesInFolder.First(x => x.Contains("album", StringComparison.OrdinalIgnoreCase));

            if (imagesInFolder.Any(x => x.Contains("folder", StringComparison.OrdinalIgnoreCase)))
                return imagesInFolder.First(x => x.Contains("folder", StringComparison.OrdinalIgnoreCase));

            return null;
        }


        private static readonly Dictionary<string, int> ExtensionPriority = new()
    {
        {".jpg", 1},
        {".png", 2},
        {".webp", 3},
        {".gif", 3},
        {".flac", 4},
        {".wav", 4},
        {".aac", 5},
        {".mp3", 5},
        {".lrc", 7},
        {".xml", 7},
        {".nfo", 7},
        {".cue", 7},

        // Add more extensions as needed
         };

        private static int GetExtensionPriority(string extension)
        {
            extension = extension.ToLowerInvariant();
            return ExtensionPriority.TryGetValue(extension, out int priority)
                ? priority
                : int.MaxValue; // Unspecified extensions go to the end
        }


        public async Task getSongs(String directory)
        {
            var files = Directory.GetFiles(directory).OrderBy(f => GetExtensionPriority(Path.GetExtension(f))).ToList();
            // files.OrderBy(f => GetExtensionPriority(Path.GetExtension(f)));
            String albumId = "";
            Console.WriteLine("Getting Files in directory: " + directory);
            List<string> imagesInFolder = new List<string>();
            foreach (var f in files)
            {
                try
                {
                    var pathExtension = Path.GetExtension(f);
                    
                    // this needs changing
                    if (f.EndsWith(".xml") || f.EndsWith(".lrc"))
                    {
                        continue;
                    }

                    if (IsImage(pathExtension)){
                        imagesInFolder.Add(f);
                        continue;
                    }
                    

                    Track file = new Track(f);
                    
                    //var file = TagLib.File.Create(f);
                    if(file.AudioFormat.Name != "Unknown")
                    {
                        if (file.Album == "California")
                        {

                        }


                        String songId = Guid.NewGuid().ToString();
                        string artistId = "";
                        string artistName = removeShittyCharacters(file.AlbumArtist);
                        if(artistName == "")
                        {
                            artistName = file.Artist;
                        }
                        var artist = dbArtists.Where(x => x.Name.ToLower() == artistName.ToLower()).FirstOrDefault() ?? artists.Where(x => x.Name.ToLower() == artistName.ToLower()).FirstOrDefault();

                        if (artist == null)
                        {
                            artistId = Guid.NewGuid().ToString();
                            var artistDir = Directory.GetParent(directory);
                            var musicBrainzArtistId = "";
                            try
                            {
                                musicBrainzArtistId = await musicBrainzHelper.getArtistIdAsync(artistName);
                            }
                            catch (Exception) { }

                            
                            //set artistId,
                            // sqliteHelper.UploadArtist(new Artists(id: artistId, name: artistName, picture: ""));
                            artists.Add(new Artists(id: artistId, name: removeShittyCharacters(artistName),artistPath: artistDir!.FullName, picture: Path.GetFileName(returnLikelyArtistImage(artistDir!.FullName, artistName)), favourite: false, musicBrainzId: musicBrainzArtistId));
                            Console.WriteLine("Info: Inserted Artist: " + artistName);
                        }
                        else
                        {
                            artistId = artist.Id;
                           // Console.WriteLine("Info: Artist already existed: " + artistName);
                        }


                        var album = dbAlbums.Where(x => x.Artist.ToLower() == artistName.ToLower() && x.Title == file.Album).FirstOrDefault() ?? albums.Where(x => x.Artist.ToLower() == artistName.ToLower() && x.Title == file.Album).FirstOrDefault();
                        
                        if (album == null)
                        {

                            //sqliteHelper.UploadAlbum(new Album(id: albumId, title: file.Tag.Album, artist: artistName, picture: ""));
                            albumId = Guid.NewGuid().ToString();
                            albums.Add(new Album(id: albumId, title:removeShittyCharacters(file.Album), artist: artistName, picture: Path.GetFileName(returnLikelyImage(imagesInFolder)), albumPath: directory, year: file.Year ?? null, favourite: false));
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
                           var song = dbSongs.Where(x => x.Artist.ToLower() == artistName.ToLower() && x.Title == file.Title && x.Album == file.Album).FirstOrDefault();

                        if (song == null)
                        {
                            var songAdd = new Songs()
                            {
                                Id = songId,
                                Title = removeShittyCharacters(file.Title),
                                TrackNumber = Convert.ToInt32(file.TrackNumber),
                                Album = file.Album,
                                AlbumId = albumId,
                                Artist = artistName,    
                                ArtistId = artistId,
                                AlbumPicture = "",
                                DiscNumber =  file.DiscNumber ?? 1,
                                Favourite = false,
                                Length = file.Duration.ToString(),
                                Codec = file.AudioFormat.ShortName,
                                BitRate = file.Bitrate.ToString(),
                                BitDepth = file.BitDepth.ToString(),
                                SampleRate = file.SampleRate.ToString(),
                                Path = f.ToString()


                            };
                            songs.Add(songAdd);
                            //sqliteHelper.UploadSong(songAdd);
                            Console.WriteLine("Info: Inserted Song:" + songAdd.Title + " : " + songAdd.Artist );
                        }
                        else
                        {
                            song.DiscNumber = file.DiscNumber ?? 1;
                            await sqliteHelper.UpdateSong(song);
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
