<script lang="ts">
	import type { Album } from '$lib/types';
	import { albumArtUrl } from '$lib/api';

	let { album }: { album: Album } = $props();
</script>

<a
	href="/albums/{album.id}"
	class="group block rounded-lg overflow-hidden bg-zinc-900 border border-zinc-800 hover:border-zinc-700 transition-all duration-200 hover:scale-[1.02] hover:shadow-xl hover:shadow-violet-900/10"
>
	<div class="aspect-square overflow-hidden">
		{#if album.picture}
			<img
				src={albumArtUrl(album.id)}
				alt="{album.title} cover"
				class="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
				loading="lazy"
			/>
		{:else}
			<div class="w-full h-full bg-zinc-800 flex items-center justify-center">
				<svg class="w-12 h-12 text-zinc-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3" />
				</svg>
			</div>
		{/if}
	</div>
	<div class="p-3">
		<p class="text-sm font-medium truncate group-hover:text-violet-300 transition-colors">{album.title}</p>
		<p class="text-xs text-zinc-400 truncate mt-0.5">{album.artist}</p>
	</div>
</a>
