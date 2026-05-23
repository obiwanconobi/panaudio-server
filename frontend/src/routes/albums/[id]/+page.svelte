<script lang="ts">
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import TrackList from '$lib/components/music/TrackList.svelte';
	import FavouriteButton from '$lib/components/ui/FavouriteButton.svelte';
	import EditModal from '$lib/components/ui/EditModal.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';
	import { fetchAlbumById, fetchAlbumSongs, setAlbumFavourite, updateAlbum, deleteAlbum, albumArtUrl } from '$lib/api';
	import { avatarGradient } from '$lib/utils/avatar';
	import { goto } from '$app/navigation';
	import type { Album, Song } from '$lib/types';

	let album = $state<Album | null>(null);
	let songs = $state<Song[]>([]);
	let loading = $state(true);
	let showEdit = $state(false);
	let imageError = $state(false);
	let editTitle = $state('');
	let editArtist = $state('');
	let editYear = $state<number | undefined>();

	let albumId = $derived($page.params.id);

	onMount(async () => {
		try {
			[album, songs] = await Promise.all([
				fetchAlbumById(albumId),
				fetchAlbumSongs(albumId)
			]);
		} finally {
			loading = false;
		}
	});

	async function toggleFavourite() {
		if (!album) return;
		const newVal = !album.favourite;
		album.favourite = newVal;
		try {
			await setAlbumFavourite(album.id, newVal);
		} catch {
			album.favourite = !newVal;
		}
	}

	function openEdit() {
		if (!album) return;
		editTitle = album.title;
		editArtist = album.artist;
		editYear = album.year ?? undefined;
		showEdit = true;
	}

	async function saveEdit() {
		if (!album) return;
		await updateAlbum(album.id, { title: editTitle, artist: editArtist, year: editYear });
		album.title = editTitle;
		album.artist = editArtist;
		album.year = editYear ?? null;
		showEdit = false;
	}

	async function handleDelete() {
		if (!album) return;
		if (!confirm(`Delete album "${album.title}"?`)) return;
		await deleteAlbum(album.id);
		goto('/albums');
	}
</script>

<svelte:head>
	<title>{album?.title || 'Album'} — PanAudio</title>
</svelte:head>

{#if loading}
	<div class="flex justify-center py-12">
		<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
	</div>
{:else if album}
	<div class="flex flex-col md:flex-row gap-8">
		<div class="w-full md:w-72 shrink-0">
			{#if imageError}
				<div
					class="w-full aspect-square rounded-xl flex items-center justify-center"
					style="background: {avatarGradient(album.title)}"
				>
					<span class="text-xl font-bold text-white/80 drop-shadow-md text-center px-4 leading-tight">
						{album.title}
					</span>
				</div>
			{:else}
				<img src={albumArtUrl(album.id)} alt="{album.title} cover" class="w-full aspect-square object-cover rounded-xl shadow-2xl" onerror={() => (imageError = true)} />
			{/if}

			<div class="mt-4 space-y-1">
				<h1 class="text-xl font-bold">{album.title}</h1>
				<p class="text-zinc-400">{album.artist}</p>
				{#if album.year}<p class="text-sm text-zinc-500">{album.year}</p>{/if}
			</div>

			<div class="flex items-center gap-2 mt-4">
				<FavouriteButton favourite={album.favourite} ontoggle={toggleFavourite} />
				<button onclick={openEdit} class="px-3 py-1.5 text-sm bg-zinc-800 hover:bg-zinc-700 rounded-lg transition-colors">Edit</button>
				<button onclick={handleDelete} class="px-3 py-1.5 text-sm bg-red-900/30 hover:bg-red-900/50 text-red-400 rounded-lg transition-colors">Delete</button>
			</div>
		</div>

		<div class="flex-1">
			{#if songs.length > 0}
				<TrackList {songs} showAlbum={false} />
			{:else}
				<EmptyState icon="music" title="No tracks" description="This album has no songs." />
			{/if}
		</div>
	</div>

	{#if showEdit}
		<EditModal
			title="Edit Album"
			onsave={saveEdit}
			onclose={() => (showEdit = false)}
		>
			<div class="space-y-4">
				<label class="block">
					<span class="text-sm text-zinc-400">Title</span>
					<input type="text" bind:value={editTitle} class="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500" />
				</label>
				<label class="block">
					<span class="text-sm text-zinc-400">Artist</span>
					<input type="text" bind:value={editArtist} class="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500" />
				</label>
				<label class="block">
					<span class="text-sm text-zinc-400">Year</span>
					<input type="number" bind:value={editYear} class="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500" />
				</label>
			</div>
		</EditModal>
	{/if}
{:else}
	<EmptyState icon="music" title="Album not found" description="The requested album could not be loaded." />
{/if}
