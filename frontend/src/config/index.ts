import type { EnvConfig } from '@/types'

// 环境配置
export const config: EnvConfig = {
  VITE_API_BASE_URL: import.meta.env.VITE_API_BASE_URL || '/api',
  VITE_APP_TITLE: import.meta.env.VITE_APP_TITLE || 'NCP Clean DDD',
  MODE: import.meta.env.MODE || 'development',
  DEV: import.meta.env.DEV || true,
  PROD: import.meta.env.PROD || false
}

// API 配置
export const apiConfig = {
  baseURL: config.VITE_API_BASE_URL,
  timeout: 10000,
  withCredentials: false
}

// 应用配置
export const appConfig = {
  title: config.VITE_APP_TITLE,
  version: '1.0.0',
  author: 'NCP Team'
}

// 分页配置
export const paginationConfig = {
  defaultPageSize: 10,
  pageSizes: [10, 20, 50, 100],
  layout: 'total, sizes, prev, pager, next, jumper'
}

// 上传配置
export const uploadConfig = {
  maxFileSize: 10 * 1024 * 1024, // 10MB
  allowedExtensions: ['.xlsx', '.xls'],
  chunkSize: 1024 * 1024 // 1MB
}

// 表格配置
export const tableConfig = {
  stripe: true,
  border: false,
  size: 'default',
  headerCellStyle: {
    backgroundColor: '#f8fafc',
    color: '#374151',
    fontWeight: '600'
  }
}

// 验证规则配置
export const validationConfig = {
  username: {
    minLength: 3,
    maxLength: 20,
    pattern: /^[a-zA-Z0-9_]+$/
  },
  password: {
    minLength: 6,
    maxLength: 50,
    pattern: /^(?=.*[a-zA-Z])(?=.*\d).+$/
  },
  email: {
    pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  },
  phone: {
    pattern: /^1[3-9]\d{9}$/
  }
}
