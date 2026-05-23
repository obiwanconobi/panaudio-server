<script lang="ts">
	import { onMount } from 'svelte';
	import TrackList from '$lib/components/music/TrackList.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import { fetchSongs } from '$lib/api';
	import { library } from '$lib/stores/library.svelte';
	import type { Song } from '$lib/types';

	let songs = $state<Song[]>([]);
	let loading = $state(true);

	onMount(async () => {
		try {
			songs = await fetchSongs();
			library.setSongs(songs);
		} finally {
			loading = false;
		}
	});
</script>

<svelte:head>
	<title>Songs — PanAudio</title>
</svelte:head>

<div>
	<h1 class="text-2xl font-bold mb-6">Songs</h1>

	{#if loading}
		<div class="flex justify-center py-12">
			<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
		</div>
	{:else if songs.length > 0}
		<TrackList {songs} />
	{:else}
		<EmptyState icon="music" title="No songs found" description="Sync your music library to see songs here." />
	{/if}
</div>
