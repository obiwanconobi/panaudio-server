<script lang="ts">
	import { onMount } from 'svelte';
	import ArtistCard from '$lib/components/music/ArtistCard.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import { fetchArtists, fetchFavouriteArtists } from '$lib/api';
	import { library } from '$lib/stores/library.svelte';
	import type { Artist } from '$lib/types';

	let allArtists = $state<Artist[]>([]);
	let favouriteArtists = $state<Artist[]>([]);
	let showFavourites = $state(false);
	let loading = $state(true);

	onMount(async () => {
		try {
			[allArtists, favouriteArtists] = await Promise.all([
				fetchArtists(),
				fetchFavouriteArtists()
			]);
			library.setArtists(allArtists);
		} finally {
			loading = false;
		}
	});

	let artists = $derived(showFavourites ? favouriteArtists : allArtists);
</script>

<svelte:head>
	<title>Artists — PanAudio</title>
</svelte:head>

<div>
	<div class="flex items-center justify-between mb-6">
		<h1 class="text-2xl font-bold">{showFavourites ? 'Favourite Artists' : 'Artists'}</h1>
		<div class="flex gap-2">
			<button
				onclick={() => (showFavourites = false)}
				class="px-3 py-1.5 text-sm rounded-lg transition-colors {showFavourites ? 'text-zinc-400 hover:text-zinc-200' : 'bg-violet-600 text-white'}"
			>
				All
			</button>
			<button
				onclick={() => (showFavourites = true)}
				class="px-3 py-1.5 text-sm rounded-lg transition-colors {showFavourites ? 'bg-violet-600 text-white' : 'text-zinc-400 hover:text-zinc-200'}"
			>
				Favourites
			</button>
		</div>
	</div>

	{#if loading}
		<div class="flex justify-center py-12">
			<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
		</div>
	{:else if artists.length > 0}
		<div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4">
			{#each artists as artist (artist.id)}
				<ArtistCard {artist} />
			{/each}
		</div>
	{:else}
		<EmptyState icon="music" title="No {showFavourites ? 'favourite ' : ''}artists found" description={showFavourites ? 'Heart an artist to see them here.' : 'Sync your music library to see artists here.'} />
	{/if}
</div>
