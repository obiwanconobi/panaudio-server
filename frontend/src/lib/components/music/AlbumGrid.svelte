<script lang="ts">
	import type { Album } from '$lib/types';
	import AlbumCard from './AlbumCard.svelte';

	let { albums }: { albums: Album[] } = $props();
</script>

<div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4">
	{#each albums as album (album.id)}
		<AlbumCard {album} />
	{/each}
</div>
