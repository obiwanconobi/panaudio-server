using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PanAudioServer.Models;

namespace PanAudioServer.Helper;

public class ListenBrainzClient
{
    private readonly HttpClient _httpClient;
    private readonly ConfigHelper _configHelper;

    public ListenBrainzClient(HttpClient httpClient, ConfigHelper configHelper)
    {
        _httpClient = httpClient;
        _configHelper = configHelper;
    }

    public async Task<bool> SubmitListenAsync(Songs song, DateTime playbackStart, int secondsPlayed)
    {
        var token = await _configHelper.GetListenBrainzToken();
        if (string.IsNullOrEmpty(token))
            return false;

        var trackLengthSeconds = ParseDuration(song.Length);
        if (trackLengthSeconds == 0)
            return false;

        int threshold = Math.Min(trackLengthSeconds / 2, 240);
        if (secondsPlayed < threshold)
            return true;

        var payload = BuildPayload(song, playbackStart, trackLengthSeconds);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.listenbrainz.org/1/submit-listens");
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", token);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"ListenBrainz submit failed: {response.StatusCode}");
                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);
            if (result.TryGetProperty("status", out var status) && status.GetString() == "ok")
                return true;

            return false;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            Console.WriteLine($"ListenBrainz submission error: {ex.Message}");
            return false;
        }
    }

    private static object BuildPayload(Songs song, DateTime playbackStart, int trackLengthSeconds)
    {
        var listen = new Dictionary<string, object>
        {
            ["listened_at"] = new DateTimeOffset(playbackStart).ToUnixTimeSeconds(),
            ["track_metadata"] = new Dictionary<string, object>
            {
                ["artist_name"] = song.Artist,
                ["track_name"] = song.Title,
                ["release_name"] = song.Album,
                ["additional_info"] = new Dictionary<string, object>
                {
                    ["submission_client"] = "PanAudioServer",
                    ["duration_ms"] = trackLengthSeconds * 1000
                }
            }
        };

        var additionalInfo = (Dictionary<string, object>)((Dictionary<string, object>)listen["track_metadata"])["additional_info"];
        if (!string.IsNullOrEmpty(song.MusicBrainzId))
            additionalInfo["recording_mbid"] = song.MusicBrainzId;

        return new { listen_type = "single", payload = new[] { listen } };
    }

    private static int ParseDuration(string length)
    {
        if (string.IsNullOrEmpty(length)) return 0;
        if (length.Contains(':'))
        {
            var parts = length.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out int min) && int.TryParse(parts[1], out int sec))
                return min * 60 + sec;
            return 0;
        }
        return int.TryParse(length, out int s) ? s : 0;
    }
}
