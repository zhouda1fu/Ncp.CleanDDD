import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as loginApi } from '@/api/user'
import { usePermissionStore } from './permission'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const refreshToken = ref<string>(localStorage.getItem('refreshToken') || '')
  
  // 从localStorage恢复用户信息
  const userFromStorage = localStorage.getItem('user')
  const user = ref<{
    userId: string
    name: string
    email: string
  } | null>(userFromStorage ? JSON.parse(userFromStorage) : null)

  const isAuthenticated = computed(() => !!token.value)

  const setToken = (newToken: string, newRefreshToken: string) => {
    token.value = newToken
    refreshToken.value = newRefreshToken
    localStorage.setItem('token', newToken)
    localStorage.setItem('refreshToken', newRefreshToken)
  }

  const setUser = (userInfo: { userId: string; name: string; email: string }) => {
    user.value = userInfo
    // 保存到localStorage
    localStorage.setItem('user', JSON.stringify(userInfo))
  }

  const logout = () => {
    token.value = ''
    refreshToken.value = ''
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
    
    // 清除权限
    const permissionStore = usePermissionStore()
    permissionStore.clearPermissions()
  }

  const login = async (username: string, password: string) => {
    try {
      const response = await loginApi({ username, password })
      const { token: newToken, refreshToken: newRefreshToken, userId, name, email, permissions } = response.data
      
      setToken(newToken, newRefreshToken)
      setUser({ userId, name, email })
      
      // 解析权限信息
      const permissionStore = usePermissionStore()
      try {
        const permissionsArray = JSON.parse(permissions || '[]')
        permissionStore.setUserPermissions(permissionsArray)
      } catch (error) {
        console.error('解析用户权限失败:', error)
        permissionStore.setUserPermissions([])
      }
      
      return response
    } catch (error) {
      throw error
    }
  }

  return {
    token,
    refreshToken,
    user,
    isAuthenticated,
    setToken,
    setUser,
    logout,
    login
  }
}) 