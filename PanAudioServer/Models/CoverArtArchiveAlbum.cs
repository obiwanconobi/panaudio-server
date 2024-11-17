﻿namespace PanAudioServer.Models
{
    public class CoverArtArchiveAlbum
    {

      
            public Image[] images { get; set; }
            public string release { get; set; }
        

        public class Image
        {
            public bool approved { get; set; }
            public bool back { get; set; }
            public string comment { get; set; }
            public int edit { get; set; }
            public bool front { get; set; }
            public string id { get; set; }
            public string image { get; set; }
            public Thumbnails thumbnails { get; set; }
            public string[] types { get; set; }
        }

        public class Thumbnails
        {
            public string _1200 { get; set; }
            public string _250 { get; set; }
            public string _500 { get; set; }
            public string large { get; set; }
            public string small { get; set; }
        }

    }
}
