import type { Song, Album, Artist } from '../types';

let albums = $state<Album[]>([]);
let artists = $state<Artist[]>([]);
let songs = $state<Song[]>([]);
let loaded = $state(false);

export const library = {
	get albums() { return albums; },
	get artists() { return artists; },
	get songs() { return songs; },
	get loaded() { return loaded; },

	setAlbums(value: Album[]) { albums = value; },
	setArtists(value: Artist[]) { artists = value; },
	setSongs(value: Song[]) { songs = value; },
	setLoaded(value: boolean) { loaded = value; },

	getAlbumById(id: string): Album | undefined {
		return albums.find((a) => a.id === id);
	},
	getArtistById(id: string): Artist | undefined {
		return artists.find((a) => a.id === id);
	},
	getSongById(id: string): Song | undefined {
		return songs.find((s) => s.id === id);
	}
};
