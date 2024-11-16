namespace PanAudioServer.Models
{
    public class Artists
    {
        public string Id { get; set;}
        public string Name { get; set;}
        public string? Picture { get; set;}
        public string? ArtistPath { get; set;}
        public bool Favourite { get; set;}
        public string? MusicBrainzId { get; set; }

        public Artists(string id = null, string name = null, string artistPath = null, string picture = null, bool favourite = false, string musicBrainzId = null)
        {
            Id = id;
            Name = name;
            ArtistPath = artistPath;
            Picture = picture;
            Favourite = favourite;
            MusicBrainzId = musicBrainzId;
        }
    }

}
