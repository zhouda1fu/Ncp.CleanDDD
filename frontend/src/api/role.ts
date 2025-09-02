import api from './index'

// 角色信息
export interface RoleInfo {
  roleId: string
  name: string
  description: string
  permissionCodes: string[]
  createdAt: string
  updatedAt: string
}

// 分页查询参数
export interface RoleQueryInput {
  pageIndex: number
  pageSize: number
  name?: string
  countTotal:boolean
}

// 分页数据
export interface PagedData<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
}

// 获取所有角色
export const getAllRoles = (params: RoleQueryInput) => {
  return api.get<PagedData<RoleInfo>>('/roles', { params })
}

// 获取单个角色
export const getRole = (roleId: string) => {
  return api.get<RoleInfo>(`/roles/${roleId}`)
}

// 创建角色
export interface CreateRoleRequest {
  name: string
  description: string
  permissionCodes: string[]
}



export interface CreateRoleResponse {
  roleId: string
  name: string
  description: string
}

export const createRole = (data: CreateRoleRequest) => {
  return api.post<CreateRoleResponse>('/roles', data)
}

// 更新角色
export interface UpdateRoleRequest {
  roleId: string
  name: string
  description: string
  permissionCodes: string[]
}


// 删除角色
export const deleteRole = (roleId: string) => {
  return api.delete(`/roles/${roleId}`)
} 

export const updateRole = (data: UpdateRoleRequest) => {
  return api.put('/roles/update', data)
} 