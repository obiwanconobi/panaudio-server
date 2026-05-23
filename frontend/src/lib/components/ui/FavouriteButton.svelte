<script lang="ts">
	let {
		favourite,
		ontoggle
	}: {
		favourite: boolean;
		ontoggle: () => void;
	} = $props();
</script>

<button
	onclick={ontoggle}
	class="p-1.5 rounded-full {favourite ? 'text-red-400 hover:text-red-300' : 'text-zinc-500 hover:text-red-400'} transition-colors"
	aria-label={favourite ? 'Remove from favourites' : 'Add to favourites'}
>
	<svg class="w-5 h-5" fill={favourite ? 'currentColor' : 'none'} stroke="currentColor" viewBox="0 0 24 24">
		<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
	</svg>
</button>
