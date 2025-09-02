import { usePermissionStore } from '@/stores/permission'

/**
 * 检查是否有权限
 * @param requiredPermissions 需要的权限列表
 * @param any 是否只需要满足任一权限即可
 * @returns 是否有权限
 */
export function hasPermission(requiredPermissions: string[], any: boolean = false): boolean {
  const permissionStore = usePermissionStore()
  
  if (!requiredPermissions || requiredPermissions.length === 0) {
    return true
  }
  
  if (any) {
    // 满足任一权限即可
    return requiredPermissions.some(permission => 
      permissionStore.hasPermission([permission])
    )
  } else {
    // 需要满足所有权限
    return requiredPermissions.every(permission =>
      permissionStore.hasPermission([permission])
    )
  }
}

/**
 * 检查是否有任一权限
 * @param requiredPermissions 需要的权限列表
 * @returns 是否有权限
 */
export function hasAnyPermission(requiredPermissions: string[]): boolean {
  return hasPermission(requiredPermissions, true)
}

/**
 * 检查是否有所有权限
 * @param requiredPermissions 需要的权限列表
 * @returns 是否有权限
 */
export function hasAllPermissions(requiredPermissions: string[]): boolean {
  return hasPermission(requiredPermissions, false)
}

/**
 * 获取用户权限列表
 * @returns 用户权限列表
 */
export function getUserPermissions(): string[] {
  const permissionStore = usePermissionStore()
  return permissionStore.userPermissions
}

/**
 * 检查是否是系统管理员
 * @returns 是否是系统管理员
 */
export function isSystemAdmin(): boolean {
  return hasPermission(['SystemAdmin'])
} 