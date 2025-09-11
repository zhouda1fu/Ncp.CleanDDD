import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginApi } from '@/api/user'
import { usePermissionStore } from './permission'
import { STORAGE_KEYS } from '@/constants'
import type { User, LoginCredentials, LoginResponse } from '@/types'
import { ErrorHandler } from '@/utils/error'

interface AuthUser {
  userId: string
  name: string
  email: string
}

export const useAuthStore = defineStore('auth', () => {
  // 状态
  const token = ref<string>(localStorage.getItem(STORAGE_KEYS.TOKEN) || '')
  const refreshToken = ref<string>(localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN) || '')
  
  // 从localStorage恢复用户信息
  const userFromStorage = localStorage.getItem(STORAGE_KEYS.USER)
  const user = ref<AuthUser | null>(
    userFromStorage ? JSON.parse(userFromStorage) : null
  )

  const isLoading = ref(false)

  // 计算属性
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const currentUser = computed(() => user.value)

  // 私有方法
  const setToken = (newToken: string, newRefreshToken: string) => {
    token.value = newToken
    refreshToken.value = newRefreshToken
    localStorage.setItem(STORAGE_KEYS.TOKEN, newToken)
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken)
  }

  const setUser = (userInfo: AuthUser) => {
    user.value = userInfo
    localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(userInfo))
  }

  const clearStorage = () => {
    localStorage.removeItem(STORAGE_KEYS.TOKEN)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
    localStorage.removeItem(STORAGE_KEYS.USER)
  }

  // 公共方法
  const logout = () => {
    token.value = ''
    refreshToken.value = ''
    user.value = null
    clearStorage()
    
    // 清除权限
    const permissionStore = usePermissionStore()
    permissionStore.clearPermissions()
  }

  const login = async (credentials: LoginCredentials) => {
    try {
      isLoading.value = true
      const response = await loginApi(credentials)
      const { 
        token: newToken, 
        refreshToken: newRefreshToken, 
        userId, 
        name, 
        email, 
        permissions 
      } = response.data
      
      setToken(newToken, newRefreshToken)
      setUser({ userId, name, email })
      
      // 解析权限信息
      const permissionStore = usePermissionStore()
      try {
        const permissionsArray = JSON.parse(permissions || '[]')
        permissionStore.setUserPermissions(permissionsArray)
      } catch (error) {
        ErrorHandler.handle(error, 'parsePermissions')
        permissionStore.setUserPermissions([])
      }
      
      return response
    } catch (error) {
      ErrorHandler.handle(error, 'login')
      throw error
    } finally {
      isLoading.value = false
    }
  }

  const refreshTokens = async () => {
    try {
      // TODO: 实现token刷新逻辑
      // const response = await refreshTokenApi(refreshToken.value)
      // setToken(response.data.token, response.data.refreshToken)
      console.warn('Token refresh not implemented yet')
    } catch (error) {
      logout() // 刷新失败时退出登录
      throw error
    }
  }

  const updateProfile = (profileData: Partial<AuthUser>) => {
    if (user.value) {
      user.value = { ...user.value, ...profileData }
      localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user.value))
    }
  }

  // 初始化方法
  const initialize = () => {
    // 检查token是否过期等初始化逻辑
    if (token.value && !user.value) {
      // 如果有token但没有用户信息，清除token
      logout()
    }
  }

  return {
    // 状态
    token,
    refreshToken,
    user,
    isLoading,
    
    // 计算属性
    isAuthenticated,
    currentUser,
    
    // 方法
    login,
    logout,
    refreshTokens,
    updateProfile,
    initialize
  }
}) 