<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import AlbumGrid from '$lib/components/music/AlbumGrid.svelte';
	import FavouriteButton from '$lib/components/ui/FavouriteButton.svelte';
	import EditModal from '$lib/components/ui/EditModal.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import { fetchArtistAlbums, setArtistFavourite, updateArtist, deleteArtist, artistArtUrl } from '$lib/api';
	import { library } from '$lib/stores/library.svelte';
	import { frontendConfig } from '$lib/stores/config.svelte';
	import { avatarGradient, artistInitials } from '$lib/utils/avatar';
	import { goto } from '$app/navigation';
	import type { Album, Artist } from '$lib/types';

	let artist = $state<Artist | null>(null);
	let albums = $state<Album[]>([]);
	let loading = $state(true);
	let showEdit = $state(false);
	let editName = $state('');

	let artistId = $derived($page.params.id);

	onMount(async () => {
		try {
			artist = library.getArtistById(artistId) ?? null;
			albums = await fetchArtistAlbums(artistId);
		} finally {
			loading = false;
		}
	});

	async function toggleFavourite() {
		if (!artist) return;
		const newVal = !artist.favourite;
		artist.favourite = newVal;
		try {
			await setArtistFavourite(artist.id, newVal);
		} catch {
			artist.favourite = !newVal;
		}
	}

	function openEdit() {
		if (!artist) return;
		editName = artist.name;
		showEdit = true;
	}

	async function saveEdit() {
		if (!artist) return;
		await updateArtist(artist.id, { name: editName });
		artist.name = editName;
		showEdit = false;
	}

	async function handleDelete() {
		if (!artist) return;
		if (!confirm(`Delete artist "${artist.name}"?`)) return;
		await deleteArtist(artist.id);
		goto('/artists');
	}
</script>

<svelte:head>
	<title>{artist?.name || 'Artist'} — PanAudio</title>
</svelte:head>

{#if loading}
	<div class="flex justify-center py-12">
		<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
	</div>
{:else if artist}
	<div>
		<div class="flex flex-col md:flex-row items-start gap-8 mb-8">
			<div class="w-48 h-48 rounded-full overflow-hidden bg-zinc-800 border-2 border-zinc-700 shrink-0">
				{#if frontendConfig.generatedArtistAvatars}
					<div
						class="w-full h-full flex items-center justify-center"
						style="background: {avatarGradient(artist.name)}"
					>
						<span class="text-5xl font-bold text-white drop-shadow-md">
							{artistInitials(artist.name)}
						</span>
					</div>
				{:else if artist.picture}
					<img src={artistArtUrl(artist.id)} alt="{artist.name}" class="w-full h-full object-cover" />
				{:else}
					<div class="w-full h-full flex items-center justify-center">
						<svg class="w-16 h-16 text-zinc-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
						</svg>
					</div>
				{/if}
			</div>

			<div>
				<h1 class="text-3xl font-bold">{artist.name}</h1>
				<p class="text-zinc-400 mt-1">{albums.length} albums</p>
				<div class="flex items-center gap-2 mt-4">
					<FavouriteButton favourite={artist.favourite} ontoggle={toggleFavourite} />
					<button onclick={openEdit} class="px-3 py-1.5 text-sm bg-zinc-800 hover:bg-zinc-700 rounded-lg transition-colors">Edit</button>
					<button onclick={handleDelete} class="px-3 py-1.5 text-sm bg-red-900/30 hover:bg-red-900/50 text-red-400 rounded-lg transition-colors">Delete</button>
				</div>
			</div>
		</div>

		<h2 class="text-xl font-bold mb-4">Albums</h2>
		{#if albums.length > 0}
			<AlbumGrid {albums} />
		{:else}
			<EmptyState icon="music" title="No albums" />
		{/if}
	</div>

	{#if showEdit}
		<EditModal
			title="Edit Artist"
			onsave={saveEdit}
			onclose={() => (showEdit = false)}
		>
			<label class="block">
				<span class="text-sm text-zinc-400">Name</span>
				<input type="text" bind:value={editName} class="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500" />
			</label>
		</EditModal>
	{/if}
{:else}
	<EmptyState icon="music" title="Artist not found" />
{/if}
