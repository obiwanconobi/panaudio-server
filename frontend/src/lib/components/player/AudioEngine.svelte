<script lang="ts">
	import { player } from '$lib/stores/player.svelte';
	import { queue } from '$lib/stores/queue.svelte';
	import { audioStreamUrl, startPlayback } from '$lib/api';
	import { frontendConfig } from '$lib/stores/config.svelte';

	let audioElement: HTMLAudioElement | undefined;

	function onLoadedMetadata() {
		if (audioElement) {
			player.setDuration(audioElement.duration || 0);
		}
	}

	function onTimeUpdate() {
		if (audioElement) {
			player.setCurrentTime(audioElement.currentTime);
		}
	}

	function onEnded() {
		if (queue.repeat === 'one' && audioElement) {
			audioElement.currentTime = 0;
			audioElement.play();
			return;
		}
		const next = queue.playNext();
		if (next && audioElement) {
			audioElement.src = audioStreamUrl(next.id);
			audioElement.play();
		} else {
			player.setPlaying(false);
		}
	}

	function onError() {
		player.setLoading(false);
		player.setPlaying(false);
		const next = queue.playNext();
		if (next && audioElement) {
			audioElement.src = audioStreamUrl(next.id);
			audioElement.play();
		}
	}

	function onPlay() {
		player.setPlaying(true);
		player.setLoading(false);
		if (queue.currentSong && frontendConfig.enablePlaybackReporting) {
			startPlayback(queue.currentSong.id);
		}
	}

	function onPause() {
		player.setPlaying(false);
	}

	function onWaiting() {
		player.setLoading(true);
	}

	function onCanPlay() {
		player.setLoading(false);
	}

	$effect(() => {
		const song = queue.currentSong;
		if (!song || !audioElement) return;

		player.setLoading(true);
		audioElement.src = audioStreamUrl(song.id);
		audioElement.play().catch(() => {});
	});

	$effect(() => {
		if (audioElement) {
			audioElement.volume = player.volume;
		}
	});
</script>

<audio
	id="audio-engine"
	bind:this={audioElement}
	onloadedmetadata={onLoadedMetadata}
	ontimeupdate={onTimeUpdate}
	onended={onEnded}
	onerror={onError}
	onplay={onPlay}
	onpause={onPause}
	onwaiting={onWaiting}
	oncanplay={onCanPlay}
	preload="auto"
></audio>
