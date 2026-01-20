import { fileURLToPath, URL } from 'node:url'
import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import AutoImport from 'unplugin-auto-import/vite'
import VueSetupExtend from 'unplugin-vue-setup-extend-plus/vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  // 生产环境发布在二级目录 /admin/ 下，通过 VITE_BASE 控制 base
  const base = env.VITE_BASE || '/'

  return {
    base,
    plugins: [
      vue(),
      VueSetupExtend({}),
      AutoImport({
        imports: [
          'vue',
          'vue-router',
          'pinia',
          '@vueuse/core',
          {
            'element-plus': [
              'ElMessage',
              'ElMessageBox',
              'ElNotification',
            ],
          },
        ],
        dts: 'src/auto-imports.d.ts',
        vueTemplate: true,
      }),
    ],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    server: {
      port: Number(env.VITE_PORT) || 5173,
    },
  }
})
