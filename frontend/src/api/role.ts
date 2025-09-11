import api from './index'
import type { Role, PaginationRequest, PaginationResponse } from '@/types'

// 重新导出通用类型
export type { Role } from '@/types'

// 角色查询请求
export interface RoleQueryRequest extends PaginationRequest {
  name?: string
}

// 创建角色请求
export interface CreateRoleRequest {
  name: string
  description: string
  permissionCodes: string[]
}

// 更新角色请求
export interface UpdateRoleRequest {
  roleId: string
  name: string
  description: string
  permissionCodes: string[]
}

// ==================== API 方法 ====================

/**
 * 获取角色列表
 */
export const getAllRoles = (params: RoleQueryRequest) => {
  return api.get<PaginationResponse<Role>>('/roles', { params })
}

/**
 * 获取单个角色
 */
export const getRole = (roleId: string) => {
  return api.get<Role>(`/roles/${roleId}`)
}

/**
 * 创建角色
 */
export const createRole = (data: CreateRoleRequest) => {
  return api.post<Role>('/roles', data)
}

/**
 * 更新角色
 */
export const updateRole = (data: UpdateRoleRequest) => {
  return api.put('/roles/update', data)
}

/**
 * 删除角色
 */
export const deleteRole = (roleId: string) => {
  return api.delete(`/roles/${roleId}`)
} 