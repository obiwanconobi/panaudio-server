using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;
using System.ComponentModel;

namespace PanAudioServer.Controllers
{
    public class MusicBrainzController : Controller
    {
        MusicBrainzHelper helper = new MusicBrainzHelper();

        [HttpGet("album-art")]
        public async Task<String> getAlbumArt(string mbAlbumId)
        {
            var result = await helper.getAlbumArtAsync(mbAlbumId);
            return result;
        }

        [HttpGet("setalbumid")]
        public async Task<MusicBrainzReleases> getAlbums(string artistId, string albumName)
        {

            var result = await helper.getAlbumIdAsync(artistId, albumName);
           // var albumUrl = await helper.getAlbumArtAsync(mbAlbumId);
            // result.releases.Where(x => x.)
            return null;
        }

        [HttpGet("SetArtistId")]
        public async Task setArtistId(string artistName)
        {
            await helper.setArtistId(artistName);
        }



        [HttpGet("setalbumpicture")]
        public async Task setAlbums(string artistName, string albumName)
        {
            await helper.setAlbum(artistName, albumName);
          
        }
    }
}
