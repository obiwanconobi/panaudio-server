using PanAudioServer.Models;

namespace PanAudioServer.Helper
{
    public class ImageHelper
    {
        SqliteHelper sqliteHelper;
        ConfigHelper configHelper;

        public ImageHelper()
        {
            sqliteHelper = new SqliteHelper();
            configHelper = new ConfigHelper();
        }

        public ImageHelper(SqliteHelper sqliteHelper)
        {
            this.sqliteHelper = sqliteHelper;
            configHelper = new ConfigHelper(sqliteHelper);
        }

        public string ImagePath(string albumId)
        {
            var album = sqliteHelper.GetAlbumById(albumId);
            return Path.Combine(album.AlbumPath, album.Picture ?? "");
        }

        public async Task SetImage(string albumId, string imageName)
        {
            var album = sqliteHelper.GetAlbumById(albumId);
            album.Picture = imageName;
            await sqliteHelper.UpdateAlbum(album);
        }

        public string ArtistImagePath(string artistId)
        {
            var enableArtistImages = configHelper.GetArtistPictures();
            if (!enableArtistImages) return "";
            var artist = sqliteHelper.GetArtistById(artistId);
            var fullPath = "";
            try
            {
                fullPath = Path.Combine(artist.ArtistPath ?? "", artist.Picture ?? "");
            }catch(Exception ex)
            {
               // Console.WriteLine("Error Getting Image for " + artist.Name);
            }
            return fullPath;
        }
    }
}
