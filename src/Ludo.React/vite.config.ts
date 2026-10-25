import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5158',
        changeOrigin: true,
      },
      '/hubs': {
        target: 'http://localhost:5158',
        changeOrigin: true,
        ws: true,
      },
    },
  },
})
