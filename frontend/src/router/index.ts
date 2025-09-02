import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { usePermissionStore } from '@/stores/permission'
import { menuRouteConfigs, convertToRouteConfig } from './menu-config'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('@/views/Login.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/register',
      name: 'Register',
      component: () => import('@/views/Register.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/',
      component: () => import('@/layouts/MainLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        // {
        //   path: '',
        //   name: 'Dashboard',
        //   component: () => import('@/views/Dashboard.vue'),
        //   meta: { permissions: ['SystemMonitor'] }
        // },
        ...menuRouteConfigs.map(convertToRouteConfig),
         {
          path: '',
          name: 'Users',
          component: () => import('@/views/Users.vue'),
          meta: { permissions: ['SystemMonitor'] }
        },
       
      ]
    }
  ]
})

// 路由守卫
router.beforeEach((to, _, next) => {
  const authStore = useAuthStore()
  const permissionStore = usePermissionStore()
  
  // 如果访问登录页面且已认证，跳转到首页
  if (to.path === '/login' && authStore.isAuthenticated) {
    next('/')
    return
  }
  
  // 如果访问注册页面且已认证，跳转到首页
  if (to.path === '/register' && authStore.isAuthenticated) {
    next('/')
    return
  }
  
  // 如果需要认证但未认证，跳转到登录页面
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }
  
  // 如果需要认证且已认证，检查页面权限
  if (to.meta.requiresAuth && authStore.isAuthenticated) {
    const requiredPermissions = to.meta.permissions as string[]
    if (requiredPermissions && requiredPermissions.length > 0 && !permissionStore.hasPermission(requiredPermissions)) {
      // 如果没有权限，尝试跳转到有权限的第一个菜单
      const authorizedMenus = permissionStore.getAuthorizedMenus
      if (authorizedMenus.length > 0) {
        next(authorizedMenus[0].path)
      } else {
        // 如果没有任何权限，跳转到登录页面
        authStore.logout()
        next('/login')
      }
      return
    }
  }
  
  next()
})

export default router 