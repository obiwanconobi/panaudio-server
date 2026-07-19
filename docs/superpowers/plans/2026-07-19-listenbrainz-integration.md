# ListenBrainz Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add best-effort ListenBrainz submission to the playback logging pipeline, with token config and bug fixes.

**Architecture:** Activate the empty `PlaybackHelper` as the playback orchestrator, add a new `ListenBrainzClient` service for HTTP submission, fix two bugs in `SqliteHelper.StartRecordPlayback`. Local DB logging always runs; ListenBrainz submission fires after the DB transaction and fails silently.

**Tech Stack:** .NET 8, EF Core + SQLite, NUnit, Microsoft.Data.Sqlite in-memory, FakeHttpMessageHandler

---

### Task 1: Fix duration parsing bug

**Files:**
- Modify: `PanAudioServer/Helper/SqliteHelper.cs:429-489`

- [ ] **Step 1: Replace `int.Parse` with safe parser in StartRecordPlayback**

In `SqliteHelper.cs`, find the `StartRecordPlayback` method (line 429). Add a private static helper method at the bottom of `SqliteHelper`:

```csharp
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
```

Replace the two `int.Parse` calls in `StartRecordPlayback`:

Line 440: `int songLength = int.Parse(song.Length);` → `int songLength = ParseDuration(song.Length);`
Line 444: `int.Parse(fullSong.Length)` → `ParseDuration(fullSong.Length)`
Line 460: `int.Parse(fullSong.Length)` → `ParseDuration(fullSong.Length)`

- [ ] **Step 2: Build and verify compilation**

```
dotnet build -c Release
```

- [ ] **Step 3: Commit**

```bash
git add PanAudioServer/Helper/SqliteHelper.cs
git commit -m "fix: safe parse colon-delimited durations in StartRecordPlayback"
```

---

### Task 2: Fix seconds truncation bug

**Files:**
- Modify: `PanAudioServer/Helper/SqliteHelper.cs:449`

- [ ] **Step 1: Change `secondsLength.Seconds` to `(int)secondsLength.TotalSeconds`**

In `SqliteHelper.cs` line 449, change:
```csharp
await UpdateLastPlayback(lastSong, secondsLength.Seconds);
```
to:
```csharp
await UpdateLastPlayback(lastSong, (int)secondsLength.TotalSeconds);
```

- [ ] **Step 2: Build**

```
dotnet build -c Release
```

- [ ] **Step 3: Commit**

```bash
git add PanAudioServer/Helper/SqliteHelper.cs
git commit -m "fix: use TotalSeconds instead of Seconds in playback recording"
```

---

### Task 3: Add ListenBrainz token config to ConfigHelper

**Files:**
- Modify: `PanAudioServer/Helper/ConfigHelper.cs`

- [ ] **Step 1: Add GetListenBrainzToken and SetListenBrainzToken methods**

After line 61 (`}` closing `SetEnableTagWriting`), add:

```csharp
    public async Task<string> GetListenBrainzToken()
    {
        var value = await sqliteHelper.GetConfigValue("ListenBrainzToken");
        return value ?? "";
    }

    public async Task SetListenBrainzToken(string token)
    {
        await sqliteHelper.SetConfigValue("ListenBrainzToken", token);
    }
```

- [ ] **Step 2: Build**

```
dotnet build -c Release
```

- [ ] **Step 3: Commit**

```bash
git add PanAudioServer/Helper/ConfigHelper.cs
git commit -m "feat: add ListenBrainz token config to ConfigHelper"
```

---

### Task 4: Create ListenBrainzClient service

**Files:**
- Create: `PanAudioServer/Helper/ListenBrainzClient.cs`

- [ ] **Step 1: Create the ListenBrainzClient class**

```csharp
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
```

- [ ] **Step 2: Build**

```
dotnet build -c Release
```

- [ ] **Step 3: Commit**

```bash
git add PanAudioServer/Helper/ListenBrainzClient.cs
git commit -m "feat: add ListenBrainzClient for submit-listens API"
```

---

### Task 5: Activate PlaybackHelper as orchestrator

**Files:**
- Modify: `PanAudioServer/Helper/PlaybackHelper.cs`

