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
        SqliteHelper sqliteHelper;
        List<Songs> songs = new List<Songs>();
        List<Album> albums = new List<Album>();
        List<Artists> artists = new List<Artists>();

        List<Songs> dbSongs = new List<Songs>();
        List<Album> dbAlbums = new List<Album>();
        List<Artists> dbArtists = new List<Artists>();
        MusicBrainzHelper musicBrainzHelper;
        private bool _dataLoaded = false;

        public DirectoryHelper()
        {
            sqliteHelper = new SqliteHelper();
            musicBrainzHelper = new MusicBrainzHelper();
        }

        public DirectoryHelper(SqliteHelper sqliteHelper)
        {
            this.sqliteHelper = sqliteHelper;
            musicBrainzHelper = new MusicBrainzHelper();
        }

        public DirectoryHelper(SqliteHelper sqliteHelper, MusicBrainzHelper musicBrainzHelper)
        {
            this.sqliteHelper = sqliteHelper;
            this.musicBrainzHelper = musicBrainzHelper;
        }

        private async Task EnsureDataLoadedAsync()
        {
            if (_dataLoaded) return;
            dbSongs = await sqliteHelper.GetAllSongs();
            dbArtists = await sqliteHelper.GetAllArtists();
            dbAlbums = await sqliteHelper.GetAllAblums();
            _dataLoaded = true;
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
             char HyphenMinus = '\u002D';        // Regular hyphen-minus: -
             char HyphenFigureDash = '\u2012';   // Figure dash: -
             char HyphenEnDash = '\u2013';       // En dash: -
             char HyphenEmDash = '\u2014';       // Em dash: -
             char HyphenNonBreakingHyphen = '\u2011'; // Non-breaking hyphen: -

            input = input.Replace('\u2010', '-');
            input = input.Replace("\u2019", "'");
            input = input.Replace(HyphenFigureDash, HyphenMinus);
            input = input.Replace(HyphenEnDash, HyphenMinus);
            input = input.Replace(HyphenEmDash, HyphenMinus);
            input = input.Replace(HyphenNonBreakingHyphen, HyphenMinus);
            input = input.Replace("`", "'");
            return input;
        }

        // ATL can't read the FLAC stream info (duration/bitrate/bit depth/sample rate)
        // when a FLAC file has a non-standard prepended ID3 tag. Fall back to parsing the
        // FLAC STREAMINFO block directly in that case.
        private static (int durationSeconds, int bitrateKbps, int bitDepth, int sampleRate)? ReadFlacStreamInfo(string filePath)
        {
            try
            {
                using var fs = File.OpenRead(filePath);
                var buffer = new byte[Math.Min(fs.Length, 1_000_000)];
                int read = fs.Read(buffer, 0, buffer.Length);

                // Locate the "fLaC" marker (it may be preceded by a stray ID3 tag).
                int flacIdx = -1;
                for (int i = 0; i + 4 <= read; i++)
                {
                    if (buffer[i] == 0x66 && buffer[i + 1] == 0x4C && buffer[i + 2] == 0x61 && buffer[i + 3] == 0x43)
                    {
                        flacIdx = i;
                        break;
                    }
                }

                if (flacIdx < 0)
                {
                    return null;
                }

                int hdr = flacIdx + 4;
                if (hdr + 4 + 34 > read || (buffer[hdr] & 0x7F) != 0)
                {
                    return null;
                }

                int data = hdr + 4;
                ulong combined = 0;
                for (int i = 0; i < 8; i++)
                {
                    combined = (combined << 8) | buffer[data + 10 + i];
                }

                int sampleRate = (int)((combined >> 44) & 0xFFFFF);
                int bitDepth = (int)((combined >> 36) & 0x1F) + 1;
                ulong totalSamples = combined & 0xFFFFFFFFFUL;

                if (sampleRate == 0)
                {
                    return null;
                }

                int durationSeconds = (int)(totalSamples / (ulong)sampleRate);
                int bitrateKbps = (int)(fs.Length * 8L / (totalSamples / (double)sampleRate) / 1000.0);

                return (durationSeconds, bitrateKbps, bitDepth, sampleRate);
            }
            catch
            {
                return null;
            }
        }

        public async Task directoryGetter(String directory)
        {
            try
            {
                await getDirectory(directory);
            }
            catch (Exception e)
            {
               // await saveData();
            }
        }

        public async Task getDirectory(String directory)
        {
            // Scan the files in this folder first: an album folder may contain its
            // own audio files alongside subfolders (e.g. "Disk 1" / "Disk 2").
            await getSongs(directory);

            var directories = getDirectories(directory);
            if (directories == null)
            {
                return;
            }

            foreach (var d in directories)
            {
                await getDirectory(d);
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

        public bool IsImage(string fileExtension)
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
            await EnsureDataLoadedAsync();
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
                            artistName = removeShittyCharacters(file.Artist ?? "");
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


                        var albumTitle = removeShittyCharacters(file.Album ?? "");
                        var album = dbAlbums.Where(x => x.Artist.ToLower() == artistName.ToLower() && x.Title.ToLower() == albumTitle.ToLower()).FirstOrDefault() ?? albums.Where(x => x.Artist.ToLower() == artistName.ToLower() && x.Title.ToLower() == albumTitle.ToLower()).FirstOrDefault();
                        
                        if (album == null)
                        {

                            //sqliteHelper.UploadAlbum(new Album(id: albumId, title: file.Tag.Album, artist: artistName, picture: ""));
                            albumId = Guid.NewGuid().ToString();
                            albums.Add(new Album(id: albumId, title:albumTitle, artist: artistName, picture: Path.GetFileName(returnLikelyImage(imagesInFolder)), albumPath: directory, year: file.Year ?? null, favourite: false));
                            Console.WriteLine("Info: Inserted Album: " + albumTitle);
                        }
                        else
                        {
                            albumId = album.Id;
                        }


                        if(file.Title == null)
                        {
                            continue;
                        }
                        
                        var songTitle = removeShittyCharacters(file.Title);
                        var songPath = f;
                    //    var song = await sqliteHelper.GetSong(artistName, file.Tag.Album, file.Tag.Title);
                           // Dedup primarily by file path (matches the unique index on Songs.Path),
                           // falling back to artist/title/album so that a track which appears on
                           // several releases is still only kept once.
                           var song = dbSongs.Where(x => x.Path == songPath).FirstOrDefault()
                                   ?? dbSongs.Where(x => x.Artist.ToLower() == artistName.ToLower()
                                                       && x.Title.ToLower() == songTitle.ToLower()
                                                       && x.Album.ToLower() == albumTitle.ToLower())
                                              .FirstOrDefault()
                                   ?? songs.Where(x => x.Path == songPath).FirstOrDefault()
                                   ?? songs.Where(x => x.Artist.ToLower() == artistName.ToLower()
                                                    && x.Title.ToLower() == songTitle.ToLower()
                                                    && x.Album.ToLower() == albumTitle.ToLower())
                                           .FirstOrDefault();

                        if (song == null)
                        {
                            int duration = file.Duration;
                            int bitrate = file.Bitrate;
                            int bitDepth = file.BitDepth;
                            int sampleRate = (int)file.SampleRate;

                            // ATL reports 0/sentinel stream info for FLAC files with a prepended
                            // ID3 tag; recover the real values from the FLAC STREAMINFO block.
                            if (sampleRate <= 0 || bitDepth <= 0 || duration <= 0)
                            {
                                var info = ReadFlacStreamInfo(songPath);
                                if (info != null)
                                {
                                    duration = info.Value.durationSeconds;
                                    bitrate = info.Value.bitrateKbps;
                                    bitDepth = info.Value.bitDepth;
                                    sampleRate = info.Value.sampleRate;
                                }
                            }

                            var songAdd = new Songs()
                            {
                                Id = songId,
                                Title = songTitle,
                                TrackNumber = Convert.ToInt32(file.TrackNumber),
                                Album = albumTitle,
                                AlbumId = albumId,
                                Artist = artistName,    
                                ArtistId = artistId,
                                AlbumPicture = "",
                                DiscNumber =  file.DiscNumber ?? 1,
                                Favourite = false,
                                Length = duration.ToString(),
                                Codec = file.AudioFormat.ShortName,
                                BitRate = bitrate.ToString(),
                                BitDepth = bitDepth.ToString(),
                                SampleRate = sampleRate.ToString(),
                                Path = songPath


                            };
                            songs.Add(songAdd);
                            //sqliteHelper.UploadSong(songAdd);
                            Console.WriteLine("Info: Inserted Song:" + songAdd.Title + " : " + songAdd.Artist );
                        }
                        else if (!songs.Contains(song))
                        {
                            // Existing DB song: refresh its disc number.
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
