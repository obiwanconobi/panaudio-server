<script lang="ts">
	let {
		title,
		description,
		onsave,
		onclose,
		children
	}: {
		title: string;
		description?: string;
		onsave: () => void;
		onclose: () => void;
		children?: unknown;
	} = $props();
</script>

<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
	<div class="bg-zinc-900 border border-zinc-700 rounded-xl w-full max-w-md mx-4 shadow-2xl">
		<div class="flex items-center justify-between px-6 py-4 border-b border-zinc-800">
			<div>
				<h2 class="text-lg font-semibold">{title}</h2>
				{#if description}
					<p class="text-sm text-zinc-400 mt-0.5">{description}</p>
				{/if}
			</div>
			<button onclick={onclose} class="text-zinc-400 hover:text-zinc-200 p-1" aria-label="Close">
				<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
				</svg>
			</button>
		</div>

		<div class="px-6 py-4">
			{#if children}
				{@render (children as () => any)()}
			{/if}
		</div>

		<div class="flex justify-end gap-3 px-6 py-4 border-t border-zinc-800">
			<button
				onclick={onclose}
				class="px-4 py-2 rounded-lg text-sm text-zinc-400 hover:text-zinc-200 hover:bg-zinc-800 transition-colors"
			>
				Cancel
			</button>
			<button
				onclick={onsave}
				class="px-4 py-2 rounded-lg text-sm bg-violet-600 hover:bg-violet-500 text-white font-medium transition-colors"
			>
				Save
			</button>
		</div>
	</div>
</div>
