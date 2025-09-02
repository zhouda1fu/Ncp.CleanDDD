import type { Directive } from 'vue'
import { usePermissionStore } from '@/stores/permission'

// 权限指令
export const permission: Directive = {
  mounted(el, binding) {
    const permissionStore = usePermissionStore()
    const requiredPermissions = binding.value
    
    if (!permissionStore.hasPermission(requiredPermissions)) {
      // 如果没有权限，隐藏元素
      el.style.display = 'none'
    }
  },
  
  updated(el, binding) {
    const permissionStore = usePermissionStore()
    const requiredPermissions = binding.value
    
    if (permissionStore.hasPermission(requiredPermissions)) {
      // 如果有权限，显示元素
      el.style.display = ''
    } else {
      // 如果没有权限，隐藏元素
      el.style.display = 'none'
    }
  }
}

// 权限指令的简写形式
export const vPermission = permission 