- [ ] **Step 1: Rewrite PlaybackHelper with orchestration logic**

Replace the entire contents of `PlaybackHelper.cs`:

```csharp
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
```

Note: This accesses `sqliteHelper._context` directly. We need to mark the `_context` field as `internal` in `SqliteHelper.cs`.

- [ ] **Step 2: Mark SqliteHelper._context as internal**

In `SqliteHelper.cs` line 9, change:
```csharp
private SqliteContext? _context;
```
to:
```csharp
internal SqliteContext? _context;
```

- [ ] **Step 3: Build**

```
dotnet build -c Release
```

- [ ] **Step 4: Commit**

```bash
git add PanAudioServer/Helper/PlaybackHelper.cs PanAudioServer/Helper/SqliteHelper.cs
git commit -m "feat: activate PlaybackHelper as playback orchestrator with ListenBrainz"
```

---

### Task 6: Register ListenBrainzClient in DI, remove old StartRecordPlayback

**Files:**
- Modify: `PanAudioServer/Program.cs`
- Modify: `PanAudioServer/Helper/SqliteHelper.cs`

- [ ] **Step 1: Register ListenBrainzClient as Scoped in Program.cs**

After line 48 (`builder.Services.AddScoped<PlaybackHelper>();`), add:
```csharp
builder.Services.AddScoped<ListenBrainzClient>();
```

- [ ] **Step 2: Remove StartRecordPlayback from SqliteHelper**

Delete the `StartRecordPlayback` method from `SqliteHelper.cs` (lines 428-489). Keep `GetLastPlaySong`, `UpdateLastPlayback`, `GetLastPlayDate`, and all read methods.

- [ ] **Step 3: Build**

```
dotnet build -c Release
```

- [ ] **Step 4: Commit**

```bash
git add PanAudioServer/Program.cs PanAudioServer/Helper/SqliteHelper.cs
git commit -m "feat: register ListenBrainzClient, remove old StartRecordPlayback from SqliteHelper"
```

---

### Task 7: Update PlaybackController to use PlaybackHelper

**Files:**
- Modify: `PanAudioServer/Controllers/PlaybackController.cs`

- [ ] **Step 1: Inject PlaybackHelper for the start endpoint**

Replace the controller with:

```csharp
using Microsoft.AspNetCore.Mvc;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Controllers
{
    [ApiController]
    [Route("api/playback")]
    public class PlaybackController : Controller
    {
        private SqliteHelper sqliteHelper;
        private PlaybackHelper playbackHelper;
        private ConfigHelper configHelper;

        public PlaybackController(SqliteHelper sqliteHelper, PlaybackHelper playbackHelper, ConfigHelper configHelper)
        {
            this.sqliteHelper = sqliteHelper;
            this.playbackHelper = playbackHelper;
            this.configHelper = configHelper;
        }

        [HttpPut("start")]
        public async Task StartPlayback(string songId)
        {
            await playbackHelper.StartRecordPlayback(songId);
        }

        [HttpGet("history")]
        public async Task<List<PlaybackCounts>> GetPlayback()
        {
           return await sqliteHelper.GetPlaybackHistory();
        }

        [HttpGet("playbackday")]
        public async Task<List<PlaybackHistory>> GetPlaybackDay(DateOnly day)
        {
            return await sqliteHelper.GetPlaybackHistoryForDay(day);
        }

        [HttpGet("historyByDate")]
        public async Task<List<PlaybackCounts>> GetPlaybackByDate(DateTime startDate, DateTime endDate)
        {
            var playbackTime = await configHelper.GetPlaybackReportingTime();
            return await sqliteHelper.GetPlaybackHistoryByDate(startDate, endDate, playbackTime);
        }

        [HttpGet("historyartists")]
        public async Task<List<PlaybackArtists>> GetPlaybackArtists(DateTime startDate, DateTime endDate)
        {
            return await sqliteHelper.GetPlaybackHistoryArtists(startDate, endDate);
        }

        [HttpGet("historydays")]
        public async Task<List<PlaybackDays>> GetPlaybackArtists(DateOnly startDate, DateOnly endDate)
        {
            return await sqliteHelper.GetPlaybackByDays(startDate, endDate);
        }
    }
}
```

