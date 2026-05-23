<script lang="ts">
	import { onMount } from 'svelte';
	import AlbumCard from '$lib/components/music/AlbumCard.svelte';
	import SongRow from '$lib/components/music/SongRow.svelte';
	import ArtistCard from '$lib/components/music/ArtistCard.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import { fetchRecentAlbums, fetchRecentReleasedAlbums, fetchFavouriteSongs, fetchFavouriteAlbums, fetchFavouriteArtists } from '$lib/api';
	import { library } from '$lib/stores/library.svelte';
	import type { Album, Song, Artist } from '$lib/types';

	let recentAlbums = $state<Album[]>([]);
	let recentReleased = $state<Album[]>([]);
	let favouriteSongs = $state<Song[]>([]);
	let favouriteAlbums = $state<Album[]>([]);
	let favouriteArtists = $state<Artist[]>([]);
	let loading = $state(true);

	onMount(async () => {
		try {
			const [recent, released, favs, favAlbums, favArtists] = await Promise.all([
				fetchRecentAlbums(),
				fetchRecentReleasedAlbums(),
				fetchFavouriteSongs(),
				fetchFavouriteAlbums(),
				fetchFavouriteArtists()
			]);
			recentAlbums = recent;
			recentReleased = released;
			favouriteSongs = favs;
			favouriteAlbums = favAlbums;
			favouriteArtists = favArtists;
		} finally {
			loading = false;
		}
	});
</script>

<svelte:head>
	<title>PanAudio</title>
</svelte:head>

<div class="space-y-10">
	<section>
		<h2 class="text-2xl font-bold mb-4">Recently Added</h2>
		{#if recentAlbums.length > 0}
			<div class="flex gap-4 overflow-x-auto pb-2 snap-x">
				{#each recentAlbums.slice(0, 10) as album (album.id)}
					<div class="w-40 shrink-0 snap-start">
						<AlbumCard {album} />
					</div>
				{/each}
			</div>
		{:else if !loading}
			<EmptyState icon="music" title="No albums yet" description="Sync your music library to get started." />
		{/if}
	</section>

	<section>
		<h2 class="text-2xl font-bold mb-4">Recent Releases</h2>
		{#if recentReleased.length > 0}
			<div class="flex gap-4 overflow-x-auto pb-2 snap-x">
				{#each recentReleased.slice(0, 10) as album (album.id)}
					<div class="w-40 shrink-0 snap-start">
						<AlbumCard {album} />
					</div>
				{/each}
			</div>
		{:else if !loading}
			<EmptyState icon="music" title="No recent releases" description="Check back after adding newer music." />
		{/if}
	</section>

	<section>
		<h2 class="text-2xl font-bold mb-4">Favourite Songs</h2>
		{#if favouriteSongs.length > 0}
			<div class="divide-y divide-zinc-800/50">
				{#each favouriteSongs.slice(0, 10) as song (song.id)}
					<SongRow {song} />
				{/each}
			</div>
		{:else if !loading}
			<EmptyState icon="heart" title="No favourites" description="Heart a song to see it here." />
		{/if}
	</section>

	<section>
		<h2 class="text-2xl font-bold mb-4">Favourite Albums</h2>
		{#if favouriteAlbums.length > 0}
			<div class="flex gap-4 overflow-x-auto pb-2 snap-x">
				{#each favouriteAlbums.slice(0, 10) as album (album.id)}
					<div class="w-40 shrink-0 snap-start">
						<AlbumCard {album} />
					</div>
				{/each}
			</div>
		{:else if !loading}
			<EmptyState icon="heart" title="No favourite albums" description="Heart an album to see it here." />
		{/if}
	</section>

	<section>
		<h2 class="text-2xl font-bold mb-4">Favourite Artists</h2>
		{#if favouriteArtists.length > 0}
			<div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4">
				{#each favouriteArtists.slice(0, 10) as artist (artist.id)}
					<ArtistCard {artist} />
				{/each}
			</div>
		{:else if !loading}
			<EmptyState icon="heart" title="No favourite artists" description="Heart an artist to see them here." />
		{/if}
	</section>

	{#if loading}
		<div class="flex justify-center py-12">
			<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
		</div>
	{/if}
</div>
