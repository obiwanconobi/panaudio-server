<script lang="ts">
	import type { Song } from '$lib/types';
	import { queue } from '$lib/stores/queue.svelte';
	import { setFavourite } from '$lib/api';
	import { formatDuration } from '$lib/utils/format';
	import PlaylistPicker from '$lib/components/ui/PlaylistPicker.svelte';

	let {
		song,
		index,
		showAlbum = false,
		showArtist = true,
		onremove
	}: {
		song: Song;
		index?: number;
		showAlbum?: boolean;
		showArtist?: boolean;
		onremove?: () => void;
	} = $props();

	let showPlaylistPicker = $state(false);

	function handlePlay() {
		queue.clearQueue();
		queue.addToQueue([song]);
	}

	function handleAddToQueue() {
		queue.addToQueue([song]);
	}

	async function toggleFavourite() {
		const newVal = !song.favourite;
		song.favourite = newVal;
		try {
			await setFavourite(song.id, newVal);
		} catch {
			song.favourite = !newVal;
		}
	}
</script>

<div class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-zinc-800/50 group transition-colors">
	{#if index !== undefined}
		<span class="w-6 text-xs text-zinc-500 text-right shrink-0">{index}</span>
	{/if}

	<div class="flex-1 min-w-0">
		<p class="text-sm truncate group-hover:text-violet-300 transition-colors">{song.title}</p>
		<div class="flex gap-2 text-xs text-zinc-500">
			{#if showArtist}
				<a href="/artists/{song.artistId}" class="hover:text-zinc-300 transition-colors">{song.artist}</a>
			{/if}
			{#if showAlbum && song.album}
				{#if showArtist}<span>&middot;</span>{/if}
				<a href="/albums/{song.albumId}" class="hover:text-zinc-300 transition-colors">{song.album}</a>
			{/if}
		</div>
	</div>

	<span class="text-xs text-zinc-500 w-12 text-right shrink-0">{formatDuration(song.length)}</span>
	<span class="text-xs text-zinc-600 w-8 text-right shrink-0">{song.playCount || 0}</span>

	<div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
		<button
			onclick={handlePlay}
			class="p-1.5 rounded-full bg-violet-600 hover:bg-violet-500 text-white"
			aria-label="Play {song.title}"
		>
			<svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24">
				<path d="M5 3l14 9-14 9V3z" />
			</svg>
		</button>

		<button
			onclick={() => (showPlaylistPicker = true)}
			class="p-1.5 text-zinc-500 hover:text-zinc-200"
			aria-label="Add to playlist"
		>
			<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16m-7 6h7" />
			</svg>
		</button>

		<button
			onclick={handleAddToQueue}
			class="p-1.5 text-zinc-500 hover:text-zinc-200"
			aria-label="Add to queue"
		>
			<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
			</svg>
		</button>

		<button
			onclick={toggleFavourite}
			class="p-1.5 {song.favourite ? 'text-red-400' : 'text-zinc-500'} hover:text-red-400"
			aria-label={song.favourite ? 'Unfavourite' : 'Favourite'}
		>
			<svg class="w-3.5 h-3.5" fill={song.favourite ? 'currentColor' : 'none'} stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
			</svg>
		</button>

		{#if onremove}
			<button
				onclick={onremove}
				class="p-1.5 text-zinc-500 hover:text-red-400"
				aria-label="Remove from playlist"
			>
				<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
				</svg>
			</button>
		{/if}
	</div>
</div>

{#if showPlaylistPicker}
	<PlaylistPicker {song} onclose={() => (showPlaylistPicker = false)} />
{/if}