- [ ] **Step 2: Build**

```
dotnet build -c Release
```

- [ ] **Step 3: Commit**

```bash
git add PanAudioServer/Controllers/PlaybackController.cs
git commit -m "refactor: inject PlaybackHelper for playback start endpoint"
```

---

### Task 8: Add ListenBrainz token config endpoints

**Files:**
- Modify: `PanAudioServer/Controllers/ConfigController.cs`

- [ ] **Step 1: Add GET and POST endpoints for ListenBrainz token**

After the `SetTagWritingConfig` method (after line 53), add:

```csharp
    [HttpGet("getListenBrainzToken")]
    public async Task<string> GetListenBrainzToken()
    {
        return await _configHelper.GetListenBrainzToken();
    }

    [HttpPost("setListenBrainzToken")]
    public async Task<IActionResult> SetListenBrainzToken(string token)
    {
        await _configHelper.SetListenBrainzToken(token);
        return Ok();
    }
```

- [ ] **Step 2: Build**

```
dotnet build -c Release
```

- [ ] **Step 3: Commit**

```bash
git add PanAudioServer/Controllers/ConfigController.cs
git commit -m "feat: add ListenBrainz token config API endpoints"
```

---

### Task 9: Add frontend API functions for ListenBrainz token

**Files:**
- Modify: `frontend/src/lib/api.ts`

- [ ] **Step 1: Add token API functions**

After the existing tag writing config functions (line 233), add:

```typescript
export function fetchListenBrainzToken(): Promise<string> {
	return fetchJson('/api/getListenBrainzToken');
}

export function setListenBrainzToken(token: string): Promise<void> {
	return fetchVoid(`/api/setListenBrainzToken?token=${encodeURIComponent(token)}`, {
		method: 'POST'
	});
}
```

- [ ] **Step 2: Build frontend to verify**

```
cd frontend && npm run build
```

- [ ] **Step 3: Commit**

```bash
git add frontend/src/lib/api.ts
git commit -m "feat: add ListenBrainz token API functions to frontend"
```

---

### Task 10: Add ListenBrainz token input to settings page

**Files:**
- Modify: `frontend/src/routes/settings/+page.svelte`

- [ ] **Step 1: Add token state, fetch, and save logic**

After line 15 (`let saving = ...`), add:
```svelte
let listenbrainzToken = $state('');
```

In the `onMount` (around line 18), add `fetchListenBrainzToken` to the imports and include it in `Promise.all`:

Update imports (line 3-10) to include:
```svelte
	fetchListenBrainzToken,
	setListenBrainzToken
```

Update the `Promise.all` to:
```svelte
			[playbackTime, artistPictures, tagWriting, listenbrainzToken] = await Promise.all([
				fetchPlaybackTimeConfig(),
				fetchArtistPictureConfig(),
				fetchTagWritingConfig(),
				fetchListenBrainzToken()
			]);
```

Add save function (after `toggleTagWriting`):
```svelte
	async function saveListenBrainzToken() {
		saving = 'token';
		await setListenBrainzToken(listenbrainzToken);
		saving = null;
	}
```

- [ ] **Step 2: Add the token input card in the template**

After the Tag Writing div (after closing `</div>` of the tag writing section, line 120), add:

```svelte
			<div class="p-6 bg-zinc-900 border border-zinc-800 rounded-xl">
				<div class="flex items-center justify-between">
					<div>
						<h3 class="font-medium">ListenBrainz Token</h3>
						<p class="text-sm text-zinc-400 mt-1">Submit listens to ListenBrainz</p>
					</div>
					<div class="flex items-center gap-2">
						<input
							type="password"
							bind:value={listenbrainzToken}
							placeholder="Token..."
							class="w-48 px-3 py-1.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500"
						/>
						<button
							onclick={saveListenBrainzToken}
							disabled={saving === 'token'}
							class="px-3 py-1.5 text-sm bg-violet-600 hover:bg-violet-500 disabled:opacity-50 text-white rounded-lg transition-colors"
						>
							{saving === 'token' ? '...' : 'Save'}
						</button>
					</div>
				</div>
			</div>
```

- [ ] **Step 3: Build frontend to verify**

```
cd frontend && npm run build
```

