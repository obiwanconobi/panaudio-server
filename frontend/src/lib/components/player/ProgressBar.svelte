<script lang="ts">
	import { player } from '$lib/stores/player.svelte';

	let seeking = $state(false);
	let seekPosition = $state(0);

	function handleInput(e: Event) {
		const target = e.target as HTMLInputElement;
		seekPosition = parseFloat(target.value);
	}

	function handleSeekStart() {
		seeking = true;
		seekPosition = player.currentTime;
	}

	function handleSeekEnd() {
		seeking = false;
		const audio = document.querySelector<HTMLAudioElement>('#audio-engine');
		if (audio) {
			audio.currentTime = seekPosition;
		}
		player.setCurrentTime(seekPosition);
	}
</script>

<div class="relative h-1 bg-zinc-800 group cursor-pointer">
	<div
		class="absolute inset-y-0 left-0 bg-violet-500 group-hover:bg-violet-400 transition-colors"
		style="width: {player.progress * 100}%"
	></div>
	<input
		type="range"
		min="0"
		max={player.duration || 0}
		step="0.1"
		value={seeking ? seekPosition : player.currentTime}
		oninput={handleInput}
		onpointerdown={handleSeekStart}
		onpointerup={handleSeekEnd}
		class="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10"
	/>
</div>
