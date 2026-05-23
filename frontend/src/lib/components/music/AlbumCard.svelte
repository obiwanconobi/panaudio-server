<script lang="ts">
	import type { Album } from '$lib/types';
	import { albumArtUrl } from '$lib/api';
	import { avatarGradient } from '$lib/utils/avatar';

	let { album }: { album: Album } = $props();
	let imageError = $state(false);
</script>

<a
	href="/albums/{album.id}"
	class="group block rounded-lg overflow-hidden bg-zinc-900 border border-zinc-800 hover:border-zinc-700 transition-all duration-200 hover:scale-[1.02] hover:shadow-xl hover:shadow-violet-900/10"
>
	<div class="aspect-square overflow-hidden">
		{#if imageError}
			<div
				class="w-full h-full flex items-center justify-center"
				style="background: {avatarGradient(album.title)}"
			>
				<span class="text-sm font-bold text-white/80 drop-shadow-md text-center px-2 leading-tight">
					{album.title}
				</span>
			</div>
		{:else}
			<img
				src={albumArtUrl(album.id)}
				alt="{album.title} cover"
				class="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
				loading="lazy"
				onerror={() => (imageError = true)}
			/>
		{/if}
	</div>
	<div class="p-3">
		<p class="text-sm font-medium truncate group-hover:text-violet-300 transition-colors">{album.title}</p>
		<p class="text-xs text-zinc-400 truncate mt-0.5">{album.artist}</p>
	</div>
</a>