- [ ] **Step 4: Commit**

```bash
git add frontend/src/routes/settings/+page.svelte
git commit -m "feat: add ListenBrainz token input to settings page"
```

---

### Task 11: Write ConfigHelper token tests

**Files:**
- Modify: `tests/PanAudioServer/Helper/ConfigHelperTests.cs`

- [ ] **Step 1: Add token tests**

After the `SetArtistPictures_DoesNotThrow` test (after line 100), add:

```csharp
        [Test]
        public async Task GetListenBrainzToken_ReturnsEmpty_WhenNoConfigValue()
        {
            string result = await _configHelper.GetListenBrainzToken();

            Assert.AreEqual("", result);
        }

        [Test]
        public async Task GetListenBrainzToken_ReturnsSetValue_WhenConfigValueExists()
        {
            await _configHelper.SetListenBrainzToken("test-token-123");

            string result = await _configHelper.GetListenBrainzToken();

            Assert.AreEqual("test-token-123", result);
        }

        [Test]
        public async Task SetListenBrainzToken_DoesNotThrow()
        {
            Assert.DoesNotThrowAsync(async () => await _configHelper.SetListenBrainzToken("test-token"));
        }
```

- [ ] **Step 2: Run tests**

```
dotnet test --filter "ConfigHelperTests"
```

- [ ] **Step 3: Commit**

```bash
git add tests/PanAudioServer/Helper/ConfigHelperTests.cs
git commit -m "test: add ListenBrainz token config tests"
```

---

### Task 12: Write ListenBrainzClient tests

**Files:**
- Create: `tests/PanAudioServer/Helper/ListenBrainzClientTests.cs`

- [ ] **Step 1: Create test file with FakeHttpMessageHandler**

Since the test project doesn't have a shared FakeHttpMessageHandler, define one in this file. Create `tests/PanAudioServer/Helper/ListenBrainzClientTests.cs`:

```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PanAudioServer.Data;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Tests.Helper
{
    [TestFixture]
    public class ListenBrainzClientTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private FakeLBHttpMessageHandler _httpHandler;
        private HttpClient _httpClient;
        private SqliteHelper _sqliteHelper;
        private ConfigHelper _configHelper;
        private ListenBrainzClient _client;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SqliteContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new SqliteContext(options);
            _context.Database.EnsureCreated();

            _httpHandler = new FakeLBHttpMessageHandler();
            _httpClient = new HttpClient(_httpHandler);

            _sqliteHelper = new SqliteHelper(_context);
            _configHelper = new ConfigHelper(_sqliteHelper);
            _client = new ListenBrainzClient(_httpClient, _configHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
            _httpHandler?.Dispose();
            _connection?.Dispose();
            _context?.Dispose();
        }

        private Songs MakeSong(string title = "Test Song", string artist = "Test Artist",
            string album = "Test Album", string length = "240", string musicBrainzId = null)
        {
            return new Songs
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Artist = artist,
                Album = album,
                AlbumId = Guid.NewGuid().ToString(),
                ArtistId = Guid.NewGuid().ToString(),
                AlbumPicture = "",
                Length = length,
                DiscNumber = 1,
                Codec = "mp3",
                BitRate = "320",
                BitDepth = "16",
                SampleRate = "44100",
                MusicBrainzId = musicBrainzId,
                Path = "/music/test.mp3"
            };
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_WhenNoTokenConfigured()
        {
            var song = MakeSong();
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-60), 60);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task SubmitListen_ReturnsTrue_WhenBelowThreshold()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            var song = MakeSong(length: "240");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-10), 10);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task SubmitListen_ReturnsTrue_WhenMeetsThreshold_AndServerAccepts()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "240");
            var playbackStart = DateTime.UtcNow.AddSeconds(-180);
            var result = await _client.SubmitListenAsync(song, playbackStart, 180);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task SubmitListen_SendsCorrectAuthorizationHeader()
        {
            await _configHelper.SetListenBrainzToken("my-secret-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "240");
            await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.AreEqual("Token my-secret-token", _httpHandler.LastAuthHeader);
        }

        [Test]
        public async Task SubmitListen_IncludesRecordingMbid_WhenPresent()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "240", musicBrainzId: "mb-recording-123");
            await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsTrue(_httpHandler.LastRequestBody.Contains("mb-recording-123"));
        }

        [Test]
        public async Task SubmitListen_OmitsRecordingMbid_WhenNull()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "240");
            await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsFalse(_httpHandler.LastRequestBody.Contains("recording_mbid"));
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_OnNetworkError()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(null);

            var song = MakeSong(length: "240");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_OnHttp401()
        {
            await _configHelper.SetListenBrainzToken("bad-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var song = MakeSong(length: "240");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_OnHttp400()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var song = MakeSong(length: "240");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task SubmitListen_HandlesColonDelimitedDuration()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            _httpHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"ok\"}")
            });

            var song = MakeSong(length: "4:05");
            await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);

            Assert.IsTrue(_httpHandler.LastRequestBody.Contains("\"duration_ms\":245000"));
        }

        [Test]
        public async Task SubmitListen_ReturnsFalse_WhenDurationIsZero()
        {
            await _configHelper.SetListenBrainzToken("test-token");
            var song = MakeSong(length: "");
            var result = await _client.SubmitListenAsync(song, DateTime.UtcNow.AddSeconds(-180), 180);
            Assert.IsFalse(result);
        }
    }

    public class FakeLBHttpMessageHandler : HttpMessageHandler
    {
        public string LastAuthHeader { get; private set; }
        public string LastRequestBody { get; private set; }
        private HttpResponseMessage _response;

        public void SetResponse(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastAuthHeader = request.Headers.Authorization?.ToString();
            LastRequestBody = request.Content?.ReadAsStringAsync().Result ?? "";

            if (_response == null)
                throw new HttpRequestException("Network error");

            return Task.FromResult(_response);
        }
    }
}
```

