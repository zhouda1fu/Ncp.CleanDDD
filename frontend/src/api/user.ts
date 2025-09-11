import api from './index'
import type { 
  User, 
  LoginCredentials, 
  LoginResponse, 
  PaginationRequest, 
  PaginationResponse,
  ImportResult 
} from '@/types'

// 重新导出通用类型
export type { LoginCredentials, LoginResponse, User } from '@/types'

// 用户注册请求
export interface RegisterRequest {
  name: string
  email: string
  password: string
  confirmPassword: string
  phone: string
  realName: string
  status: number
  gender: string
  age: number
  roleIds: string[]
  organizationUnitId: number
  organizationUnitName: string
  birthDate: string
  userId: string
}

// 更新用户请求
export interface UpdateUserRequest {
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
  password: string
}

// 更新用户角色请求
export interface UpdateUserRolesRequest {
  roleIds: string[]
  userId: string
}

// 获取用户列表请求
export interface GetUsersRequest extends PaginationRequest {
  keyword?: string
  status?: number
  organizationUnitId?: number
}

// 批量导入用户数据
export interface BatchImportUsersData {
  file: File
  organizationUnitId?: number
  organizationUnitName?: string
  roleIds?: string[]
}

// ==================== API 方法 ====================

/**
 * 用户登录
 */
export const login = (data: LoginCredentials) => {
  return api.post<LoginResponse>('/user/login', data)
}

/**
 * 用户注册
 */
export const register = (data: RegisterRequest) => {
  return api.post<User>('/user/register', data)
}

/**
 * 获取用户资料
 */
export const getUserProfile = (userId: string) => {
  return api.get<User>(`/user/profile/${userId}`)
}

/**
 * 更新用户信息
 */
export const updateUser = (data: UpdateUserRequest) => {
  return api.put('/user/update', data)
}

/**
 * 重置用户密码
 */
export const resetPassword = (userId: string) => {
  return api.put('/user/password-reset', { userId })
}

/**
 * 更新用户角色
 */
export const updateUserRoles = (data: UpdateUserRolesRequest) => {
  return api.put('/users/update-roles', data)
}

/**
 * 获取用户列表
 */
export const getUsers = (params: GetUsersRequest) => {
  return api.get<PaginationResponse<User>>('/users', { params })
}

/**
 * 删除用户
 */
export const deleteUser = (userId: string) => {
  return api.delete(`/users/${userId}`)
}

/**
 * 下载用户导入模板
 */
export const downloadUserTemplate = (): Promise<Blob> => {
  const templateUrl = '/Downloads/ExcelTemplates/users_template.xlsx'
  
  return fetch(templateUrl, {
    method: 'GET',
    headers: {
      'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    }
  }).then(response => {
    if (!response.ok) {
      throw new Error('模板文件下载失败')
    }
    return response.blob()
  })
}

/**
 * 批量导入用户
 */
export const batchImportUsers = (data: BatchImportUsersData) => {
  const formData = new FormData()
  formData.append('file', data.file)

  if (data.organizationUnitId) {
    formData.append('organizationUnitId', data.organizationUnitId.toString())
  }
  if (data.organizationUnitName) {
    formData.append('organizationUnitName', data.organizationUnitName)
  }
  
  if (data.roleIds?.length) {
    data.roleIds.forEach(roleId => {
      formData.append('roleIds', roleId)
    })
  }
  
  return api.post<ImportResult>('/users/batch-import', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}



