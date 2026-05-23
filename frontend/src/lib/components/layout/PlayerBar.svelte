<script lang="ts">
	import { player } from '$lib/stores/player.svelte';
	import { queue } from '$lib/stores/queue.svelte';
	import ProgressBar from '../player/ProgressBar.svelte';
	import VolumeControl from '../player/VolumeControl.svelte';
	import QueuePanel from '../player/QueuePanel.svelte';
	import { audioStreamUrl } from '$lib/api';
	import { formatSeconds } from '$lib/utils/format';

	let audioElement: HTMLAudioElement | undefined;
	let showQueue = $state(false);

	function handlePlayPause() {
		if (!audioElement) return;
		if (player.isPlaying) {
			audioElement.pause();
		} else {
			audioElement.play();
		}
		player.togglePlay();
	}

	function handlePrevious() {
		const song = queue.playPrevious();
		if (song && audioElement) {
			audioElement.src = audioStreamUrl(song.id);
			audioElement.play();
		}
	}

	function handleNext() {
		const song = queue.playNext();
		if (song && audioElement) {
			audioElement.src = audioStreamUrl(song.id);
			audioElement.play();
		}
	}

	$effect(() => {
		const el = document.querySelector<HTMLAudioElement>('#audio-engine');
		if (el) audioElement = el;
	});
</script>

{#if queue.currentSong}
	<div class="fixed bottom-0 left-0 right-0 bg-zinc-900/95 backdrop-blur border-t border-zinc-800 z-50">
		<ProgressBar />

		<div class="flex items-center gap-4 px-4 py-2 h-16">
			<div class="flex items-center gap-3 w-56 min-w-0">
				{#if queue.currentSong.albumPicture}
					<img
						src="/api/albumArt?albumId={queue.currentSong.albumId}"
						alt=""
						class="w-10 h-10 rounded object-cover shrink-0"
					/>
				{/if}
				<div class="min-w-0">
					<p class="text-sm font-medium truncate">{queue.currentSong.title}</p>
					<p class="text-xs text-zinc-400 truncate">{queue.currentSong.artist}</p>
				</div>
			</div>

			<div class="flex-1 flex items-center justify-center gap-4">
				<button
					onclick={() => queue.setShuffle(!queue.shuffle)}
					class="p-2 rounded-full {queue.shuffle ? 'text-violet-400' : 'text-zinc-400'} hover:text-zinc-200"
					aria-label="Shuffle"
				>
					<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
					</svg>
				</button>

				<button onclick={handlePrevious} class="p-2 text-zinc-400 hover:text-zinc-200" aria-label="Previous">
					<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
						<path d="M9.195 18.44c1.25.713 2.805-.19 2.805-1.629v-2.34l6.945 3.968c1.25.714 2.805-.188 2.805-1.628v-8.56c0-1.44-1.555-2.342-2.805-1.628L12 10.44v-2.34c0-1.44-1.555-2.343-2.805-1.629l-7.108 4.062c-1.26.72-1.26 2.536 0 3.256l7.108 4.061z" />
					</svg>
				</button>

				<button
					onclick={handlePlayPause}
					class="p-3 rounded-full bg-violet-600 hover:bg-violet-500 text-white"
					aria-label={player.isPlaying ? 'Pause' : 'Play'}
				>
					{#if player.isPlaying}
						<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
							<path d="M6 4h4v16H6V4zm8 0h4v16h-4V4z" />
						</svg>
					{:else}
						<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
							<path d="M5 3l14 9-14 9V3z" />
						</svg>
					{/if}
				</button>

				<button onclick={handleNext} class="p-2 text-zinc-400 hover:text-zinc-200" aria-label="Next">
					<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
						<path d="M5.055 5.56c-1.25-.713-2.805.19-2.805 1.629v8.56c0 1.44 1.555 2.342 2.805 1.628L12 13.56v2.34c0 1.44 1.555 2.342 2.805 1.628l7.108-4.061c1.26-.72 1.26-2.536 0-3.256l-7.108-4.061C13.555 5.44 12 6.343 12 7.78v2.34L5.055 5.56z" />
					</svg>
				</button>

				<button
					onclick={() => queue.cycleRepeat()}
					class="p-2 rounded-full {queue.repeat !== 'off' ? 'text-violet-400' : 'text-zinc-400'} hover:text-zinc-200"
					aria-label="Repeat: {queue.repeat}"
				>
					<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
					</svg>
					{#if queue.repeat === 'one'}
						<span class="absolute text-[8px] font-bold -mt-1 ml-0.5">1</span>
					{/if}
				</button>
			</div>

			<div class="flex items-center gap-3 w-56 justify-end">
				<span class="text-xs text-zinc-400 tabular-nums w-16 text-right">
					{formatSeconds(player.currentTime)} / {formatSeconds(player.duration)}
				</span>
				<VolumeControl />
				<button
					onclick={() => (showQueue = !showQueue)}
					class="p-2 text-zinc-400 hover:text-zinc-200"
					aria-label="Queue"
				>
					<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16" />
					</svg>
				</button>
			</div>
		</div>

		{#if showQueue}
			<QueuePanel onclose={() => (showQueue = false)} />
		{/if}
	</div>
{/if}
