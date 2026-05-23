import type {
	Song,
	Album,
	Artist,
	Playlist,
	UpdateSongRequest,
	UpdateAlbumRequest,
	UpdateArtistRequest
} from './types';

const BASE = '';

async function fetchJson<T>(url: string, init?: RequestInit): Promise<T> {
	const res = await fetch(`${BASE}${url}`, init);
	if (!res.ok) {
		throw new Error(`API ${init?.method || 'GET'} ${url} failed: ${res.status}`);
	}
	const text = await res.text();
	return text ? JSON.parse(text) : (undefined as unknown as T);
}

async function fetchVoid(url: string, init?: RequestInit): Promise<void> {
	const res = await fetch(`${BASE}${url}`, init);
	if (!res.ok) {
		throw new Error(`API ${init?.method || 'GET'} ${url} failed: ${res.status}`);
	}
}

// ── Albums ──

export function fetchAlbums(): Promise<Album[]> {
	return fetchJson('/api/albums');
}

export function fetchAlbumById(albumId: string): Promise<Album> {
	return fetchJson(`/api/albums-by-id?albumId=${encodeURIComponent(albumId)}`);
}

export function fetchAlbumSongs(albumId: string): Promise<Song[]> {
	return fetchJson(`/api/album/${encodeURIComponent(albumId)}/songs`);
}

export function fetchRecentAlbums(): Promise<Album[]> {
	return fetchJson('/api/recent-albums');
}

export function fetchRecentReleasedAlbums(): Promise<Album[]> {
	return fetchJson('/api/recent-released-albums');
}

export function fetchFavouriteAlbums(): Promise<Album[]> {
	return fetchJson('/api/favourite-albums');
}

export function setAlbumFavourite(albumId: string, favourite: boolean): Promise<void> {
	return fetchVoid(`/api/favourite-album?albumId=${encodeURIComponent(albumId)}&favourite=${favourite}`, {
		method: 'POST'
	});
}

export function updateAlbum(albumId: string, fields: UpdateAlbumRequest): Promise<Album> {
	return fetchJson(`/api/album/${encodeURIComponent(albumId)}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(fields)
	});
}

export function deleteAlbum(albumId: string): Promise<void> {
	return fetchVoid('/api/delete-album', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ albumId })
	});
}

// ── Artists ──

export function fetchArtists(): Promise<Artist[]> {
	return fetchJson('/api/artists');
}

export function fetchFavouriteArtists(): Promise<Artist[]> {
	return fetchJson('/api/favourite-artists');
}

export function fetchArtistAlbums(artistId: string): Promise<Album[]> {
	return fetchJson(`/api/artist/${encodeURIComponent(artistId)}/albums`);
}

export function setArtistFavourite(artistId: string, favourite: boolean): Promise<void> {
	return fetchVoid(`/api/favourite-artist?artistId=${encodeURIComponent(artistId)}&favourite=${favourite}`, {
		method: 'POST'
	});
}

export function updateArtist(artistId: string, fields: UpdateArtistRequest): Promise<Artist> {
	return fetchJson(`/api/artist/${encodeURIComponent(artistId)}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(fields)
	});
}

export function deleteArtist(artistId: string): Promise<void> {
	return fetchVoid('/api/delete-artist', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ artistId })
	});
}

// ── Songs ──

export function fetchSongs(): Promise<Song[]> {
	return fetchJson('/api/songs');
}

export function fetchSong(songId: string): Promise<Song> {
	return fetchJson(`/api/song?songId=${encodeURIComponent(songId)}`);
}

export function fetchFavouriteSongs(): Promise<Song[]> {
	return fetchJson('/api/favourite-songs');
}

export function setFavourite(songId: string, favourite: boolean): Promise<void> {
	return fetchVoid(`/api/favourite?songId=${encodeURIComponent(songId)}&favourite=${favourite}`, {
		method: 'POST'
	});
}

export function updateSong(songId: string, fields: UpdateSongRequest): Promise<Song> {
	return fetchJson(`/api/song/${encodeURIComponent(songId)}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(fields)
	});
}

export function deleteSong(songId: string): Promise<void> {
	return fetchVoid('/api/delete-song', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ songId })
	});
}

// ── Search ──

export function searchSongs(query: string): Promise<Song[]> {
	return fetchJson(`/api/search/songs?query=${encodeURIComponent(query)}`);
}

export function searchAlbums(query: string): Promise<Album[]> {
	return fetchJson(`/api/search/albums?query=${encodeURIComponent(query)}`);
}

export function searchArtists(query: string): Promise<Artist[]> {
	return fetchJson(`/api/search/artists?query=${encodeURIComponent(query)}`);
}

// ── Playlists ──

export function fetchPlaylists(): Promise<Playlist[]> {
	return fetchJson('/api/playlists');
}

export function fetchPlaylist(playlistId: string): Promise<Playlist> {
	return fetchJson(`/api/playlist?playlistId=${encodeURIComponent(playlistId)}`);
}

export function createPlaylist(playlistName: string): Promise<void> {
	return fetchVoid(`/api/playlist?playlistName=${encodeURIComponent(playlistName)}`, {
		method: 'PUT'
	});
}

export function addSongToPlaylist(playlistId: string, songId: string): Promise<void> {
	return fetchVoid(
		`/api/addSong?playlistId=${encodeURIComponent(playlistId)}&songId=${encodeURIComponent(songId)}`,
		{ method: 'PUT' }
	);
}

export function removeSongFromPlaylist(playlistId: string, songId: string): Promise<void> {
	return fetchVoid(
		`/api/deleteSong?playlistId=${encodeURIComponent(playlistId)}&songId=${encodeURIComponent(songId)}`,
		{ method: 'PUT' }
	);
}

export function deletePlaylist(playlistId: string): Promise<void> {
	return fetchVoid(`/api/playlist?playlistId=${encodeURIComponent(playlistId)}`, { method: 'DELETE' });
}

// ── Playback ──

export function startPlayback(songId: string): Promise<void> {
	return fetchVoid(`/api/playback/start?songId=${encodeURIComponent(songId)}`, {
		method: 'PUT'
	});
}

// ── Config ──

export function fetchPlaybackTimeConfig(): Promise<number> {
	return fetchJson('/api/getPlaybackTimeConfig');
}

export function setPlaybackTimeConfig(time: number): Promise<void> {
	return fetchVoid('/api/setPlaybackTimeConfig', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ time })
	});
}

export function fetchArtistPictureConfig(): Promise<boolean> {
	return fetchJson('/api/getArtistPictureConfig');
}

export function setArtistPictureConfig(value: boolean): Promise<void> {
	return fetchVoid('/api/setArtistPictureConfig', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ value })
	});
}

export function fetchTagWritingConfig(): Promise<boolean> {
	return fetchJson('/api/getTagWritingConfig');
}

export function setTagWritingConfig(value: boolean): Promise<void> {
	return fetchVoid('/api/setTagWritingConfig', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ value })
	});
}

// ── Utility ──

export function albumArtUrl(albumId: string): string {
	return `/api/albumArt?albumId=${encodeURIComponent(albumId)}`;
}

export function artistArtUrl(artistId: string): string {
	return `/api/artistArt?artistId=${encodeURIComponent(artistId)}`;
}

export function audioStreamUrl(songId: string): string {
	return `/api/audio-stream?songId=${encodeURIComponent(songId)}`;
}
