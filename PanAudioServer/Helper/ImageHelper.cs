﻿using PanAudioServer.Models;

namespace PanAudioServer.Helper
{
    public class ImageHelper
    {
        SqliteHelper sqliteHelper = new SqliteHelper();

        public string ImagePath(string albumId)
        {
            var album = sqliteHelper.GetAlbumById(albumId);
            return Path.Combine(album.AlbumPath, album.Picture ?? "");
        }

        public void SetImage(string albumId, string imageName)
        {
            var album = sqliteHelper.GetAlbumById(albumId);
            album.Picture = imageName;
            sqliteHelper.UpdateAlbum(album);
        }

        public string ArtistImagePath(string artistId)
        {
            var artist = sqliteHelper.GetArtistById(artistId);
            var fullPath = "";
            try
            {
                fullPath = Path.Combine(artist.ArtistPath ?? "", artist.Picture ?? "");
            }catch(Exception ex)
            {
               // Console.WriteLine("Error Getting Image for " + artist.Name);
            }


            return "";
        }
    }
}
