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

        public string ArtistImagePath(string artistId)
        {
            var artist = sqliteHelper.GetArtistById(artistId);
            return Path.Combine(artist.ArtistPath ?? "", artist.Picture ?? "");
        }
    }
}
