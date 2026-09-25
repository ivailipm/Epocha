import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Proxies /api requests to the backend during development, so the app can call
// relative URLs and avoid CORS entirely instead of configuring it on the API.
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': 'http://localhost:5196',
    },
  },
})
