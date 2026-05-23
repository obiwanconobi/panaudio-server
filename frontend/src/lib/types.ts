export interface Song {
	id: string;
	trackNumber: number | null;
	title: string;
	album: string;
	albumId: string;
	artist: string;
	artistId: string;
	albumPicture: string;
	discNumber: number;
	favourite: boolean | null;
	length: string;
	codec: string;
	bitRate: string;
	bitDepth: string;
	sampleRate: string;
	path: string;
	musicBrainzId: string | null;
	playCount: number;
}

export interface Album {
	id: string;
	title: string;
	artist: string;
	year: number | null;
	picture: string | null;
	albumPath: string | null;
	dateAdded: string;
	favourite: boolean;
	musicBrainzId: string | null;
}

export interface Artist {
	id: string;
	name: string;
	picture: string | null;
	artistPath: string | null;
	favourite: boolean;
	musicBrainzId: string | null;
}

export interface Playlist {
	playlistId: string;
	playlistName: string;
	playlistItems?: { song: Song }[];
}

export interface UpdateSongRequest {
	title?: string;
	artist?: string;
	album?: string;
	trackNumber?: number;
	discNumber?: number;
	favourite?: boolean;
}

export interface UpdateAlbumRequest {
	title?: string;
	artist?: string;
	year?: number;
}

export interface UpdateArtistRequest {
	name?: string;
}

export interface PlaybackCount {
	songId: string;
	title: string;
	artist: string;
	album: string;
	playCount: number;
	lastPlayed: string;
}

export interface PlaybackHistory {
	songId: string;
	playedAt: string;
}
