import api from './index'
import type { OrganizationUnit, OrganizationUnitTree } from '@/types'

// 重新导出通用类型
export type { OrganizationUnit, OrganizationUnitTree } from '@/types'

// 创建组织架构请求
export interface CreateOrganizationUnitRequest {
  name: string
  description: string
  parentId?: number
  sortOrder: number
}

// 更新组织架构请求
export interface UpdateOrganizationUnitRequest {
  id: number
  name: string
  description: string
  parentId?: number
  sortOrder: number
}

// 分配用户组织架构请求
export interface AssignUserRequest {
  userId: string
  organizationUnitIds: number[]
  primaryOrganizationUnitId?: number
}

// 组织架构查询参数
export interface OrganizationQueryParams {
  includeInactive?: boolean
}

// ==================== API 方法 ====================

/**
 * 组织架构相关API
 */
export const organizationApi = {
  /**
   * 获取所有组织架构
   */
  getAll: (includeInactive = false) => 
    api.get<OrganizationUnit[]>('/organization-units', {
      params: { includeInactive }
    }),

  /**
   * 获取组织架构树
   */
  getTree: (includeInactive = false) => 
    api.get<OrganizationUnitTree[]>('/organization-units/tree', {
      params: { includeInactive }
    }),

  /**
   * 获取单个组织架构
   */
  getById: (id: number) => 
    api.get<OrganizationUnit>(`/organization-units/${id}`),

  /**
   * 创建组织架构
   */
  create: (data: CreateOrganizationUnitRequest) => 
    api.post<OrganizationUnit>('/organization-units', data),

  /**
   * 更新组织架构
   */
  update: (data: UpdateOrganizationUnitRequest) => 
    api.put('/organization-units', data),

  /**
   * 删除组织架构
   */
  delete: (id: number) => 
    api.delete(`/organization-units/${id}`),

  /**
   * 分配用户组织架构
   */
  assignUser: (data: AssignUserRequest) => 
    api.post('/organization-units/assign-user', data)
} 