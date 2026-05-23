export function formatDuration(length: string): string {
	const parts = length.split(':').map(Number);
	if (parts.length === 3) {
		const [h, m, s] = parts;
		return h > 0 ? `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}` : `${m}:${String(s).padStart(2, '0')}`;
	}
	if (parts.length === 2) {
		const [m, s] = parts;
		return `${m}:${String(s).padStart(2, '0')}`;
	}
	const secs = parseFloat(length);
	if (!isNaN(secs)) {
		const m = Math.floor(secs / 60);
		const s = Math.floor(secs % 60);
		return `${m}:${String(s).padStart(2, '0')}`;
	}
	return length;
}

export function formatSeconds(seconds: number): string {
	const m = Math.floor(seconds / 60);
	const s = Math.floor(seconds % 60);
	return `${m}:${String(s).padStart(2, '0')}`;
}
