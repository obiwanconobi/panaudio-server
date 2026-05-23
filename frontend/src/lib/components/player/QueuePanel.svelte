<script lang="ts">
	import { queue } from '$lib/stores/queue.svelte';
	import { formatDuration } from '$lib/utils/format';

	let { onclose } = $props<{ onclose: () => void }>();
</script>

<div class="absolute bottom-full left-0 right-0 bg-zinc-900 border-t border-zinc-800 max-h-80 overflow-y-auto">
	<div class="flex items-center justify-between px-4 py-2 border-b border-zinc-800">
		<h3 class="text-sm font-medium">Queue</h3>
		<button onclick={onclose} class="text-zinc-400 hover:text-zinc-200" aria-label="Close queue">
			<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
			</svg>
		</button>
	</div>

	{#if queue.items.length === 0}
		<p class="px-4 py-8 text-center text-zinc-500 text-sm">Queue is empty</p>
	{:else}
		<ul>
			{#each queue.items as song, index}
				<li
					class="flex items-center gap-3 px-4 py-2 hover:bg-zinc-800/50 transition-colors
						{index === queue.currentIndex ? 'bg-violet-600/10' : ''}"
				>
					<span class="text-xs text-zinc-500 w-6 text-right">{index + 1}</span>
					{#if song.albumPicture}
						<img
							src="/api/albumArt?albumId={song.albumId}"
							alt=""
							class="w-8 h-8 rounded object-cover shrink-0"
						/>
					{/if}
					<div class="min-w-0 flex-1">
						<p class="text-sm truncate {index === queue.currentIndex ? 'text-violet-300' : ''}">
							{song.title}
						</p>
						<p class="text-xs text-zinc-500 truncate">{song.artist}</p>
					</div>
					<span class="text-xs text-zinc-500">{formatDuration(song.length)}</span>
					<button
						onclick={() => queue.removeFromQueue(index)}
						class="text-zinc-500 hover:text-red-400 p-1"
						aria-label="Remove from queue"
					>
						<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
						</svg>
					</button>
					<button
						onclick={() => { queue.playIndex(index); onclose(); }}
						class="text-zinc-400 hover:text-zinc-200 p-1"
						aria-label="Play"
					>
						<svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
							<path d="M5 3l14 9-14 9V3z" />
						</svg>
					</button>
				</li>
			{/each}
		</ul>
	{/if}
</div>
