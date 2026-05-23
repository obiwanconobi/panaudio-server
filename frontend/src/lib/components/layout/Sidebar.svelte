<script lang="ts">
	import { page } from '$app/stores';
	import { library } from '$lib/stores/library.svelte';

	const navItems = [
		{ href: '/', label: 'Home', icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' },
		{ href: '/albums', label: 'Albums', icon: 'M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10' },
		{ href: '/artists', label: 'Artists', icon: 'M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3' },
		{ href: '/songs', label: 'Songs', icon: 'M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3' },
		{ href: '/playlists', label: 'Playlists', icon: 'M4 6h16M4 10h16M4 14h16M4 18h16' },
		{ href: '/search', label: 'Search', icon: 'M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z' },
		{ href: '/settings', label: 'Settings', icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z' },
	];

	let collapsed = $state(false);
</script>

<aside class="flex flex-col h-full bg-zinc-900 border-r border-zinc-800 transition-all duration-300 {collapsed ? 'w-16' : 'w-56'}">
	<div class="p-4 flex items-center justify-between">
		<div class="flex items-center gap-2">
			<img src="/logo.png" alt="panaudio" class="w-[50px] h-[50px] shrink-0" />
			{#if !collapsed}
				<span class="text-lg font-bold text-violet-400 lowercase" style="font-family: 'League Spartan', sans-serif;">panaudio</span>
			{/if}
		</div>
		<button
			onclick={() => (collapsed = !collapsed)}
			class="text-zinc-400 hover:text-zinc-200 p-1 rounded-lg hover:bg-zinc-800"
			aria-label="Toggle sidebar"
		>
			<svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
			</svg>
		</button>
	</div>

	<nav class="flex-1 px-2 space-y-1 overflow-y-auto">
		{#each navItems as item}
			<a
				href={item.href}
				class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors
					{$page.url.pathname === item.href
						? 'bg-violet-600/20 text-violet-300'
						: 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-800'}"
			>
				<svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d={item.icon} />
				</svg>
				{#if !collapsed}
					<span>{item.label}</span>
				{/if}
			</a>
		{/each}
	</nav>

	<div class="p-3 border-t border-zinc-800">
		{#if !collapsed}
			<p class="text-xs text-zinc-500 truncate">
				{library.songs.length} songs &middot; {library.albums.length} albums
			</p>
		{/if}
	</div>
</aside>
