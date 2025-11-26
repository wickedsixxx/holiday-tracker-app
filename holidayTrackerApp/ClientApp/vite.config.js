import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
// ... (varsa sertifika pluginleri vs kalsýn)

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 5173,
        proxy: {
            '/api': {
                target: 'https://localhost:7013', // <-- BURASI 7013 OLMALI (HTTPS)
                changeOrigin: true,
                secure: false, // <-- Bu, SSL sertifika hatasý almaný engeller
                // rewrite satýrýna gerek yok, backend'in zaten /api bekliyor muhtemelen.
            }
        }
    }
});