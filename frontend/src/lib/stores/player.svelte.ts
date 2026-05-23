let isPlaying = $state(false);
let currentTime = $state(0);
let duration = $state(0);
let volume = $state(0.8);
let isMuted = $state(false);
let isLoading = $state(false);

let previousVolume = 0.8;

export const player = {
	get isPlaying() { return isPlaying; },
	get currentTime() { return currentTime; },
	get duration() { return duration; },
	get volume() { return volume; },
	get isMuted() { return isMuted; },
	get isLoading() { return isLoading; },
	get progress() { return duration > 0 ? currentTime / duration : 0; },

	setPlaying(value: boolean) { isPlaying = value; },
	setCurrentTime(value: number) { currentTime = value; },
	setDuration(value: number) { duration = value; },
	setVolume(value: number) {
		volume = value;
		if (value > 0) isMuted = false;
	},

	toggleMute() {
		if (isMuted) {
			isMuted = false;
			volume = previousVolume;
		} else {
			previousVolume = volume;
			isMuted = true;
			volume = 0;
		}
	},

	togglePlay() { isPlaying = !isPlaying; },
	setLoading(value: boolean) { isLoading = value; }
};
