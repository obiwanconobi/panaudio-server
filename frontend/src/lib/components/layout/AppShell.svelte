<script lang="ts">
	import { onMount } from 'svelte';
	import AudioEngine from '$lib/components/player/AudioEngine.svelte';
	import Sidebar from './Sidebar.svelte';
	import PlayerBar from './PlayerBar.svelte';
	import { frontendConfig } from '$lib/stores/config.svelte';

	let { children } = $props();

	onMount(() => {
		frontendConfig.load();
	});
</script>

<AudioEngine />

<div class="flex h-screen overflow-hidden">
	<Sidebar />
	<main class="flex-1 overflow-y-auto pb-24 px-4 md:px-8 pt-4">
		{@render children()}
	</main>
</div>

<PlayerBar />
