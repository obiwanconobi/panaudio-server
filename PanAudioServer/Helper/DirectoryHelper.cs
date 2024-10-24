﻿using PanAudioServer.Models;
using System.Reflection;

namespace PanAudioServer.Helper
{
    public class DirectoryHelper
    {
       // private readonly string _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
       SqliteHelper sqliteHelper  = new SqliteHelper();


        public string[]? getDirectories(String path)
        {
            var directories = Directory.GetDirectories(path);
            if(directories.Length > 0)
            {
                return directories;
            }

            return null;
        }


        public void getDirectory(String directory, int depth = 0)
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
                        break;
                    }

                    if (dd.Length > 0)
                    {
                        getDirectory(dd[count], 1);
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
                        var artist = await sqliteHelper.GetArtist(artistName); 

                        if (artist == null)
                        {
                            artistId = Guid.NewGuid().ToString();
                            //set artistId,
                            sqliteHelper.UploadArtist(new Artists(id: artistId, name: artistName, picture: ""));
                            Console.WriteLine("Info: Inserted Artist: " + artistName);
                        }
                        else
                        {
                            artistId = artist.Id;
                            Console.WriteLine("Info: Artist already existed: " + artistName);
                        }


                        var album = await sqliteHelper.GetAlbum(artistName, file.Tag.Album);

                        if (album == null)
                        {
                            sqliteHelper.UploadAlbum(new Album(id: albumId, title: file.Tag.Album, artist: artistName, picture: ""));
                            Console.WriteLine("Info: Inserted Album: " + file.Tag.Album);
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

                            sqliteHelper.UploadSong(songAdd);
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
