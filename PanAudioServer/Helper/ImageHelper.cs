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

        public async Task<string> ImagePath(string albumId)
        {
            var album = await sqliteHelper.GetAlbumById(albumId);
            return Path.Combine(album.AlbumPath, album.Picture ?? "");
        }

        public async Task SetImage(string albumId, string imageName)
        {
            var album = await sqliteHelper.GetAlbumById(albumId);
            album.Picture = imageName;
            await sqliteHelper.UpdateAlbum(album);
        }

        public async Task<string> ArtistImagePath(string artistId)
        {
            var enableArtistImages = await configHelper.GetArtistPictures();
            if (!enableArtistImages) return "";
            var artist = await sqliteHelper.GetArtistById(artistId);
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
