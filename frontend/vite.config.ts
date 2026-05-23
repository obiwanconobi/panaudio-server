import { sveltekit } from '@sveltejs/kit/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';

const DEFAULTS = {
	generatedArtistAvatars: true,
	enablePlaybackReporting: true
};

export default defineConfig({
	plugins: [
		sveltekit(),
		tailwindcss(),
		{
			name: 'dev-config',
			configureServer(server) {
				server.middlewares.use('/config', (_req, res) => {
					res.writeHead(200, { 'Content-Type': 'application/json' });
					res.end(JSON.stringify(DEFAULTS));
				});
			}
		}
	],
	server: {
		proxy: {
			'/api': 'http://localhost:5027'
		}
	}
});
