<template>
  <el-container class="layout-container">
    <!-- 侧边栏 -->
    <el-aside :width="sidebarWidth" class="sidebar" :class="{ 'sidebar-collapsed': isCollapsed }">
      <div class="logo">
        <div class="logo-icon">
          <el-icon size="24"><Monitor /></el-icon>
          <!-- <img src="@/assets/images/logo.png" alt="Logo" class="logo-image" /> -->
        </div>
        <!-- <h2 v-show="!isCollapsed">CleanDDD</h2> -->
      </div>
      
      <el-menu
        :default-active="getActiveMenuPath"
        class="sidebar-menu"
        router
        :collapse="isCollapsed"
        background-color="transparent"
        text-color="#e1e5e9"
        active-text-color="#409EFF"
      >
        <el-menu-item 
          v-for="menu in permissionStore.getAuthorizedMenus" 
          :key="menu.path"
          :index="menu.path" 
          class="menu-item"
        >
          <el-icon>
            <component :is="menu.icon" />
          </el-icon>
          <template #title>
            <span class="menu-text">{{ menu.name }}</span>
          </template>
        </el-menu-item>
      </el-menu>
    </el-aside>
    
    <!-- 主内容区 -->
    <el-container>
      <!-- 顶部导航 -->
      <el-header class="header">
        <div class="header-left">
          <el-button
            type="text"
            class="collapse-btn"
            @click="toggleSidebar"
          >
            <el-icon size="20">
              <Fold v-if="!isCollapsed" />
              <Expand v-else />
            </el-icon>
          </el-button>
          
          <el-breadcrumb separator="/" class="breadcrumb">
            <el-breadcrumb-item :to="{ path: '/' }">首页</el-breadcrumb-item>
            <el-breadcrumb-item v-if="$route.name !== 'Dashboard'">
              {{ getPageTitle() }}
            </el-breadcrumb-item>
          </el-breadcrumb>
        </div>
        
        <div class="header-right">
          <!-- <el-button
            type="text"
            class="notification-btn"
            @click="showNotifications"
          >
            <el-badge :value="3" class="notification-badge">
              <el-icon size="20"><Bell /></el-icon>
            </el-badge>
          </el-button> -->
          
          <el-dropdown @command="handleCommand" class="user-dropdown">
            <div class="user-info">
              <el-avatar :size="36" class="user-avatar">
                <el-icon><UserFilled /></el-icon>
              </el-avatar>
              <div class="user-details" v-show="!isMobile">
                <div class="username">{{ authStore.user?.name || '用户' }}</div>
                <div class="user-role">管理员</div>
              </div>
              <el-icon class="dropdown-icon"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu class="user-dropdown-menu">
                <el-dropdown-item command="profile" class="dropdown-item">
                  <el-icon><User /></el-icon>
                  <span>个人资料</span>
                </el-dropdown-item>
                <el-dropdown-item 
                  v-if="permissionStore.hasPermission(['SystemAdmin'])"
                  command="settings" 
                  class="dropdown-item"
                >
                  <el-icon><Setting /></el-icon>
                  <span>系统设置</span>
                </el-dropdown-item>
                <el-dropdown-item divided command="logout" class="dropdown-item">
                  <el-icon><SwitchButton /></el-icon>
                  <span>退出登录</span>
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>
      
      <!-- 内容区 -->
      <el-main class="main-content">
        <div class="content-wrapper">
          <router-view />
        </div>
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessageBox, ElMessage } from 'element-plus'
import { useAuthStore } from '@/stores/auth'
import { usePermissionStore } from '@/stores/permission'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const permissionStore = usePermissionStore()

const isCollapsed = ref(false)
const isMobile = ref(false)

const sidebarWidth = computed(() => {
  if (isMobile.value) return '0px'
  return isCollapsed.value ? '64px' : '250px'
})

// 获取当前应该激活的菜单路径
const getActiveMenuPath = computed(() => {
  // 如果当前路由的 meta 中有 activeMenu 属性，则使用该值
  if (route.meta.activeMenu) {
    return route.meta.activeMenu as string
  }
  // 否则使用当前路由路径
  return route.path
})

const getPageTitle = () => {
  const routeMap: Record<string, string> = {
    'Users': '用户管理',
    'Roles': '角色管理',
    'Profile': '个人资料'
  }
  
  // 从权限菜单中查找对应的页面标题
  const currentMenu = permissionStore.getAuthorizedMenus.find(menu => menu.path === route.path)
  if (currentMenu) {
    return currentMenu.name
  }
  
  return routeMap[route.name as string] || ''
}

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
}

