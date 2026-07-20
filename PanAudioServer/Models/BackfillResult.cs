namespace PanAudioServer.Models
{
    public class BackfillResult
    {
        public int Submitted { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
    }
}
