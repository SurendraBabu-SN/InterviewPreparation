import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7289',   // your .NET API
        changeOrigin: true,
        secure: false                       // accept the .NET dev certificate
      }
    }
  }
})
