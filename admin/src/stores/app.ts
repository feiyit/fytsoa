import { appStorage, STORAGE_KEYS } from '@/utils/storage'

export const useAppStore = defineStore('app', {
  state: () => ({
    title: import.meta.env.VITE_APP_NAME || 'FytAdmin',
    loading: true,
    theme: 'light' as 'light' | 'dark',
    primaryColor: '#409EFF',
  }),
  actions: {
    setLoading(value: boolean) {
      this.loading = value
    },
    setTheme(theme: 'light' | 'dark', persist = true) {
      this.theme = theme
      if (typeof document !== 'undefined') {
        const root = document.documentElement
        if (theme === 'dark') {
          root.classList.add('dark')
        } else {
          root.classList.remove('dark')
        }
      }

      if (persist) {
        appStorage.set(STORAGE_KEYS.THEME, theme)
      }
    },
    toggleTheme() {
      this.setTheme(this.theme === 'dark' ? 'light' : 'dark')
    },
    initTheme() {
      if (typeof window === 'undefined') return

      // Prefer the user's last choice; fall back to system preference.
      const saved = appStorage.get<'light' | 'dark'>(STORAGE_KEYS.THEME)
      if (saved === 'light' || saved === 'dark') {
        this.setTheme(saved, false)
        return
      }

      const prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches
      const initial = prefersDark ? 'dark' : 'light'
      //console.log('initial',initial);
      this.setTheme(initial, false)
    },
    setPrimaryColor(color: string) {
      this.primaryColor = color

      if (typeof document !== 'undefined') {
        const root = document.documentElement
        root.style.setProperty('--el-color-primary', color)
      }

      appStorage.set(STORAGE_KEYS.PRIMARY_COLOR, color)
    },
    initPrimaryColor() {
      // 优先恢复用户上次选择的主题色，否则使用默认主题色
      const saved = appStorage.get<string>(STORAGE_KEYS.PRIMARY_COLOR, this.primaryColor)
      this.primaryColor = saved || this.primaryColor

      if (typeof document !== 'undefined') {
        const root = document.documentElement
        root.style.setProperty('--el-color-primary', this.primaryColor)
      }
    },
  },
})
