

using Newtonsoft.Json;

namespace PanAudioServer.Models
{
    public class MusicBrainzReleaseGroups
    {

        public DateTime created { get; set; }
        public int count { get; set; }
        public int offset { get; set; }

        [JsonProperty("release-groups")]
        public List<ReleaseGroup> releasegroups { get; set; }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Artist
        {
            public string id { get; set; }
            public string name { get; set; }

            [JsonProperty("sort-name")]
            public string sortname { get; set; }
        }

        public class ArtistCredit
        {
            public string name { get; set; }
            public Artist artist { get; set; }
        }

        public class Release
        {
            public string id { get; set; }

            [JsonProperty("status-id")]
            public string statusid { get; set; }
            public string title { get; set; }
            public string status { get; set; }
        }

        public class ReleaseGroup
        {
            public string id { get; set; }

            [JsonProperty("type-id")]
            public string typeid { get; set; }
            public int score { get; set; }

            [JsonProperty("primary-type-id")]
            public string primarytypeid { get; set; }
            public int count { get; set; }
            public string title { get; set; }

            [JsonProperty("first-release-date")]
            public string firstreleasedate { get; set; }

            [JsonProperty("primary-type")]
            public string primarytype { get; set; }

            [JsonProperty("secondary-types")]
            public List<string> secondarytypes { get; set; }

            [JsonProperty("secondary-type-ids")]
            public List<string> secondarytypeids { get; set; }

            [JsonProperty("artist-credit")]
            public List<ArtistCredit> artistcredit { get; set; }
            public List<Release> releases { get; set; }
            public List<Tag> tags { get; set; }
        }

      

        

        public class Tag
        {
            public int count { get; set; }
            public string name { get; set; }
        }



    }
}
