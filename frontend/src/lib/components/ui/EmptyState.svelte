<script lang="ts">
	let {
		icon = 'search',
		title = 'Nothing here',
		description = ''
	}: {
		icon?: string;
		title?: string;
		description?: string;
	} = $props();

	const icons: Record<string, string> = {
		search: 'M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z',
		music: 'M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3',
		playlist: 'M4 6h16M4 10h16M4 14h16M4 18h16',
		heart: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z'
	};
</script>

<div class="flex flex-col items-center justify-center py-16 text-center">
	<svg class="w-16 h-16 text-zinc-700 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
		<path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d={icons[icon] || icons.search} />
	</svg>
	<h3 class="text-lg font-medium text-zinc-400">{title}</h3>
	{#if description}
		<p class="text-sm text-zinc-500 mt-1 max-w-sm">{description}</p>
	{/if}
</div>