// const showNotifications = () => {
//   ElMessage.info('通知功能开发中...')
// }

const handleCommand = async (command: string) => {
  switch (command) {
    case 'profile':
      router.push('/profile')
      break
    case 'settings':
      if (permissionStore.hasPermission(['SystemAdmin'])) {
        ElMessage.info('系统设置功能开发中...')
      } else {
        ElMessage.error('您没有访问系统设置的权限')
      }
      break
    case 'logout':
      try {
        await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning',
          customClass: 'custom-message-box'
        })
        authStore.logout()
        router.push('/login')
        ElMessage.success('已成功退出登录')
      } catch {
        // 用户取消
      }
      break
  }
}

const checkMobile = () => {
  isMobile.value = window.innerWidth <= 768
  if (isMobile.value) {
    isCollapsed.value = true
  }
}

onMounted(() => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})
</script>

<style scoped>
.layout-container {
  height: 100vh;
  background: #f8fafc;
}

.sidebar {
  background: linear-gradient(180deg, #1e293b 0%, #334155 100%);
  color: white;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.1);
  position: relative;
  z-index: 1000;
}

.sidebar-collapsed {
  width: 64px !important;
}

.logo {
  height: 70px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  padding: 0 20px;
  gap: 12px;
}

.logo-icon {
  width: 100%;
  height: 100%;
  /* background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); */
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  flex-shrink: 0;
}

.logo-image {
  width: 100%;
  height: 80%;
  object-fit: contain;
  border-radius: 12px;
}

.logo h2 {
  color: white;
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
}

.sidebar-menu {
  border: none;
  background: transparent;
  margin-top: 20px;
}

.menu-item {
  margin: 4px 12px;
  border-radius: 8px;
  height: 48px;
  line-height: 48px;
  transition: all 0.3s ease;
}

.menu-item:hover {
  background: rgba(255, 255, 255, 0.1) !important;
  transform: translateX(4px);
}

.menu-item.is-active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.menu-text {
  font-weight: 500;
  font-size: 14px;
}

.header {
  background: white;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  position: relative;
  z-index: 999;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 20px;
}

.collapse-btn {
  color: #64748b;
  padding: 8px;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.collapse-btn:hover {
  background: #f1f5f9;
  color: #334155;
}

.breadcrumb {
  font-size: 14px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.notification-btn {
  color: #64748b;
  padding: 8px;
  border-radius: 8px;
  transition: all 0.2s ease;
  position: relative;
}

.notification-btn:hover {
  background: #f1f5f9;
  color: #334155;
}

.notification-badge :deep(.el-badge__content) {
  background: #ef4444;
  border: 2px solid white;
}

.user-dropdown {
  cursor: pointer;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  border-radius: 12px;
  transition: all 0.2s ease;
  min-width: 120px;
}

.user-info:hover {
  background: #f8fafc;
}

.user-avatar {
  border: 2px solid #e2e8f0;
  transition: all 0.2s ease;
}

.user-info:hover .user-avatar {
  border-color: #667eea;
}

.user-details {
  flex: 1;
  min-width: 0;
}

.username {
  font-weight: 600;
  color: #1e293b;
  font-size: 14px;
  line-height: 1.2;
}

.user-role {
  color: #64748b;
  font-size: 12px;
  line-height: 1.2;
}

.dropdown-icon {
  color: #94a3b8;
  transition: transform 0.2s ease;
}

.user-info:hover .dropdown-icon {
  transform: rotate(180deg);
}

.user-dropdown-menu {
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
  border: 1px solid #e2e8f0;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  font-size: 14px;
  transition: all 0.2s ease;
}

.dropdown-item:hover {
  background: #f8fafc;
  color: #667eea;
}

.main-content {
  background: #f8fafc;
  padding: 24px;
  overflow-y: auto;
}

.content-wrapper {
  max-width: 1400px;
  margin: 0 auto;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .header {
    padding: 0 16px;
  }
  
  .main-content {
    padding: 16px;
  }
  
  .user-details {
    display: none;
  }
  
  .user-info {
    min-width: auto;
    padding: 8px;
  }
  
  .breadcrumb {
    display: none;
  }
}

/* 自定义消息框样式 */
:deep(.custom-message-box) {
  border-radius: 12px;
}

:deep(.custom-message-box .el-message-box__header) {
  padding: 20px 20px 0;
}

:deep(.custom-message-box .el-message-box__content) {
  padding: 20px;
}

:deep(.custom-message-box .el-message-box__btns) {
  padding: 0 20px 20px;
}
</style> 