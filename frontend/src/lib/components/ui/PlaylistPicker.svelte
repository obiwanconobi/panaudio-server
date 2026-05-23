<script lang="ts">
	import { fetchPlaylists, addSongToPlaylist, createPlaylist } from '$lib/api';
	import type { Song, Playlist } from '$lib/types';

	let {
		song,
		onclose
	}: {
		song: Song;
		onclose: () => void;
	} = $props();

	let playlists = $state<Playlist[]>([]);
	let loading = $state(true);
	let newPlaylistName = $state('');
	let adding = $state(false);

	async function loadPlaylists() {
		loading = true;
		try {
			playlists = await fetchPlaylists();
		} finally {
			loading = false;
		}
	}

	loadPlaylists();

	async function handleAddToPlaylist(playlist: Playlist) {
		if (adding) return;
		adding = true;
		try {
			await addSongToPlaylist(playlist.playlistId, song.id);
			onclose();
		} finally {
			adding = false;
		}
	}

	async function handleCreatePlaylist() {
		const name = newPlaylistName.trim();
		if (!name || adding) return;
		adding = true;
		try {
			await createPlaylist(name);
			newPlaylistName = '';
			await loadPlaylists();
		} finally {
			adding = false;
		}
	}
</script>

<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
	<div class="bg-zinc-900 border border-zinc-700 rounded-xl w-full max-w-md mx-4 shadow-2xl">
		<div class="flex items-center justify-between px-6 py-4 border-b border-zinc-800">
			<h2 class="text-lg font-semibold">Add to Playlist</h2>
			<button onclick={onclose} class="text-zinc-400 hover:text-zinc-200 p-1" aria-label="Close">
				<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
				</svg>
			</button>
		</div>

		<div class="px-6 py-4 space-y-3">
			<div class="flex gap-2">
				<input
					type="text"
					bind:value={newPlaylistName}
					placeholder="New playlist name..."
					class="flex-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500"
				/>
				<button
					onclick={handleCreatePlaylist}
					disabled={!newPlaylistName.trim() || adding}
					class="px-3 py-2 text-sm bg-violet-600 hover:bg-violet-500 text-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
				>
					Create
				</button>
			</div>

			{#if loading}
				<div class="flex justify-center py-4">
					<div class="w-6 h-6 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
				</div>
			{:else if playlists.length > 0}
				<div class="max-h-64 overflow-y-auto space-y-1">
					{#each playlists as playlist (playlist.playlistId)}
						<button
							onclick={() => handleAddToPlaylist(playlist)}
							disabled={adding}
							class="w-full flex items-center gap-3 px-3 py-2 text-left rounded-lg hover:bg-zinc-800 transition-colors disabled:opacity-50"
						>
							<svg class="w-4 h-4 text-violet-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
							</svg>
							<div class="min-w-0 flex-1">
								<p class="text-sm truncate">{playlist.playlistName}</p>
							</div>
						</button>
					{/each}
				</div>
			{:else}
				<p class="text-sm text-zinc-500 text-center py-4">No playlists yet. Create one above.</p>
			{/if}
		</div>
	</div>
</div>
