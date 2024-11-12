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
        public MusicBrainzHelper() {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "panaudio-server-beta");
        }

        public async Task<String> getAlbumArtAsync(string id)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("https://coverartarchive.org/release/" + id);

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var albums = JsonSerializer.Deserialize<CoverArtArchiveAlbum>(responseBody);
            return albums.images.FirstOrDefault().image;
        }

        public async Task<MusicBrainzReleases> getAlbumIdAsync(string musicBrainzArtistId)
        {
            //http://musicbrainz.org/ws/2/release/?query=arid:"0743b15a-3c32-48c8-ad58-cb325350befa"&primarytype:"album"&fmt=json
            using HttpResponseMessage response = await _httpClient.GetAsync("http://musicbrainz.org/ws/2/release/?query=arid:" + musicBrainzArtistId + "&primarytype:Album&fmt=json&inc=artist-credits");

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Replace("artist-credit", "artistcredit");
            var albums = JsonSerializer.Deserialize<MusicBrainzReleases>(responseBody);
            return albums;
        }

        public async Task<string> getArtistIdAsync(string artistName)
        {
            
            using HttpResponseMessage response = await _httpClient.GetAsync("http://musicbrainz.org/ws/2/artist/?query=artist:" + artistName.Replace(" ", "") + "&fmt=json");
            
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var artist = JsonSerializer.Deserialize<MusicBrainzArtist>(responseBody);
            return artist.artists[0].id;
        }

    }
}
