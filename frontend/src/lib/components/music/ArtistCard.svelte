<script lang="ts">
	import type { Artist } from '$lib/types';
	import { artistArtUrl } from '$lib/api';
	import { frontendConfig } from '$lib/stores/config.svelte';
	import { avatarGradient, artistInitials } from '$lib/utils/avatar';

	let { artist }: { artist: Artist } = $props();
</script>

<a
	href="/artists/{artist.id}"
	class="group flex flex-col items-center gap-3 p-4 rounded-lg hover:bg-zinc-800/50 transition-colors"
>
	<div class="w-32 h-32 rounded-full overflow-hidden border-2 border-zinc-700 group-hover:border-violet-600 transition-colors">
		{#if frontendConfig.generatedArtistAvatars}
			<div
				class="w-full h-full flex items-center justify-center"
				style="background: {avatarGradient(artist.name)}"
			>
				<span class="text-3xl font-bold text-white drop-shadow-md">
					{artistInitials(artist.name)}
				</span>
			</div>
		{:else if artist.picture}
			<img
				src={artistArtUrl(artist.id)}
				alt="{artist.name}"
				class="w-full h-full object-cover"
				loading="lazy"
			/>
		{:else}
			<div class="w-full h-full bg-zinc-800 flex items-center justify-center">
				<svg class="w-10 h-10 text-zinc-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
				</svg>
			</div>
		{/if}
	</div>
	<p class="text-sm font-medium text-center truncate w-full group-hover:text-violet-300 transition-colors">
		{artist.name}
	</p>
</a>
