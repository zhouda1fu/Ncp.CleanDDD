import api from './index'
import type { Permission, PermissionGroup } from '@/types'

// 重新导出通用类型
export type { Permission, PermissionGroup } from '@/types'

// ==================== API 方法 ====================

/**
 * 获取权限树
 */
export const getPermissionTree = () => {
  return api.get<PermissionGroup[]>('/permissions/tree')
}

/**
 * 获取所有权限
 */
export const getAllPermissions = () => {
  return api.get<Permission[]>('/permissions')
}

/**
 * 根据角色ID获取权限
 */
export const getPermissionsByRole = (roleId: string) => {
  return api.get<Permission[]>(`/permissions/role/${roleId}`)
}

/**
 * 根据用户ID获取权限
 */
export const getPermissionsByUser = (userId: string) => {
  return api.get<Permission[]>(`/permissions/user/${userId}`)
}

