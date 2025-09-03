<template>
  <div class="dashboard">
    <!-- 欢迎横幅 -->
    <div class="welcome-banner">
      <div class="welcome-content">
        <h1 class="welcome-title">欢迎回来，{{ authStore.user?.name || '管理员' }}！</h1>
        <p class="welcome-subtitle">今天是美好的一天，让我们开始工作吧</p>
      </div>
      <div class="welcome-illustration">
        <el-icon size="80" color="#667eea"><Monitor /></el-icon>
      </div>
    </div>

    <!-- 统计卡片 -->
    <el-row :gutter="24" class="stats-row">
      <el-col :xs="24" :sm="12" :md="6" v-for="(stat, index) in statsData" :key="index">
        <div class="stat-card hover-card" :class="stat.class">
          <div class="stat-content">
            <div class="stat-icon">
              <el-icon :size="32"><component :is="stat.icon" /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-number">{{ stat.value }}</div>
              <div class="stat-label">{{ stat.label }}</div>
              <div class="stat-trend" :class="stat.trend > 0 ? 'trend-up' : 'trend-down'">
                <el-icon size="12">
                  <component :is="stat.trend > 0 ? 'ArrowUp' : 'ArrowDown'" />
                </el-icon>
                <span>{{ Math.abs(stat.trend) }}%</span>
                <span class="trend-text">较上月</span>
              </div>
            </div>
          </div>
        </div>
      </el-col>
    </el-row>
    
    <!-- 主要内容区域 -->
    <el-row :gutter="24" class="content-row">
      <el-col :xs="24" :lg="16">
        <!-- 最近活动 -->
        <el-card class="activity-card hover-card">
          <template #header>
            <div class="card-header">
              <div class="header-left">
                <el-icon size="20" color="#667eea"><Clock /></el-icon>
                <span class="header-title">最近登录用户</span>
              </div>
              <el-button type="text" class="view-all-btn">
                查看全部
                <el-icon><ArrowRight /></el-icon>
              </el-button>
            </div>
          </template>
          
          <div class="activity-list">
            <div 
              v-for="(user, index) in recentUsers" 
              :key="index"
              class="activity-item"
              :class="{ 'activity-item-new': index < 2 }"
            >
              <div class="activity-avatar">
                <el-avatar :size="40">
                  <el-icon><UserFilled /></el-icon>
                </el-avatar>
                <div class="activity-status" :class="user.status"></div>
              </div>
              <div class="activity-content">
                <div class="activity-title">{{ user.name }}</div>
                <div class="activity-desc">{{ user.email }}</div>
                <div class="activity-time">{{ user.lastLoginTime }}</div>
              </div>
              <div class="activity-action">
                <el-button type="text" size="small">查看详情</el-button>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
      
      <el-col :xs="24" :lg="8">
        <!-- 系统信息 -->
        <el-card class="system-card hover-card">
          <template #header>
            <div class="card-header">
              <div class="header-left">
                <el-icon size="20" color="#667eea"><Monitor /></el-icon>
                <span class="header-title">系统信息</span>
              </div>
            </div>
          </template>
          
          <div class="system-info">
            <div class="info-item">
              <div class="info-label">
                <el-icon size="16" color="#64748b"><InfoFilled /></el-icon>
                <span>系统版本</span>
              </div>
              <div class="info-value">CleanDDD v1.0.0</div>
            </div>
            
            <div class="info-item">
              <div class="info-label">
                <el-icon size="16" color="#64748b"><Timer /></el-icon>
                <span>运行时间</span>
              </div>
              <div class="info-value">{{ uptime }}</div>
            </div>
            
            <div class="info-item">
              <div class="info-label">
                <el-icon size="16" color="#64748b"><Server /></el-icon>
                <span>服务器状态</span>
              </div>
              <div class="info-value status-normal">
                <el-icon size="12"><CircleCheck /></el-icon>
                正常
              </div>
            </div>
            
            <div class="info-item">
              <div class="info-label">
                <el-icon size="16" color="#64748b"><DataBase /></el-icon>
                <span>数据库状态</span>
              </div>
              <div class="info-value status-normal">
                <el-icon size="12"><CircleCheck /></el-icon>
                正常
              </div>
            </div>
          </div>
          
          <!-- 快速操作 -->
          <div class="quick-actions">
            <h4 class="actions-title">快速操作</h4>
            <div class="actions-grid">
              <el-button 
                v-for="action in quickActions" 
                :key="action.name"
                :type="action.type" 
                size="small"
                class="action-btn"
                @click="handleQuickAction(action)"
              >
                <el-icon size="16"><component :is="action.icon" /></el-icon>
                {{ action.name }}
              </el-button>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'

const authStore = useAuthStore()

const statsData = ref([
  {
    label: '用户总数',
    value: 1250,
    icon: 'User',
    class: 'stat-user',
    trend: 12.5
  },
  {
    label: '角色总数',
    value: 8,
    icon: 'Setting',
    class: 'stat-role',
    trend: 0
  },
  {
    label: '活跃用户',
    value: 89,
    icon: 'UserFilled',
    class: 'stat-active',
    trend: 8.3
  },
  {
    label: '系统状态',
    value: '正常',
    icon: 'Monitor',
    class: 'stat-system',
    trend: 0
  }
])

