<script lang="ts">
	import type { Song } from '$lib/types';
	import { queue } from '$lib/stores/queue.svelte';
	import SongRow from './SongRow.svelte';

	let {
		songs,
		showAlbum = true,
		showArtist = true,
		onremoveSong
	}: {
		songs: Song[];
		showAlbum?: boolean;
		showArtist?: boolean;
		onremoveSong?: (song: Song) => void;
	} = $props();

	function playAll() {
		if (songs.length === 0) return;
		queue.clearQueue();
		queue.addToQueue(songs);
		queue.playIndex(0);
	}
</script>

<div>
	{#if songs.length > 0}
		<div class="flex items-center gap-2 mb-3">
			<button
				onclick={playAll}
				class="flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-violet-600 hover:bg-violet-500 text-white text-sm font-medium transition-colors"
			>
				<svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
					<path d="M5 3l14 9-14 9V3z" />
				</svg>
				Play All
			</button>
			<span class="text-xs text-zinc-500">{songs.length} songs</span>
		</div>
	{/if}

	<div class="divide-y divide-zinc-800/50">
		{#each songs as song, i (song.id)}
			<SongRow {song} index={i + 1} {showAlbum} {showArtist} onremove={onremoveSong ? () => onremoveSong(song) : undefined} />
		{/each}
	</div>
</div>
