<script lang="ts">
	import { onMount } from 'svelte';
	import {
		fetchPlaybackTimeConfig,
		setPlaybackTimeConfig,
		fetchArtistPictureConfig,
		setArtistPictureConfig,
		fetchTagWritingConfig,
		setTagWritingConfig,
		fetchListenBrainzToken,
		setListenBrainzToken
	} from '$lib/api';

	let playbackTime = $state(0);
	let artistPictures = $state(false);
	let tagWriting = $state(false);
	let loading = $state(true);
	let saving = $state<string | null>(null);
	let listenbrainzToken = $state('');

	onMount(async () => {
		try {
			[playbackTime, artistPictures, tagWriting, listenbrainzToken] = await Promise.all([
				fetchPlaybackTimeConfig(),
				fetchArtistPictureConfig(),
				fetchTagWritingConfig(),
				fetchListenBrainzToken()
			]);
		} finally {
			loading = false;
		}
	});

	async function savePlaybackTime() {
		saving = 'playback';
		await setPlaybackTimeConfig(playbackTime);
		saving = null;
	}

	async function toggleArtistPictures() {
		artistPictures = !artistPictures;
		saving = 'artist';
		await setArtistPictureConfig(artistPictures);
		saving = null;
	}

	async function toggleTagWriting() {
		tagWriting = !tagWriting;
		saving = 'tag';
		await setTagWritingConfig(tagWriting);
		saving = null;
	}

	async function saveListenBrainzToken() {
		saving = 'token';
		await setListenBrainzToken(listenbrainzToken);
		saving = null;
	}
</script>

<svelte:head>
	<title>Settings — PanAudio</title>
</svelte:head>

<div class="max-w-2xl">
	<h1 class="text-2xl font-bold mb-8">Settings</h1>

	{#if loading}
		<div class="flex justify-center py-12">
			<div class="w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full animate-spin"></div>
		</div>
	{:else}
		<div class="space-y-6">
			<div class="p-6 bg-zinc-900 border border-zinc-800 rounded-xl">
				<div class="flex items-center justify-between">
					<div>
						<h3 class="font-medium">Playback Reporting Time</h3>
						<p class="text-sm text-zinc-400 mt-1">Minimum seconds before a play counts</p>
					</div>
					<div class="flex items-center gap-2">
						<input
							type="number"
							bind:value={playbackTime}
							min="0"
							class="w-20 px-3 py-1.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm text-center focus:outline-none focus:border-violet-500"
						/>
						<button
							onclick={savePlaybackTime}
							disabled={saving === 'playback'}
							class="px-3 py-1.5 text-sm bg-violet-600 hover:bg-violet-500 disabled:opacity-50 text-white rounded-lg transition-colors"
						>
							{saving === 'playback' ? '...' : 'Save'}
						</button>
					</div>
				</div>
			</div>

			<div class="p-6 bg-zinc-900 border border-zinc-800 rounded-xl">
				<div class="flex items-center justify-between">
					<div>
						<h3 class="font-medium">Artist Pictures</h3>
						<p class="text-sm text-zinc-400 mt-1">Fetch artist images from MusicBrainz</p>
					</div>
					<button
						onclick={toggleArtistPictures}
						disabled={saving === 'artist'}
						class="relative w-11 h-6 rounded-full transition-colors {artistPictures ? 'bg-violet-600' : 'bg-zinc-700'}"
						aria-label="Toggle artist pictures"
					>
						<span class="absolute top-0.5 left-0.5 w-5 h-5 rounded-full bg-white transition-transform {artistPictures ? 'translate-x-5' : ''}"></span>
					</button>
				</div>
			</div>

			<div class="p-6 bg-zinc-900 border border-zinc-800 rounded-xl">
				<div class="flex items-center justify-between">
					<div>
						<h3 class="font-medium">Tag Writing</h3>
						<p class="text-sm text-zinc-400 mt-1">Write metadata edits back to audio files</p>
					</div>
					<button
						onclick={toggleTagWriting}
						disabled={saving === 'tag'}
						class="relative w-11 h-6 rounded-full transition-colors {tagWriting ? 'bg-violet-600' : 'bg-zinc-700'}"
						aria-label="Toggle tag writing"
					>
						<span class="absolute top-0.5 left-0.5 w-5 h-5 rounded-full bg-white transition-transform {tagWriting ? 'translate-x-5' : ''}"></span>
					</button>
				</div>
			</div>

			<div class="p-6 bg-zinc-900 border border-zinc-800 rounded-xl">
				<div class="flex items-center justify-between">
					<div>
						<h3 class="font-medium">ListenBrainz Token</h3>
						<p class="text-sm text-zinc-400 mt-1">Submit listens to ListenBrainz</p>
					</div>
					<div class="flex items-center gap-2">
						<input
							type="password"
							bind:value={listenbrainzToken}
							placeholder="Token..."
							class="w-48 px-3 py-1.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm focus:outline-none focus:border-violet-500"
						/>
						<button
							onclick={saveListenBrainzToken}
							disabled={saving === 'token'}
							class="px-3 py-1.5 text-sm bg-violet-600 hover:bg-violet-500 disabled:opacity-50 text-white rounded-lg transition-colors"
						>
							{saving === 'token' ? '...' : 'Save'}
						</button>
					</div>
				</div>
			</div>
		</div>
	{/if}
</div>
