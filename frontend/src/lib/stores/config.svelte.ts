interface FrontendConfig {
	generatedArtistAvatars: boolean;
	enablePlaybackReporting: boolean;
}

let config = $state<FrontendConfig>({ generatedArtistAvatars: true, enablePlaybackReporting: true });
let loaded = $state(false);

async function loadConfig() {
	const res = await fetch('/config');
	config = await res.json();
	loaded = true;
}

export const frontendConfig = {
	get generatedArtistAvatars() { return config.generatedArtistAvatars; },
	get enablePlaybackReporting() { return config.enablePlaybackReporting; },
	get loaded() { return loaded; },
	load: loadConfig
};
