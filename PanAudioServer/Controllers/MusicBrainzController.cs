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
        public async Task<String> getAlbumArt(string albumId)
        {
            var result = await helper.getAlbumArtAsync(albumId);
            return result;
        }

        [HttpGet("albums")]
        public async Task<MusicBrainzReleases> getAlbums(string artistId)
        {
            var result = await helper.getAlbumIdAsync(artistId);
           // result.releases.Where(x => x.)
            return result;
        }
    }
}
