namespace PanAudioServer.Models
{
    public class Artists
    {
        public string Id { get; set;}
        public string Name { get; set;}
        public string Picture { get; set;}

        public Artists(string id = null, string name = null, string picture = null)
        {
            Id = id;
            Name = name;
            Picture = picture;
        }
    }

}
