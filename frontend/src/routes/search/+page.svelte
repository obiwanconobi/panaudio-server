<script lang="ts">
	import SearchBar from '$lib/components/ui/SearchBar.svelte';
	import AlbumGrid from '$lib/components/music/AlbumGrid.svelte';
	import SongRow from '$lib/components/music/SongRow.svelte';
	import ArtistCard from '$lib/components/music/ArtistCard.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import { searchSongs, searchAlbums, searchArtists } from '$lib/api';
	import type { Song, Album, Artist } from '$lib/types';

	let query = $state('');
	let songResults = $state<Song[]>([]);
	let albumResults = $state<Album[]>([]);
	let artistResults = $state<Artist[]>([]);
	let searching = $state(false);
	let hasSearched = $state(false);

	async function handleSearch(q: string) {
		query = q;
		if (!q.trim()) {
			songResults = [];
			albumResults = [];
			artistResults = [];
			hasSearched = false;
			return;
		}

		searching = true;
		hasSearched = true;

		try {
			const [songs, albums, artists] = await Promise.all([
				searchSongs(q),
				searchAlbums(q),
				searchArtists(q)
			]);
			songResults = songs;
			albumResults = albums;
			artistResults = artists;
		} finally {
			searching = false;
		}
	}
</script>

<svelte:head>
	<title>Search — PanAudio</title>
</svelte:head>

<div>
	<h1 class="text-2xl font-bold mb-6">Search</h1>
	<SearchBar onsearch={handleSearch} />

	{#if searching}
		<div class="flex justify-center py-12">
			<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
		</div>
	{:else if hasSearched && !songResults.length && !albumResults.length && !artistResults.length}
		<EmptyState icon="search" title="No results" description="Try a different search term." />
	{:else if hasSearched}
		<div class="mt-8 space-y-10">
			{#if songResults.length > 0}
				<section>
					<h2 class="text-lg font-semibold mb-3">Songs</h2>
					<div class="divide-y divide-zinc-800/50">
						{#each songResults.slice(0, 10) as song (song.id)}
							<SongRow {song} />
						{/each}
					</div>
				</section>
			{/if}

			{#if albumResults.length > 0}
				<section>
					<h2 class="text-lg font-semibold mb-3">Albums</h2>
					<AlbumGrid albums={albumResults.slice(0, 10)} />
				</section>
			{/if}

			{#if artistResults.length > 0}
				<section>
					<h2 class="text-lg font-semibold mb-3">Artists</h2>
					<div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
						{#each artistResults.slice(0, 10) as artist (artist.id)}
							<ArtistCard {artist} />
						{/each}
					</div>
				</section>
			{/if}
		</div>
	{/if}
</div>
