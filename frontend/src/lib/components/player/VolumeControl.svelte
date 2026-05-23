<script lang="ts">
	import { player } from '$lib/stores/player.svelte';

	let showSlider = $state(false);

	function handleInput(e: Event) {
		const target = e.target as HTMLInputElement;
		player.setVolume(parseFloat(target.value));
	}
</script>

<div
	class="relative flex items-center gap-2"
	onmouseenter={() => (showSlider = true)}
	onmouseleave={() => (showSlider = false)}
	role="group"
	aria-label="Volume control"
>
	<button
		onclick={() => player.toggleMute()}
		class="p-1 text-zinc-400 hover:text-zinc-200"
		aria-label={player.isMuted ? 'Unmute' : 'Mute'}
	>
		{#if player.isMuted || player.volume === 0}
			<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
				<path d="M5.586 15H4a1 1 0 01-1-1v-4a1 1 0 011-1h1.586l4.707-4.707C10.923 3.663 12 4.109 12 5v14c0 .891-1.077 1.337-1.707.707L5.586 15z" />
				<path stroke="currentColor" stroke-linecap="round" stroke-width="2" d="M17 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2" />
			</svg>
		{:else if player.volume < 0.5}
			<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
				<path d="M5.586 15H4a1 1 0 01-1-1v-4a1 1 0 011-1h1.586l4.707-4.707C10.923 3.663 12 4.109 12 5v14c0 .891-1.077 1.337-1.707.707L5.586 15z" />
				<path d="M17.657 16.657A8 8 0 0020 12a8 8 0 00-2.343-4.657" stroke="currentColor" stroke-linecap="round" fill="none" stroke-width="2" />
			</svg>
		{:else}
			<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
				<path d="M5.586 15H4a1 1 0 01-1-1v-4a1 1 0 011-1h1.586l4.707-4.707C10.923 3.663 12 4.109 12 5v14c0 .891-1.077 1.337-1.707.707L5.586 15z" />
				<path d="M17.657 16.657A8 8 0 0020 12a8 8 0 00-2.343-4.657" stroke="currentColor" stroke-linecap="round" fill="none" stroke-width="2" />
				<path d="M15.536 12a4 4 0 00-1.172-2.828" stroke="currentColor" stroke-linecap="round" fill="none" stroke-width="2" />
			</svg>
		{/if}
	</button>

	<div class="relative w-24 h-6 flex items-center {showSlider ? 'opacity-100' : 'opacity-0 md:opacity-100'} transition-opacity">
		<input
			type="range"
			min="0"
			max="1"
			step="0.01"
			value={player.isMuted ? 0 : player.volume}
			oninput={handleInput}
			class="w-full h-1 bg-zinc-700 rounded-full appearance-none cursor-pointer
				[&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:w-3 [&::-webkit-slider-thumb]:h-3
				[&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:bg-violet-400"
		/>
	</div>
</div>
