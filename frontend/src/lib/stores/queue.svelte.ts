import type { Song } from '../types';

type RepeatMode = 'off' | 'one' | 'all';

let items = $state<Song[]>([]);
let currentIndex = $state(-1);
let shuffle = $state(false);
let repeat = $state<RepeatMode>('off');

let shuffledOrder = $state<number[]>([]);

function buildShuffledOrder(): number[] {
	const indices = items.map((_, i) => i);
	for (let i = indices.length - 1; i > 0; i--) {
		const j = Math.floor(Math.random() * (i + 1));
		[indices[i], indices[j]] = [indices[j], indices[i]];
	}
	return indices;
}

export const queue = {
	get items() { return items; },
	get currentIndex() { return currentIndex; },
	get currentSong(): Song | null {
		return items[currentIndex] ?? null;
	},
	get shuffle() { return shuffle; },
	get repeat() { return repeat; },

	addToQueue(songs: Song[]) {
		items.push(...songs);
		if (currentIndex === -1) currentIndex = 0;
		if (shuffle) shuffledOrder = buildShuffledOrder();
	},

	addToQueueNext(song: Song) {
		items.splice(currentIndex + 1, 0, song);
		if (shuffle) shuffledOrder = buildShuffledOrder();
	},

	removeFromQueue(index: number) {
		if (index < 0 || index >= items.length) return;
		items.splice(index, 1);
		if (index < currentIndex) currentIndex--;
		if (currentIndex >= items.length) currentIndex = items.length - 1;
		if (shuffle) shuffledOrder = buildShuffledOrder();
	},

	reorder(from: number, to: number) {
		const [moved] = items.splice(from, 1);
		items.splice(to, 0, moved);
		if (from === currentIndex) {
			currentIndex = to;
		} else if (from < currentIndex && to >= currentIndex) {
			currentIndex--;
		} else if (from > currentIndex && to <= currentIndex) {
			currentIndex++;
		}
		if (shuffle) shuffledOrder = buildShuffledOrder();
	},

	playIndex(index: number) {
		if (index < 0 || index >= items.length) return null;
		currentIndex = index;
		return items[index];
	},

	playNext(): Song | null {
		if (items.length === 0) return null;
		if (shuffle) {
			const currentShufflePos = shuffledOrder.indexOf(currentIndex);
			const nextPos = (currentShufflePos + 1) % shuffledOrder.length;
			if (nextPos === 0 && repeat !== 'all') return null;
			return this.playIndex(shuffledOrder[nextPos]);
		}
		const next = currentIndex + 1;
		if (next >= items.length) {
			if (repeat === 'all') return this.playIndex(0);
			return null;
		}
		return this.playIndex(next);
	},

	playPrevious(): Song | null {
		if (items.length === 0) return null;
		if (shuffle) {
			const currentShufflePos = shuffledOrder.indexOf(currentIndex);
			const prevPos = (currentShufflePos - 1 + shuffledOrder.length) % shuffledOrder.length;
			return this.playIndex(shuffledOrder[prevPos]);
		}
		const prev = currentIndex - 1;
		if (prev < 0) {
			if (repeat === 'all') return this.playIndex(items.length - 1);
			return null;
		}
		return this.playIndex(prev);
	},

	setShuffle(value: boolean) {
		shuffle = value;
		if (value) shuffledOrder = buildShuffledOrder();
	},

	toggleShuffle() { this.setShuffle(!shuffle); },

	cycleRepeat() {
		if (repeat === 'off') repeat = 'all';
		else if (repeat === 'all') repeat = 'one';
		else repeat = 'off';
	},

	clearQueue() {
		items = [];
		currentIndex = -1;
		shuffledOrder = [];
	}
};
