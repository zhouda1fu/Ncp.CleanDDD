import axios from 'axios'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'
import { apiConfig } from '@/config'
import type { BaseResponse } from '@/types'

// 创建axios实例
const api = axios.create({
  baseURL: apiConfig.baseURL,
  timeout: apiConfig.timeout,
  headers: {
    'Content-Type': 'application/json',
  },
})

// 添加防重复处理401错误的标志
let isHandling401 = false

// 请求拦截器
api.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
api.interceptors.response.use(
  (response) => {
    const data = response.data as BaseResponse
    if (data.success) {
      return response.data
    } else {
      // 处理后端返回的错误格式
      if (data?.message) {
        ElMessage.error(data.message)
      } else {
        ElMessage.error('请求失败')
      }
      return Promise.reject(data)
    }
  },
  (error) => {
    if (error.response) {
      const { status, data } = error.response
      
      switch (status) {
        case 401:
          // 防止多次并发请求时重复处理401错误
          if (!isHandling401) {
            isHandling401 = true
            ElMessage.error('登录已过期，请重新登录')
            const authStore = useAuthStore()
            authStore.logout()
            //跳转到登录页面
            router.push('/login')
            
            // 2秒后重置标志，防止正常的401处理被阻止
            setTimeout(() => {
              isHandling401 = false
            }, 2000)
          }
          break
        case 403:
          ElMessage.error('权限不足')
          break
        case 404:
          ElMessage.error('请求的资源不存在')
          break
        case 500:
          ElMessage.error('服务器内部错误')
          break
        default:
          ElMessage.error(data?.message || '请求失败')
      }
    } else {
      ElMessage.error('网络错误，请检查网络连接')
    }
    return Promise.reject(error)
  }
)

export default api 