import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { getPermissionTree } from '@/api/permission'
import { menuRouteConfigs, convertToMenuPermission } from '@/router/menu-config'

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

// 菜单权限配置
export interface MenuPermission {
  path: string
  name: string
  icon: string
  requiredPermissions: string[]
  children?: MenuPermission[]
}

export const usePermissionStore = defineStore('permission', () => {
  const userPermissions = ref<string[]>([])
  const permissionTree = ref<PermissionGroup[]>([])
  const isLoading = ref(false)

  // 从统一配置生成菜单权限配置
  const menuPermissions = computed(() => 
    menuRouteConfigs
      .filter(config => !config.meta?.hideInMenu)  // 过滤掉隐藏的菜单项
      .map(convertToMenuPermission)
  )

  // 设置用户权限
  const setUserPermissions = (permissions: string[]) => {
    userPermissions.value = permissions
    savePermissionsToStorage()
  }

  // 检查是否有权限
  const hasPermission = (requiredPermissions: string[]): boolean => {
    if (!requiredPermissions || requiredPermissions.length === 0) {
      return true
    }
    
    // 如果用户权限为空，返回false
    if (!userPermissions.value || userPermissions.value.length === 0) {
      return false
    }
    
    // 如果有系统管理员权限，则拥有所有权限
    if (userPermissions.value.includes('SystemAdmin')) {
      return true
    }
    
    return requiredPermissions.some(permission => 
      userPermissions.value.includes(permission)
    )
  }

  // 获取有权限的菜单
  const getAuthorizedMenus = computed(() => {
    return menuPermissions.value.filter((menu: MenuPermission) => 
      hasPermission(menu.requiredPermissions)
    )
  })

  // 加载权限树
  const loadPermissionTree = async () => {
    try {
      isLoading.value = true
      const response = await getPermissionTree()
      permissionTree.value = response.data
    } catch (error) {
      console.error('加载权限树失败:', error)
    } finally {
      isLoading.value = false
    }
  }

  // 清除权限
  const clearPermissions = () => {
    userPermissions.value = []
    permissionTree.value = []
    localStorage.removeItem('userPermissions')
  }

  // 初始化权限（从本地存储恢复）
  const initPermissions = () => {
    const storedPermissions = localStorage.getItem('userPermissions')
    if (storedPermissions) {
      try {
        const permissions = JSON.parse(storedPermissions)
        userPermissions.value = permissions
      } catch (error) {
        console.error('解析本地存储的权限失败:', error)
        localStorage.removeItem('userPermissions')
      }
    }
  }

  // 保存权限到本地存储
  const savePermissionsToStorage = () => {
    localStorage.setItem('userPermissions', JSON.stringify(userPermissions.value))
  }

  return {
    userPermissions,
    permissionTree,
    isLoading,
    menuPermissions,
    setUserPermissions,
    hasPermission,
    getAuthorizedMenus,
    loadPermissionTree,
    clearPermissions,
    initPermissions,
    savePermissionsToStorage
  }
}) 