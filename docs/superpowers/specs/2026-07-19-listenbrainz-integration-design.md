# ListenBrainz Integration Design

**Date:** 2026-07-19
**Status:** Approved

---

## Overview

Add ListenBrainz submission to the existing playback logging system. When a user finishes listening to a song, if a ListenBrainz token is configured, submit the listen to ListenBrainz in addition to the local DB record. Local DB logging always happens; ListenBrainz is best-effort.

---

## Architecture

### New service: `ListenBrainzClient`
- Registered as **Scoped** in DI (receives `HttpClient` via DI)
- One public method: `Task<bool> SubmitListenAsync(Songs song, DateTime playbackStart, int secondsPlayed)`
- Returns `true` on success, `false` on failure (network error, 401, 400, etc.)
- Reads token from `ConfigHelper.GetListenBrainzToken()`. If token is empty/null, returns `false` immediately (no HTTP call).
- Checks secondsPlayed against threshold before submitting
- POSTs to `https://api.listenbrainz.org/1/submit-listens` with `Authorization: Token <token>` header

### Refactored service: `PlaybackHelper`
- Moves `StartRecordPlayback` orchestration from `SqliteHelper` into `PlaybackHelper`
- Injects `SqliteHelper`, `ConfigHelper`, `ListenBrainzClient`
- Orchestrates: record locally (always, in DB transaction), then submit to ListenBrainz (best-effort, outside transaction)

### `SqliteHelper`
- Keeps all read operations unchanged
- `StartRecordPlayback` remains public for PlaybackHelper to call
- Bug fixes applied inline (see below)

### `PlaybackController`
- Injects `PlaybackHelper` (instead of calling `SqliteHelper` directly for the `start` endpoint)
- All other endpoints unchanged

---

## Config

Two new config keys in the `Config` DB table, managed via `ConfigHelper`:

| Key | Default | Purpose |
|-----|---------|---------|
| `listenbrainz_token` | `""` (empty) | User token from listenbrainz.org/settings. Empty = disabled. |

No separate enabled/disabled toggle — presence of a non-empty token enables ListenBrainz.

`ConfigHelper` additions:
- `GetListenBrainzToken()` → `Task<string>`
- `SetListenBrainzToken(string token)` → `Task`

Backend API additions (`ConfigController`):
- `GET /api/getListenBrainzToken` → `Task<string>`
- `POST /api/setListenBrainzToken?token=...` → `Task<IActionResult>`

Frontend additions (follows existing `ConfigController` pattern, NOT the nginx `/config` endpoint):
- `api.ts`: add `fetchListenBrainzToken()`, `setListenBrainzToken()` — calls `ConfigController` API
- Settings page: text input for the token with show/hide toggle
- Token is NOT exposed via nginx `/config` (it's a secret). No changes to vite.config.ts, nginx.conf.template, or compose.yml needed.

---

## ListenBrainz Submission Spec

### Threshold (per ListenBrainz spec)
"half the track or 4 minutes, whichever is lower":

```csharp
int threshold = Math.Min((int)(trackLengthSeconds * 0.5), 240);
if (secondsPlayed >= threshold) → submit to ListenBrainz
```

### JSON Payload

```json
{
  "listen_type": "single",
  "payload": [{
    "listened_at": <unix timestamp of playback start>,
    "track_metadata": {
      "artist_name": "<Song.Artist>",
      "track_name": "<Song.Title>",
      "release_name": "<Song.Album>",
      "additional_info": {
        "submission_client": "PanAudioServer",
        "duration_ms": <trackLengthMs>
      }
    }
  }]
}
```

- `recording_mbid` included only when `Song.MusicBrainzId` is non-empty (it's often null per existing codebase)
- `listened_at` = `PlaybackStart` as Unix timestamp (not completion time)
- `submission_client` = `"PanAudioServer"`

---

## Data Flow

When song B starts, `PlaybackHelper.StartRecordPlayback("B")` runs:

1. Start DB transaction
2. `GetLastPlaySong()` → song A's pending row (if exists)
3. If song A exists: calculate `elapsed = (int)(UtcNow - A.PlaybackStart).TotalSeconds`
4. `UpdateLastPlayback(A, elapsed)` — writes seconds to DB
5. Insert new pending row for song B
6. Commit transaction
7. **AFTER** transaction (best-effort):
   - Check: is ListenBrainz token configured?
   - Check: does song A's `elapsed` meet threshold?
   - If both yes: `ListenBrainzClient.SubmitListenAsync(songA, A.PlaybackStart, elapsed)`
   - Log any failure (Sentry + console), do NOT throw

### Edge cases
- **First song ever**: no previous song → skip steps 3, 4, 7
- **Last song in session**: its `Seconds` stays 0 (never submitted). Acceptable — the song was never "completed."
- **Same song back-to-back**: each play is independently evaluated

---

## Bug Fixes (in `StartRecordPlayback`)

1. `secondsLength.Seconds` → `(int)secondsLength.TotalSeconds` — currently records only 0-59 range
2. `int.Parse(song.Length)` → use safe parsing that handles both `"245"` (plain seconds) and `"4:05"` (colon-delimited)

Helper method (in `PlaybackHelper` or shared utility):
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

---

## Error Handling

- ListenBrainz failure never blocks local logging — transaction commits first
- Network errors, 401, 400, 429 are caught, logged to Sentry, silently dropped
- Empty token → skip ListenBrainz entirely, no HTTP call
- `{ "code": 200, "status": "ok" }` is the expected success response
- Unparseable song `Length` → threshold check fails safe (treat as 0, skip submission)

---

## Testing Strategy

### `ListenBrainzClientTests.cs` (new)
- Uses `FakeHttpMessageHandler` (existing pattern)
- No token → returns false, no HTTP call
- Meets threshold → correct JSON, Authorization header, timestamp
- Below threshold → skips submission
- Network failure → returns false, no throw
- HTTP 401/400 → returns false, no throw
- MusicBrainzId null → omits `recording_mbid`
- Duration parsing: colon-delimited and plain seconds

### `PlaybackHelperTests.cs` (new)
- First song ever → inserts pending row only
- Second song, threshold met → correct seconds, submits to LB, inserts new pending
- Second song, threshold not met → correct seconds, no LB submission
- Colon-delimited length → parsed correctly
- LB failure → local DB still correct
- Bug fix: `TotalSeconds` not `.Seconds`

### Existing tests
- No changes required. SqliteHelper read methods unchanged.

---

## Files Changed

| File | Change |
|------|--------|
| `Helper/ListenBrainzClient.cs` | **New.** HTTP client for submit-listens |
| `Helper/PlaybackHelper.cs` | Move orchestration here; inject SqliteHelper + ListenBrainzClient |
| `Helper/ConfigHelper.cs` | Add GetListenBrainzToken / SetListenBrainzToken |
| `Helper/SqliteHelper.cs` | Bug fixes; keep StartRecordPlayback public for PlaybackHelper |
| `Controllers/PlaybackController.cs` | Inject PlaybackHelper for `start` endpoint |
| `Controllers/ConfigController.cs` | Add token endpoints |
| `Program.cs` | Register ListenBrainzClient as Scoped |
| `frontend/src/lib/api.ts` | Add token API functions |
| `frontend/src/lib/api.ts` | Add token API functions (calls ConfigController, not /config) |
| `frontend/src/routes/settings/+page.svelte` | Add token input with show/hide toggle |
| `tests/.../ListenBrainzClientTests.cs` | **New.** |
| `tests/.../PlaybackHelperTests.cs` | **New.** |
