<script lang="ts">
	import { onMount } from 'svelte';
	import AlbumGrid from '$lib/components/music/AlbumGrid.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import { fetchAlbums, fetchFavouriteAlbums } from '$lib/api';
	import { library } from '$lib/stores/library.svelte';
	import type { Album } from '$lib/types';

	let allAlbums = $state<Album[]>([]);
	let favouriteAlbums = $state<Album[]>([]);
	let showFavourites = $state(false);
	let loading = $state(true);

	onMount(async () => {
		try {
			[allAlbums, favouriteAlbums] = await Promise.all([
				fetchAlbums(),
				fetchFavouriteAlbums()
			]);
			library.setAlbums(allAlbums);
		} finally {
			loading = false;
		}
	});

	let albums = $derived(showFavourites ? favouriteAlbums : allAlbums);
</script>

<svelte:head>
	<title>Albums — PanAudio</title>
</svelte:head>

<div>
	<div class="flex items-center justify-between mb-6">
		<h1 class="text-2xl font-bold">{showFavourites ? 'Favourite Albums' : 'Albums'}</h1>
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
	{:else if albums.length > 0}
		<AlbumGrid {albums} />
	{:else}
		<EmptyState icon="music" title="No {showFavourites ? 'favourite ' : ''}albums found" description={showFavourites ? 'Heart an album to see it here.' : 'Sync your music library to see albums here.'} />
	{/if}
</div>
