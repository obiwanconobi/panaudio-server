<script lang="ts">
	import { onMount } from 'svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import EditModal from '$lib/components/ui/EditModal.svelte';
	import { fetchPlaylists, createPlaylist, deletePlaylist, uploadPlaylist } from '$lib/api';
	import type { Playlist } from '$lib/types';

	let playlists = $state<Playlist[]>([]);
	let loading = $state(true);
	let showCreate = $state(false);
	let newName = $state('');
	let uploading = $state(false);
	let fileInput: HTMLInputElement;

	onMount(async () => {
		try {
			playlists = await fetchPlaylists();
		} finally {
			loading = false;
		}
	});

	async function handleCreate() {
		if (!newName.trim()) return;
		await createPlaylist(newName.trim());
		playlists = await fetchPlaylists();
		showCreate = false;
		newName = '';
	}

	async function handleDelete(playlist: Playlist) {
		if (!confirm(`Delete playlist "${playlist.playlistName}"?`)) return;
		await deletePlaylist(playlist.playlistId);
		playlists = playlists.filter((p) => p.playlistId !== playlist.playlistId);
	}

	async function handleUpload() {
		const file = fileInput?.files?.[0];
		if (!file) return;
		uploading = true;
		try {
			await uploadPlaylist(file);
			playlists = await fetchPlaylists();
		} finally {
			uploading = false;
			fileInput.value = '';
		}
	}
</script>

<svelte:head>
	<title>Playlists — PanAudio</title>
</svelte:head>

<div>
	<div class="flex items-center justify-between mb-6">
		<h1 class="text-2xl font-bold">Playlists</h1>
		<div class="flex items-center gap-2">
			<input
				type="file"
				accept=".jspf"
				class="hidden"
				bind:this={fileInput}
				onchange={handleUpload}
			/>
			<button
				onclick={() => fileInput?.click()}
				disabled={uploading}
				class="flex items-center gap-1.5 px-4 py-2 bg-zinc-700 hover:bg-zinc-600 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50"
			>
				{#if uploading}
					<div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
				{:else}
					<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
					</svg>
				{/if}
				Upload Playlist
			</button>
			<button
				onclick={() => (showCreate = true)}
				class="flex items-center gap-1.5 px-4 py-2 bg-violet-600 hover:bg-violet-500 text-white rounded-lg text-sm font-medium transition-colors"
			>
				<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
				</svg>
				New Playlist
			</button>
		</div>
	</div>

	{#if loading}
		<div class="flex justify-center py-12">
			<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
		</div>
	{:else if playlists.length > 0}
		<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
			{#each playlists as playlist (playlist.playlistId)}
				<a
					href="/playlists/{playlist.playlistId}"
					class="group flex items-center justify-between p-4 bg-zinc-900 border border-zinc-800 rounded-lg hover:border-violet-600/50 transition-colors"
				>
					<div>
						<p class="font-medium group-hover:text-violet-300 transition-colors">{playlist.playlistName}</p>
					</div>
					<button
						onclick={(e) => { e.preventDefault(); handleDelete(playlist); }}
						class="p-1.5 text-zinc-600 hover:text-red-400 opacity-0 group-hover:opacity-100 transition-all"
						aria-label="Delete playlist"
					>
						<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
						</svg>
					</button>
				</a>
			{/each}
		</div>
	{:else}
		<EmptyState icon="playlist" title="No playlists" description="Create your first playlist." />
	{/if}

	{#if showCreate}
		<EditModal
			title="New Playlist"
			onsave={handleCreate}
			onclose={() => (showCreate = false)}
		>
			<label class="block">
				<span class="text-sm text-zinc-400">Name</span>
				<input type="text" bind:value={newName} class="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500" placeholder="My Playlist" />
			</label>
		</EditModal>
	{/if}
</div>
