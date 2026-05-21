import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'node:path';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: { '@': path.resolve(__dirname, 'src') },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:50443',
        changeOrigin: true,
        secure: false,
      },
      '/ws': {
        target: 'wss://localhost:50443',
        changeOrigin: true,
        secure: false,
        ws: true,
        rewriteWsOrigin: true,
      },
    },
  },
});
