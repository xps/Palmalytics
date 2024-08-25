import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Doc: https://vitejs.dev/config/
export default defineConfig({
    base: '/palmalytics/',
    plugins: [react()],
    css: {
        postcss: {
            plugins: [
            ]
        }
    },
    build: {
        chunkSizeWarningLimit: 600,
        rollupOptions: {
            output: {
                manualChunks: function (id) {
                    if (id.includes('/node_modules/apexcharts/')) {
                        return "apexcharts";
                    }
                    if (id.includes('/node_modules/')) {
                        return "vendor";
                    }
                }
            }
        }
    },
    server: {
        proxy: {
            '/palmalytics/api/': {
                target: 'https://localhost:44316',
                changeOrigin: true,
                secure: false
            }
        }
    }
});