const recentUsers = ref([
  {
    name: 'admin',
    email: 'admin@example.com',
    lastLoginTime: '2024-01-15 10:30:00',
    status: 'online'
  },
  {
    name: 'user1',
    email: 'user1@example.com',
    lastLoginTime: '2024-01-15 09:15:00',
    status: 'online'
  },
  {
    name: 'user2',
    email: 'user2@example.com',
    lastLoginTime: '2024-01-15 08:45:00',
    status: 'offline'
  },
  {
    name: 'user3',
    email: 'user3@example.com',
    lastLoginTime: '2024-01-14 16:20:00',
    status: 'offline'
  }
])

const quickActions = ref([
  { name: '新建用户', icon: 'Plus', type: 'primary' },
  { name: '角色管理', icon: 'Setting', type: 'success' },
  { name: '系统设置', icon: 'Tools', type: 'warning' },
  { name: '数据备份', icon: 'Download', type: 'info' }
])

const uptime = ref('0天 0小时 0分钟')

const handleQuickAction = (action: any) => {
  ElMessage.info(`${action.name}功能开发中...`)
}

onMounted(() => {
  // 模拟运行时间
  const startTime = new Date('2024-01-01')
  const now = new Date()
  const diff = now.getTime() - startTime.getTime()
  const days = Math.floor(diff / (1000 * 60 * 60 * 24))
  const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))
  
  uptime.value = `${days}天 ${hours}小时 ${minutes}分钟`
})
</script>

<style scoped>
.dashboard {
  padding: 0;
}

.welcome-banner {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 16px;
  padding: 32px;
  margin-bottom: 32px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: white;
  box-shadow: 0 8px 32px rgba(102, 126, 234, 0.3);
}

.welcome-content {
  flex: 1;
}

.welcome-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0 0 8px 0;
  line-height: 1.2;
}

.welcome-subtitle {
  font-size: 16px;
  opacity: 0.9;
  margin: 0;
  font-weight: 400;
}

.welcome-illustration {
  opacity: 0.8;
}

.stats-row {
  margin-bottom: 32px;
}

.stat-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  height: 100%;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);
}

.stat-content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-icon {
  width: 64px;
  height: 64px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-user .stat-icon {
  background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);
  color: #1976d2;
}

.stat-role .stat-icon {
  background: linear-gradient(135deg, #f3e5f5 0%, #e1bee7 100%);
  color: #7b1fa2;
}

.stat-active .stat-icon {
  background: linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%);
  color: #388e3c;
}

.stat-system .stat-icon {
  background: linear-gradient(135deg, #fff3e0 0%, #ffcc80 100%);
  color: #f57c00;
}

.stat-info {
  flex: 1;
  min-width: 0;
}

.stat-number {
  font-size: 32px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 4px;
  line-height: 1;
}

.stat-label {
  font-size: 14px;
  color: #64748b;
  margin-bottom: 8px;
  font-weight: 500;
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  font-weight: 500;
}

.trend-up {
  color: #10b981;
}

.trend-down {
  color: #ef4444;
}

.trend-text {
  color: #94a3b8;
  margin-left: 4px;
}

.content-row {
  margin-bottom: 32px;
}

.activity-card,
.system-card {
  border-radius: 16px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  height: 100%;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.header-title {
  font-size: 16px;
  font-weight: 600;
  color: #1e293b;
}

.view-all-btn {
  color: #667eea;
  font-weight: 500;
  padding: 4px 8px;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.view-all-btn:hover {
  background: #f1f5f9;
  color: #4f46e5;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border-radius: 12px;
  background: #f8fafc;
  transition: all 0.2s ease;
  position: relative;
}

.activity-item:hover {
  background: #f1f5f9;
  transform: translateX(4px);
}

.activity-item-new {
  background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
  border: 1px solid #f59e0b;
}

.activity-item-new:hover {
  background: linear-gradient(135deg, #fde68a 0%, #fcd34d 100%);
}

.activity-avatar {
  position: relative;
  flex-shrink: 0;
}

.activity-status {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid white;
}

.activity-status.online {
  background: #10b981;
}

.activity-status.offline {
  background: #94a3b8;
}

.activity-content {
  flex: 1;
  min-width: 0;
}

.activity-title {
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 4px;
  font-size: 14px;
}

.activity-desc {
  color: #64748b;
  font-size: 12px;
  margin-bottom: 4px;
}

.activity-time {
  color: #94a3b8;
  font-size: 11px;
}

.activity-action {
  flex-shrink: 0;
}

.system-info {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-bottom: 32px;
}

.info-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px;
  background: #f8fafc;
  border-radius: 12px;
  transition: all 0.2s ease;
}

.info-item:hover {
  background: #f1f5f9;
}

.info-label {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #64748b;
  font-size: 14px;
  font-weight: 500;
}

.info-value {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #1e293b;
  font-weight: 600;
  font-size: 14px;
}

.status-normal {
  color: #10b981;
}

.quick-actions {
  border-top: 1px solid #e2e8f0;
  padding-top: 24px;
}

.actions-title {
  font-size: 16px;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 16px 0;
}

.actions-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.action-btn {
  height: 40px;
  border-radius: 8px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.action-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .welcome-banner {
    flex-direction: column;
    text-align: center;
    gap: 20px;
    padding: 24px;
  }
  
  .welcome-title {
    font-size: 24px;
  }
  
  .welcome-illustration {
    display: none;
  }
  
  .stat-content {
    flex-direction: column;
    text-align: center;
    gap: 12px;
  }
  
  .stat-icon {
    width: 48px;
    height: 48px;
  }
  
  .stat-number {
    font-size: 24px;
  }
  
  .activity-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .activity-action {
    align-self: flex-end;
  }
  
  .actions-grid {
    grid-template-columns: 1fr;
  }
}
</style> 