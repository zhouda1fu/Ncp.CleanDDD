// 通用类型定义
export interface BaseResponse<T = any> {
  success: boolean
  message: string
  code: number
  data: T
  errorData?: any[]
}

export interface PaginationRequest {
  pageIndex: number
  pageSize: number
  countTotal?: boolean
}

export interface PaginationResponse<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
}

// 用户相关类型
export interface User {
  userId: string
  name: string
  email: string
  phone: string
  realName: string
  status: number
  gender: string
  age: number
  organizationUnitId: number
  organizationUnitName: string
  birthDate: string
  roles: string[]
  createdAt: string
}

export interface LoginCredentials {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  userId: string
  name: string
  email: string
  permissions: string
}

// 角色相关类型
export interface Role {
  roleId: string
  name: string
  displayName: string
  description: string
  isSystem: boolean
  createdAt: string
  permissionCodes?: string[]
}

// 组织架构相关类型
export interface OrganizationUnit {
  id: number
  name: string
  description?: string
  parentId: number | null
  level: number
  path: string
  sortOrder: number
  createdAt: string
}

export interface OrganizationUnitTree extends OrganizationUnit {
  children: OrganizationUnitTree[]
}

// 权限相关类型
export interface Permission {
  code: string
  displayName: string
  children: Permission[]
  isEnabled: boolean
}

export interface PermissionGroup {
  name: string
  permissions: Permission[]
}

// 菜单相关类型
export interface MenuPermission {
  path: string
  name: string
  icon: string
  requiredPermissions: string[]
  children?: MenuPermission[]
}

// 表单验证相关类型
export interface FormValidationRule {
  required?: boolean
  message?: string
  trigger?: string | string[]
  min?: number
  max?: number
  pattern?: RegExp
  validator?: (rule: any, value: any, callback: any) => void
}

// 表格相关类型
export interface TableColumn {
  prop: string
  label: string
  width?: string | number
  minWidth?: string | number
  fixed?: boolean | 'left' | 'right'
  sortable?: boolean
  align?: 'left' | 'center' | 'right'
}

// 导入导出相关类型
export interface ImportResult {
  successCount: number
  failCount: number
  totalCount: number
  failedRows: ImportFailedRow[]
}

export interface ImportFailedRow {
  row: number
  errors: string[]
  data: any
}

// API 错误类型
export interface ApiError {
  message: string
  code: number
  details?: any
}

// 通用枚举
export enum UserStatus {
  DISABLED = 0,
  ENABLED = 1
}

export enum Gender {
  MALE = '男',
  FEMALE = '女'
}

// 环境配置类型
export interface EnvConfig {
  VITE_API_BASE_URL: string
  VITE_APP_TITLE: string
  MODE: string
  DEV: boolean
  PROD: boolean
}
