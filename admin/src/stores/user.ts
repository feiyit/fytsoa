import { defineStore } from 'pinia'
import { appStorage, STORAGE_KEYS } from '@/utils/storage'
import type { LoginResult, UserInfo } from '@/api/auth'

interface UserState {
  token: string | null
  userInfo: UserInfo | null
  /** 当前用户拥有的权限编码列表，例如：['sys:role:add', 'sys:role:edit'] */
  permissions: string[]
}

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    token: appStorage.get<string>(STORAGE_KEYS.ACCESS_TOKEN) ?? null,
    userInfo: appStorage.get<UserInfo>(STORAGE_KEYS.USER_INFO) ?? null,
    // 权限列表：从本地存储恢复，避免刷新后丢失
    permissions: appStorage.get<string[]>(STORAGE_KEYS.PERMISSIONS, []) ?? [],
  }),
  getters: {
    isLoggedIn: (state) => !!state.token,
    displayName: (state) => state.userInfo?.username || '未登录',
    avatar: (state) => state.userInfo?.avatar || '',
  },
  actions: {
    setUser(payload: LoginResult) {
      this.token = payload.accessToken || ''
      this.userInfo = payload.userInfo || null

      if (this.token) {
        appStorage.set(STORAGE_KEYS.ACCESS_TOKEN, this.token)
      } else {
        appStorage.remove(STORAGE_KEYS.ACCESS_TOKEN)
      }

      if (this.userInfo) {
        appStorage.set(STORAGE_KEYS.USER_INFO, this.userInfo)
      } else {
        appStorage.remove(STORAGE_KEYS.USER_INFO)
      }
    },
    /** 设置权限编码列表，并持久化到本地 */
    setPermissions(perms: string[] = []) {
      const list = Array.isArray(perms) ? perms : []
      this.permissions = list
      appStorage.set(STORAGE_KEYS.PERMISSIONS, list)
    },
    clearUser() {
      this.token = null
      this.userInfo = null
      this.permissions = []
      appStorage.remove(STORAGE_KEYS.ACCESS_TOKEN)
      appStorage.remove(STORAGE_KEYS.REFRESH_TOKEN)
      appStorage.remove(STORAGE_KEYS.USER_INFO)
      appStorage.remove(STORAGE_KEYS.PERMISSIONS)
    },
  },
})
