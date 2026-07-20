using Microsoft.EntityFrameworkCore;
using PanAudioServer.Models;

namespace PanAudioServer.Helper;

public class PlaybackHelper
{
    private SqliteHelper sqliteHelper;
    private ListenBrainzClient? listenBrainzClient;

    public PlaybackHelper()
    {
        sqliteHelper = new SqliteHelper();
    }

    public PlaybackHelper(SqliteHelper sqliteHelper)
    {
        this.sqliteHelper = sqliteHelper;
    }

    public PlaybackHelper(SqliteHelper sqliteHelper, ListenBrainzClient listenBrainzClient)
    {
        this.sqliteHelper = sqliteHelper;
        this.listenBrainzClient = listenBrainzClient;
    }

    public async Task StartRecordPlayback(string songId)
    {
        Songs? previousSong = null;
        DateTime? previousPlaybackStart = null;
        int elapsedSeconds = 0;

        DateTime playbackStartTime = DateTime.UtcNow;

        try
        {
            await sqliteHelper._context.Database.BeginTransactionAsync();
            var lastSong = await sqliteHelper.GetLastPlaySong();
            var song = await sqliteHelper.GetSongById(songId);
            int songLength = ParseDuration(song.Length);

            if (lastSong != null)
            {
                var fullSong = await sqliteHelper.GetSongById(lastSong.SongId);
                if (DateTime.UtcNow < lastSong.PlaybackStart.AddSeconds(ParseDuration(fullSong.Length)))
                {
                    elapsedSeconds = (int)(DateTime.UtcNow - lastSong.PlaybackStart).TotalSeconds;
                    await sqliteHelper.UpdateLastPlayback(lastSong, elapsedSeconds);
                    Console.WriteLine("Updated last Playback for: " + fullSong.Title + " With Seconds: " + elapsedSeconds);

                    previousSong = fullSong;
                    previousPlaybackStart = lastSong.PlaybackStart;

                    await sqliteHelper._context.PlaybackHistory.AddAsync(new PlaybackHistory() { SongId = songId, PlaybackStart = playbackStartTime });
                    Console.WriteLine("Playback logged for song: " + songId);
                }
                else
                {
                    await sqliteHelper.UpdateLastPlayback(lastSong, ParseDuration(fullSong.Length));
                    Console.WriteLine("Updated last Playback for: " + fullSong.Title + " With full song Length");

                    previousSong = fullSong;
                    previousPlaybackStart = lastSong.PlaybackStart;
                    elapsedSeconds = ParseDuration(fullSong.Length);

                    await sqliteHelper._context.PlaybackHistory.AddAsync(new PlaybackHistory() { SongId = songId, PlaybackStart = playbackStartTime });
                    Console.WriteLine("Playback logged for song: " + songId);
                }
            }
            else
            {
                await sqliteHelper._context.PlaybackHistory.AddAsync(new PlaybackHistory() { SongId = songId, PlaybackStart = playbackStartTime });
                Console.WriteLine("Playback logged for song: " + songId);
            }

            await sqliteHelper._context.SaveChangesAsync();
            await sqliteHelper._context.Database.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            Console.WriteLine("Error Logging Song: " + songId);
            Console.WriteLine(ex.ToString());
        }

        if (previousSong != null && previousPlaybackStart != null && listenBrainzClient != null)
        {
            await listenBrainzClient.SubmitListenAsync(previousSong, previousPlaybackStart.Value, elapsedSeconds);
        }
    }

    public async Task<BackfillResult> BackfillHistoricalListensAsync(DateTime startDate, DateTime endDate)
    {
        var result = new BackfillResult();

        if (listenBrainzClient == null)
            return result;

        var token = await sqliteHelper.GetConfigValue("ListenBrainzToken");
        if (string.IsNullOrEmpty(token))
            return result;

        try
        {
            var records = await sqliteHelper.GetPlaybackHistoryRawByDate(startDate, endDate);
            if (records.Count == 0)
                return result;

            var songIds = records.Select(r => r.SongId).Distinct().ToList();
            var songs = await sqliteHelper._context.Songs
                .Where(s => songIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            var qualified = new List<(Songs Song, DateTime PlaybackStart)>();

            foreach (var record in records)
            {
                if (!songs.TryGetValue(record.SongId, out var song))
                {
                    result.Failed++;
                    continue;
                }

                var trackLengthSeconds = ParseDuration(song.Length);
                if (trackLengthSeconds == 0)
                {
                    result.Failed++;
                    continue;
                }

                int threshold = Math.Min(trackLengthSeconds / 2, 240);
                if (record.Seconds < threshold)
                {
                    result.Skipped++;
                    continue;
                }

                qualified.Add((song, record.PlaybackStart));
            }

            if (qualified.Count > 0)
            {
                var (submitted, failed) = await listenBrainzClient.SubmitImportListensAsync(qualified);
                result.Submitted = submitted;
                result.Failed += failed;
            }
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            Console.WriteLine($"BackfillHistoricalListensAsync error: {ex}");
        }

        return result;
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
