import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'https://localhost:7058',
        changeOrigin: true,// 解决跨域和安全问题
        secure: false,//禁用 SSL 证书验证（开发环境中的自签名证书）
        headers: {
          'Connection': 'keep-alive'
        },
      },
      '/Downloads': {
        target: 'https://localhost:7058',
        changeOrigin: true,
        secure: false,
      },
    },
  },
}) 