- [ ] **Step 2: Run tests**

```
dotnet test --filter "ListenBrainzClientTests"
```

- [ ] **Step 3: Commit**

```bash
git add tests/PanAudioServer/Helper/ListenBrainzClientTests.cs
git commit -m "test: add ListenBrainzClient unit tests"
```

---

### Task 13: Write PlaybackHelper integration tests

**Files:**
- Create: `tests/PanAudioServer/Helper/PlaybackHelperTests.cs`

- [ ] **Step 1: Create PlaybackHelper test file**

Create `tests/PanAudioServer/Helper/PlaybackHelperTests.cs`:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PanAudioServer.Data;
using PanAudioServer.Helper;
using PanAudioServer.Models;

namespace PanAudioServer.Tests.Helper
{
    [TestFixture]
    public class PlaybackHelperTests
    {
        private SqliteConnection _connection;
        private SqliteContext _context;
        private SqliteHelper _sqliteHelper;
        private PlaybackHelper _playbackHelper;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<SqliteContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new SqliteContext(options);
            _context.Database.EnsureCreated();

            _sqliteHelper = new SqliteHelper(_context);
            _playbackHelper = new PlaybackHelper(_sqliteHelper);
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
            _context?.Dispose();
        }

        private Songs SeedSong(string id = null, string title = "Test Song", string artist = "Test Artist",
            string album = "Test Album", string albumId = null, string artistId = null,
            string path = "/music/test/song.mp3", bool favourite = false, int? trackNumber = null,
            string length = "240", string musicBrainzId = null)
        {
            var song = new Songs
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Title = title,
                Artist = artist,
                Album = album,
                AlbumId = albumId ?? Guid.NewGuid().ToString(),
                ArtistId = artistId ?? Guid.NewGuid().ToString(),
                AlbumPicture = "",
                Path = path,
                Favourite = favourite,
                TrackNumber = trackNumber,
                Length = length,
                DiscNumber = 1,
                Codec = "mp3",
                BitRate = "320",
                BitDepth = "16",
                SampleRate = "44100",
                MusicBrainzId = musicBrainzId
            };
            _context.Songs.Add(song);
            _context.SaveChanges();
            return song;
        }

        [Test]
        public async Task StartRecordPlayback_FirstSongEver_InsertsPendingRowOnly()
        {
            var song = SeedSong("song-1", "First Song");

            await _playbackHelper.StartRecordPlayback(song.Id);

            var history = _context.PlaybackHistory.ToList();
            Assert.AreEqual(1, history.Count);
            Assert.AreEqual("song-1", history[0].SongId);
            Assert.AreEqual(0, history[0].Seconds);
        }

        [Test]
        public async Task StartRecordPlayback_SecondSong_UpdatesPreviousSongSeconds()
        {
            var songA = SeedSong("song-a", "Song A", length: "240");
            var songB = SeedSong("song-b", "Song B", length: "180");

            await _playbackHelper.StartRecordPlayback(songA.Id);

            var aRow = _context.PlaybackHistory.Where(x => x.SongId == "song-a").First();
            aRow.PlaybackStart = DateTime.UtcNow.AddSeconds(-120);
            _context.SaveChanges();

            await _playbackHelper.StartRecordPlayback(songB.Id);

            var updatedA = _context.PlaybackHistory.Where(x => x.SongId == "song-a").First();
            Assert.GreaterOrEqual(updatedA.Seconds, 119);

            var history = _context.PlaybackHistory.ToList();
            Assert.AreEqual(2, history.Count);
        }

        [Test]
        public async Task StartRecordPlayback_HandlesColonDelimitedDuration()
        {
            var songA = SeedSong("song-cd-a", "Colon Song", length: "4:05");
            var songB = SeedSong("song-cd-b", "Next Song", length: "3:30");

            await _playbackHelper.StartRecordPlayback(songA.Id);

            var aRow = _context.PlaybackHistory.Where(x => x.SongId == "song-cd-a").First();
            aRow.PlaybackStart = DateTime.UtcNow.AddSeconds(-30);
            _context.SaveChanges();

            Assert.DoesNotThrowAsync(async () => await _playbackHelper.StartRecordPlayback(songB.Id));
        }

        [Test]
        public async Task StartRecordPlayback_DurationCalculation_UsesTotalSeconds()
        {
            var songA = SeedSong("song-ts-a", "TotalSec A", length: "240");
            var songB = SeedSong("song-ts-b", "TotalSec B", length: "180");

            await _playbackHelper.StartRecordPlayback(songA.Id);

            var aRow = _context.PlaybackHistory.Where(x => x.SongId == "song-ts-a").First();
            aRow.PlaybackStart = DateTime.UtcNow.AddSeconds(-125);
            _context.SaveChanges();

            await _playbackHelper.StartRecordPlayback(songB.Id);

            var updatedA = _context.PlaybackHistory.Where(x => x.SongId == "song-ts-a").First();
            Assert.GreaterOrEqual(updatedA.Seconds, 124);
        }

        [Test]
        public async Task StartRecordPlayback_WithFullSongElapsed_GivesFullLength()
        {
            var fullSongId = Guid.NewGuid().ToString();
            var songA = SeedSong(fullSongId, "Full Song", length: "90");
            var songB = SeedSong("song-b", "Next One", length: "180");

            await _playbackHelper.StartRecordPlayback(songA.Id);

            var aRow = _context.PlaybackHistory.Where(x => x.SongId == fullSongId).First();
            aRow.PlaybackStart = DateTime.UtcNow.AddSeconds(-100);
            _context.SaveChanges();

            await _playbackHelper.StartRecordPlayback(songB.Id);

            var updatedA = _context.PlaybackHistory.Where(x => x.SongId == fullSongId).First();
            Assert.AreEqual(90, updatedA.Seconds);
        }
    }
}
```

- [ ] **Step 2: Run tests**

```
dotnet test --filter "PlaybackHelperTests"
```

- [ ] **Step 3: Commit**

```bash
git add tests/PanAudioServer/Helper/PlaybackHelperTests.cs
git commit -m "test: add PlaybackHelper integration tests"
```

---

### Task 14: Final integration - build and run all tests

**Files:** (none new, verification only)

- [ ] **Step 1: Run the full test suite**

```
dotnet test
```

Expected: all 93+ existing tests pass, plus new PlaybackHelper and ListenBrainzClient tests.

- [ ] **Step 2: Full build verification**

```
dotnet build -c Release
cd frontend && npm run build
```

- [ ] **Step 3: Commit (if any cleanup needed)**

If all tests pass and builds succeed, no commit needed for this step. The feature is complete.
