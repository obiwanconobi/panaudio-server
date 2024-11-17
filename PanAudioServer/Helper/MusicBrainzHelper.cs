using System.Formats.Asn1;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Models;


namespace PanAudioServer.Helper
{
    public class MusicBrainzHelper
    {
        // http://musicbrainz.org/ws/2/artist/?query=artist:blink-182&fmt=json

        HttpClient _httpClient = new HttpClient();
        SqliteHelper sqliteHelper = new SqliteHelper();
        ImageHelper imageHelper = new ImageHelper();
        public MusicBrainzHelper() {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "panaudio-server-beta/0.0.5 (panaudio-support@panaro.co.uk)");
        }

        public async Task<String> getAlbumArtAsync(string id)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("https://coverartarchive.org/release/" + id);

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var albums = JsonSerializer.Deserialize<CoverArtArchiveAlbum>(responseBody);
            return albums.images.FirstOrDefault().image;
        }

        public async Task<string> getAlbumIdAsync(string musicBrainzArtistId, string albumName)
        {
            //http://musicbrainz.org/ws/2/release/?query=arid:"0743b15a-3c32-48c8-ad58-cb325350befa"&primarytype:"album"&fmt=json
            using HttpResponseMessage response = await _httpClient.GetAsync("https://musicbrainz.org/ws/2/release/?query=arid:" + musicBrainzArtistId + " AND release:" + albumName + " AND status:Official&fmt=json&inc=artist-credits&limit=1");

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Replace("artist-credit", "artistcredit");
            var albums = JsonSerializer.Deserialize<MusicBrainzReleases>(responseBody);
            return albums.releases[0].id;
        }


        public async Task setArtistId(string artistName)
        {
            var id = await getAlbumArtAsync(artistName);
            var artist = sqliteHelper.GetArtist(artistName);
            artist.MusicBrainzId = id;
            sqliteHelper.UpdateArtist(artist);


        }

        public async Task<string> getArtistIdAsync(string artistName)
        {
            
            using HttpResponseMessage response = await _httpClient.GetAsync("http://musicbrainz.org/ws/2/artist/?query=artist:" + artistName + "&fmt=json&limit=1");
            
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var artist = JsonSerializer.Deserialize<MusicBrainzArtist>(responseBody);
            return artist.artists[0].id ?? "";
        }

        public async Task setAlbumId(string artist, string album)
        {

            var artistf =  sqliteHelper.GetArtist(artist);
            var albumId = await getAlbumIdAsync(artistf.MusicBrainzId, album);
            var albumf = await sqliteHelper.GetAlbum(artist, album);
            albumf.MusicBrainzId = albumId;

            sqliteHelper.UpdateAlbum(albumf);



        }

        public async Task setAlbum(string artist, string album)
        {
            var albumId = await sqliteHelper.GetMusicBrainzUrl(artist, album);
            if (albumId == "") { 
              await setAlbumId(artist, album);
            }

            var albumSave = await sqliteHelper.GetAlbum(artist, album);
            var albumUrl = await getAlbumArtAsync(albumSave.MusicBrainzId);
            var savePlace = imageHelper.ImagePath(albumSave.Id);
            byte[] imageBytes = await _httpClient.GetByteArrayAsync(albumUrl);
            try
            {
                await File.WriteAllBytesAsync(Path.Combine(savePlace, "cover.jpg"), imageBytes);
                albumSave.Picture = "cover.jpg";
                sqliteHelper.UpdateAlbum(albumSave);
            }catch (Exception ex) {
                
            }
         

        }



    }
}
