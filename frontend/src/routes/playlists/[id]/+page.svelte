<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import TrackList from '$lib/components/music/TrackList.svelte';
	import EditModal from '$lib/components/ui/EditModal.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import { fetchSongs, fetchPlaylist, addSongToPlaylist, removeSongFromPlaylist, deletePlaylist } from '$lib/api';
	import { goto } from '$app/navigation';
	import type { Playlist, Song } from '$lib/types';

	let playlist = $state<Playlist | null>(null);
	let allSongs = $state<Song[]>([]);
	let loading = $state(true);
	let showAddSong = $state(false);
	let searchFilter = $state('');

	let playlistId = $derived($page.params.id);

	onMount(async () => {
		try {
			[playlist, allSongs] = await Promise.all([
				fetchPlaylist(playlistId),
				fetchSongs()
			]);
		} finally {
			loading = false;
		}
	});

	async function handleAddSong(song: Song) {
		await addSongToPlaylist(playlistId, song.id);
		playlist = await fetchPlaylist(playlistId);
	}

	async function handleRemoveSong(song: Song) {
		await removeSongFromPlaylist(playlistId, song.id);
		playlist = await fetchPlaylist(playlistId);
	}

	async function handleDelete() {
		if (!playlist) return;
		if (!confirm(`Delete playlist "${playlist.playlistName}"?`)) return;
		await deletePlaylist(playlist.playlistId);
		goto('/playlists');
	}

	let playlistSongIds = $derived(new Set(playlist?.playlistItems?.map((pi) => pi.song.id) ?? []));
	let availableSongs = $derived(
		allSongs.filter((s) => !playlistSongIds.has(s.id) && (!searchFilter || s.title.toLowerCase().includes(searchFilter.toLowerCase()) || s.artist.toLowerCase().includes(searchFilter.toLowerCase())))
	);
</script>

<svelte:head>
	<title>{playlist?.playlistName || 'Playlist'} — PanAudio</title>
</svelte:head>

{#if loading}
	<div class="flex justify-center py-12">
		<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
	</div>
{:else if playlist}
	<div>
		<div class="flex items-center justify-between mb-6">
			<div>
				<h1 class="text-2xl font-bold">{playlist.playlistName}</h1>
				<p class="text-sm text-zinc-400">{playlist.playlistItems?.length || 0} songs</p>
			</div>
			<div class="flex gap-2">
				<button
					onclick={() => (showAddSong = true)}
					class="px-3 py-1.5 text-sm bg-violet-600 hover:bg-violet-500 text-white rounded-lg transition-colors"
				>
					Add Songs
				</button>
				<button
					onclick={handleDelete}
					class="px-3 py-1.5 text-sm bg-red-900/30 hover:bg-red-900/50 text-red-400 rounded-lg transition-colors"
				>
					Delete
				</button>
			</div>
		</div>

		{#if playlist.playlistItems && playlist.playlistItems.length > 0}
			<TrackList songs={playlist.playlistItems.map((pi) => pi.song)} onremoveSong={handleRemoveSong} />
		{:else}
			<EmptyState icon="playlist" title="Empty playlist" description="Add some songs to get started." />
		{/if}
	</div>

	{#if showAddSong}
		<EditModal
			title="Add Songs"
			onsave={() => (showAddSong = false)}
			onclose={() => (showAddSong = false)}
		>
			<div class="space-y-3">
				<input
					type="text"
					bind:value={searchFilter}
					placeholder="Filter songs..."
					class="w-full px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500"
				/>
				<div class="max-h-64 overflow-y-auto space-y-1">
					{#each availableSongs as song (song.id)}
						<button
							onclick={() => handleAddSong(song)}
							class="w-full flex items-center gap-3 px-3 py-2 text-left rounded-lg hover:bg-zinc-800 transition-colors"
						>
							<div class="min-w-0 flex-1">
								<p class="text-sm truncate">{song.title}</p>
								<p class="text-xs text-zinc-500 truncate">{song.artist} &middot; {song.album}</p>
							</div>
							<svg class="w-4 h-4 text-violet-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
							</svg>
						</button>
					{/each}
				</div>
			</div>
		</EditModal>
	{/if}
{:else}
	<EmptyState icon="playlist" title="Playlist not found" />
{/if}
