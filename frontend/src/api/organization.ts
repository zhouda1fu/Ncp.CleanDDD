import api from './index'

export interface OrganizationUnit {
  id: number
  name: string
  description: string
  parentId?: number
  sortOrder: number
  isActive: boolean
  createdAt: string
  deletedAt?: string
}

export interface OrganizationUnitTree {
  id: number
  name: string
  description: string
  parentId?: number
  sortOrder: number
  isActive: boolean
  createdAt: string
  children: OrganizationUnitTree[]
}

export interface CreateOrganizationUnitRequest {
  name: string
  description: string
  parentId?: number
  sortOrder: number
}

export interface UpdateOrganizationUnitRequest {
  id: number
  name: string
  description: string
  parentId?: number
  sortOrder: number
}

export const organizationApi = {
  // 获取所有组织架构
  getAll: (includeInactive = false) => 
    api.get<OrganizationUnit[]>('/organization-units', {
      params: { includeInactive }
    }),

  // 获取组织架构树
  getTree: (includeInactive = false) => 
    api.get<OrganizationUnitTree[]>('/organization-units/tree', {
      params: { includeInactive }
    }),

  // 获取单个组织架构
  getById: (id: number) => 
    api.get<OrganizationUnit>(`/organization-units/${id}`),

  // 创建组织架构
  create: (data: CreateOrganizationUnitRequest) => 
    api.post<{ id: number; message: string }>('/organization-units', data),

  // 更新组织架构
  update: (id: number, data: Omit<UpdateOrganizationUnitRequest, 'id'>) => 
    api.put<{ message: string }>(`/organization-units/${id}`, data),

  // 删除组织架构
  delete: (id: number) => 
    api.delete<{ message: string }>(`/organization-units/${id}`),

  // 分配用户组织架构
  assignUser: (data: { userId: string; organizationUnitIds: number[]; primaryOrganizationUnitId?: number }) => 
    api.post<{ message: string }>('/organization-units/assign-user', data)
} 