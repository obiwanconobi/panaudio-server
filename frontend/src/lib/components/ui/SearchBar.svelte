<script lang="ts">
	let { value, placeholder = 'Search...', onsearch } = $props<{
		value?: string;
		placeholder?: string;
		onsearch: (query: string) => void;
	}>();

	let searchText = $state('');
	let inputValue = $derived(searchText || (value ?? ''));
	let debounceTimer: ReturnType<typeof setTimeout>;

	function handleInput(e: Event) {
		const target = e.target as HTMLInputElement;
		searchText = target.value;
		clearTimeout(debounceTimer);
		debounceTimer = setTimeout(() => {
			onsearch(searchText);
		}, 300);
	}

	function handleClear() {
		searchText = '';
		onsearch('');
	}
</script>

<div class="relative">
	<svg class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-zinc-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
		<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
	</svg>
	<input
		type="text"
		value={inputValue}
		oninput={handleInput}
		placeholder={placeholder}
		class="w-full pl-10 pr-10 py-2.5 bg-zinc-900 border border-zinc-700 rounded-lg text-sm text-zinc-100
			placeholder-zinc-500 focus:outline-none focus:border-violet-500 focus:ring-1 focus:ring-violet-500
			transition-colors"
	/>
	{#if inputValue}
		<button
			onclick={handleClear}
			class="absolute right-3 top-1/2 -translate-y-1/2 text-zinc-500 hover:text-zinc-300"
			aria-label="Clear search"
		>
			<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
			</svg>
		</button>
	{/if}
</div>
