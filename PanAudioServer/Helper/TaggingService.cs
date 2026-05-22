using ATL;
using PanAudioServer.Helper;

namespace PanAudioServer.Helper
{
    public class TaggingService
    {
        private readonly ConfigHelper _configHelper;

        public TaggingService(ConfigHelper configHelper)
        {
            _configHelper = configHelper;
        }

        public async Task WriteTagsAsync(string filePath, Dictionary<string, object> changedFields)
        {
            if (!await _configHelper.GetEnableTagWriting())
                return;

            try
            {
                var track = new Track(filePath);
                if (changedFields.ContainsKey("title")) track.Title = (string)changedFields["title"];
                if (changedFields.ContainsKey("artist")) track.Artist = (string)changedFields["artist"];
                if (changedFields.ContainsKey("album")) track.Album = (string)changedFields["album"];
                if (changedFields.ContainsKey("trackNumber")) track.TrackNumber = Convert.ToInt32(changedFields["trackNumber"]);
                if (changedFields.ContainsKey("discNumber")) track.DiscNumber = Convert.ToInt32(changedFields["discNumber"]);
                await Task.Run(() => track.Save());
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
    }
}
