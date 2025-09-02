import api from './index'

// 权限项接口
export interface PermissionItem {
  code: string
  displayName: string
  children: PermissionItem[]
  isEnabled: boolean
}

// 权限组接口
export interface PermissionGroup {
  name: string
  permissions: PermissionItem[]
}



// 获取权限树
export const getPermissionTree = () => {
  return api.get<PermissionGroup[]>('/permissions/tree')
}

