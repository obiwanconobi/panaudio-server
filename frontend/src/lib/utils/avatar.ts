const palette = [
	[0x9a, 0x9b, 0xc3],
	[0xfd, 0xd0, 0xff],
	[0xd0, 0xff, 0xfd],
	[0x98, 0xff, 0xcc],
	[0xf4, 0xc3, 0xd8],
	[0xc7, 0xc3, 0xf4],
	[0xdf, 0xff, 0xe2],
	[0xa3, 0xbf, 0xff]
];

function hashName(name: string): number {
	let h = 0;
	for (let i = 0; i < name.length; i++) {
		h = (Math.imul(31, h) + name.charCodeAt(i)) | 0;
	}
	return Math.abs(h);
}

export function avatarGradient(name: string): string {
	const h = hashName(name);
	const c1 = palette[h % palette.length];
	const c2 = palette[(h + 3) % palette.length];
	return `linear-gradient(135deg, rgb(${c1.join(',')}), rgb(${c2.join(',')}))`;
}

export function artistInitials(name: string): string {
	if (!name) return '';
	return name
		.split(' ')
		.filter((w) => w.length > 0)
		.map((w) => w[0])
		.join('')
		.toUpperCase();
}
