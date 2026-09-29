import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

// In development the browser talks to Vite, which forwards /api calls to the .NET API.
// That keeps the front end free of hard-coded API URLs and avoids CORS during development.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
  test: {
    environment: 'node',
  },
});
