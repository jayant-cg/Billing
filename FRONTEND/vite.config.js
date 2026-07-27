import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://billingcg-bbgybfaafkdda0g2.indiasouthcentral-01.azurewebsites.net',
        changeOrigin: true,
        secure: true
      }
    }
  }
})

 