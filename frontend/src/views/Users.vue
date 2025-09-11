<template>
  <div class="management-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1>用户管理</h1>
          <p class="subtitle">管理系统中的所有用户账户和权限</p>
        </div>
        <el-button 
          v-permission="[PERMISSIONS.USER_CREATE]"
          type="primary" 
          size="large"
          class="create-btn"
          @click="showCreateDialog"
          icon="Plus"
        >
          新建用户
        </el-button>
      </div>
    </div>

    <div class="main-content">
      <!-- 搜索栏 -->
      <div class="search-section">
        <div class="section-header">
          <h2>
            <el-icon class="section-icon"><Search /></el-icon>
            搜索筛选
          </h2>
        </div>
        <div class="search-wrapper">
          <el-form :inline="true" :model="searchParams" class="search-form">
            <el-form-item label="搜索">
              <el-input
                v-model="searchParams.keyword"
                placeholder="请输入用户名"
                clearable
                class="search-input"
                :prefix-icon="Search"
                @keyup.enter="() => handleSearch()"
              />
            </el-form-item>
            <el-form-item label="状态">
              <el-select 
                v-model="searchParams.status" 
                placeholder="请选择状态" 
                clearable 
                class="status-select"
              >
                <el-option label="启用" :value="USER_STATUS.ENABLED" />
                <el-option label="禁用" :value="USER_STATUS.DISABLED" />
              </el-select>
            </el-form-item>
            <el-form-item label="组织架构">
              <el-tree-select
                v-model="searchParams.organizationUnitId"
                :data="organizationTreeOptions"
                placeholder="请选择组织架构"
                clearable
                check-strictly
                :render-after-expand="false"
                :props="{
                  value: 'id',
                  label: 'name',
                  children: 'children'
                }"
                class="org-select"
              />
            </el-form-item>
            <el-form-item>
              <div class="action-buttons">
                <el-button type="primary" class="search-btn" @click="() => handleSearch()" icon="Search">
                  搜索 
                </el-button>
                <!-- <el-button class="reset-btn" @click="handleReset">
                  <el-icon><Refresh /></el-icon>
                  重置
                </el-button> -->
                <el-button 
                  v-permission="['UserCreate']"
                  type="success" 
                  class="template-btn" 
                  @click="handleDownloadTemplate"
                  icon="Download"
                >
                  下载模板
                </el-button>
                <el-button 
                  v-permission="['UserCreate']"
                  type="warning" 
                  class="import-btn" 
                  @click="showImportDialog"
                  icon="Upload"
                >
                  用户导入
                </el-button>
              </div>
            </el-form-item>
          </el-form>
        </div>
      </div>
      
      <!-- 用户列表 -->
      <div class="table-section">
        <div class="section-header">
          <div class="header-left">
            <h2>
              <el-icon class="section-icon"><User /></el-icon>
              用户列表
            </h2>
            <el-tag type="info" class="count-tag">{{ pagination.total }} 个用户</el-tag>
          </div>
          <div class="header-right">
            <el-button 
              v-if="selectedUsers.length > 0"
              v-permission="['UserDelete']"
              type="danger" 
              size="small"
              class="batch-delete-btn"
              @click="handleBatchDelete"
              icon="Delete"
            >
              批量删除 ({{ selectedUsers.length }})
            </el-button>
            <el-button 
              v-if="selectedUsers.length > 0"
              v-permission="['UserResetPassword']"
              type="primary" 
              size="small"
              class="batch-reset-password-btn"
              @click="handleBatchResetPassword"
              icon="Refresh"
            >
              批量重置密码 ({{ selectedUsers.length }})
            </el-button>
          </div>
        </div>
        
        <div class="table-wrapper">
          <el-table
            v-loading="loading"
            :data="users"
            class="data-table"
            @selection-change="handleSelectionChange"
            :header-cell-style="{ backgroundColor: '#f8fafc', color: '#374151', fontWeight: '600' }"
            stripe
          >
            <el-table-column type="selection" width="55" />
            <el-table-column prop="name" label="用户名" min-width="120">
              <template #default="{ row }">
                <div class="entity-cell">
                  <el-avatar :size="32" class="entity-avatar">
                    <el-icon><UserFilled /></el-icon>
                  </el-avatar>
                  <div class="entity-info">
                    <div class="entity-name">{{ row.name }}</div>
                    <div class="entity-description">{{ row.realName }}</div>
                  </div>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="gender" label="性别" width="80" align="center">
              <template #default="{ row }">
                <el-tag 
                  :type="row.gender === '男' ? 'primary' : 'danger'" 
                  class="gender-tag"
                  size="small"
                >
                  {{ row.gender || '未设置' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="age" label="年龄" width="80" align="center">
              <template #default="{ row }">
                <span class="age-text">{{ row.age || '未设置' }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="status" label="状态" width="100" align="center">
              <template #default="{ row }">
                <el-tag 
                  :type="row.status === 1 ? 'success' : 'danger'" 
                  class="status-tag"
                  size="small"
                >
                  <el-icon size="12">
                    <component :is="row.status === 1 ? 'CircleCheck' : 'CircleClose'" />
                  </el-icon>
                  {{ row.status === 1 ? '启用' : '禁用' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="roles" label="角色" min-width="150">
              <template #default="{ row }">
                <div class="roles-container">
                  <el-tag
                    v-for="role in row.roles"
                    :key="role"
                    type="info"
                    class="role-tag"
                    size="small"
                  >
                    <el-icon size="12"><Setting /></el-icon>
                    {{ role }}
                  </el-tag>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="organizationUnitName" label="组织架构" min-width="120">
              <template #default="{ row }">
                <div class="org-cell" v-if="row.organizationUnitName">
                  <el-icon class="org-icon"><OfficeBuilding /></el-icon>
                  <span>{{ row.organizationUnitName }}</span>
                </div>
                <span v-else class="empty-text">未分配</span>
              </template>
            </el-table-column>
            <el-table-column prop="createdAt" label="创建时间" min-width="160" align="center">
              <template #default="{ row }">
                <div class="time-cell">
                  <el-icon class="time-icon"><Clock /></el-icon>
                  <span>{{ formatDate(row.createdAt) }}</span>
                </div>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="200" fixed="right" align="center">
              <template #default="{ row }">
                <div class="table-actions">
                  <el-button 
                    v-permission="['UserEdit']"
                    size="small" 
                    type="primary" 
                    title="编辑"
                    :icon="Edit"
                    circle
                    class="table-action-btn"
                    @click="handleEdit(row)"
                  />
                  <el-button 
                    v-permission="['UserRoleAssign']"
                    size="small" 
                    type="warning" 
                    title="分配角色"
                    :icon="Setting"
                    circle
                    class="table-action-btn"
                    @click="handleRoles(row)"
                  />
                  <el-button 
                    v-permission="['UserResetPassword']"
                    size="small" 
                    type="primary" 
                    title="重置密码"
                    :icon="Refresh"
                    circle
                    class="table-action-btn"
                    @click="handleResetPassword(row)"
                  />
                  <el-button 
                    v-permission="['UserDelete']"
                    size="small" 
                    type="danger" 
                    title="删除"
                    :icon="Delete"
                    circle
                    class="table-action-btn"
                    @click="handleDelete(row)"
                  />
                </div>
              </template>
            </el-table-column>
          </el-table>
          
          <div v-if="users.length === 0" class="empty-state">
            <el-icon class="empty-icon"><DocumentRemove /></el-icon>
            <p>暂无用户数据</p>
          </div>
          
          <!-- 分页 -->
          <div class="pagination-wrapper">
            <el-pagination
              v-model:current-page="pagination.pageIndex"
              v-model:page-size="pagination.pageSize"
              :total="pagination.total"
              :page-sizes="[10, 20, 50, 100]"
              layout="total, sizes, prev, pager, next, jumper"
              class="pagination"
              @update:page-size="handlePageSizeChange"
              @update:current-page="handlePageCurrentChange"
            />
          </div>
        </div>
      </div>
    </div>
    
    <!-- 创建/编辑用户对话框 -->
    <UserFormDialog
      v-model:visible="dialogVisible"
      :is-edit="isEdit"
      :user-data="currentUser"
      :organization-tree-options="organizationTreeOptions"
      @success="handleUserFormSuccess"
    />
    
    <!-- 分配角色对话框 -->
    <UserRoleAssignDialog
      v-model:visible="roleDialogVisible"
      :user-data="currentUser"
      :all-roles="allRoles"
      @success="handleRoleAssignSuccess"
    />

    <!-- 批量导入对话框 -->
    <UserImportDialog
      v-model:visible="importDialogVisible"
      :all-roles="allRoles"
      :organization-tree-options="organizationTreeOptions"
      @success="handleImportSuccess"
    />

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import {
  Search, 
  Edit, 
  Delete, 
  Setting, 
  OfficeBuilding, 
  Clock, 
  DocumentRemove,
  Refresh
} from '@element-plus/icons-vue'
import { resetPassword, getUsers, deleteUser, downloadUserTemplate, type GetUsersRequest } from '@/api/user'
import { UserFormDialog, UserRoleAssignDialog, UserImportDialog } from '@/components'
import { getAllRoles } from '@/api/role'
import { organizationApi, type OrganizationUnitTree } from '@/api/organization'
import type { User, Role, OrganizationUnit } from '@/types'
import { useTable, useConfirm } from '@/composables'
import { DateFormatter } from '@/utils/format'
import { ErrorHandler } from '@/utils/error'
import { hasPermission } from '@/utils/permission'
import { PERMISSIONS, USER_STATUS } from '@/constants'

// 对话框状态
const dialogVisible = ref(false)
const roleDialogVisible = ref(false)
const importDialogVisible = ref(false)
const isEdit = ref(false)
const currentUser = ref<User | null>(null)

// 角色和组织架构数据
const allRoles = ref<Role[]>([])
const organizationOptions = ref<OrganizationUnit[]>([])
const organizationTreeOptions = ref<OrganizationUnitTree[]>([])

// 使用表格Composable
const {
  data: users,
  selectedRows: selectedUsers,
  loading,
  pagination,
  searchParams,
  loadData,
  handleSearch,
  handleSelectionChange,
  refresh,
  handlePageSizeChange,
  handlePageCurrentChange
} = useTable<User>(async (params: GetUsersRequest) => {
  const response = await getUsers(params)
  return response.data
})

// 扩展搜索参数类型
Object.assign(searchParams, {
  keyword: '',
  status: null as number | null,
  organizationUnitId: null as number | null
})

// 确认对话框
const { confirmDelete, confirmBatchDelete } = useConfirm()



// 在树形结构中递归查找指定ID的组织架构
const findOrganizationById = (treeData: OrganizationUnitTree[], id: number): OrganizationUnitTree | null => {
  for (const item of treeData) {
    if (item.id === id) {
      return item;
    }
    if (item.children && item.children.length > 0) {
      const found = findOrganizationById(item.children, id);
      if (found) {
        return found;
      }
    }
  }
  return null;
}





const loadOrganizationUnits = async () => {
  // 检查是否有权限
  if (!hasPermission([PERMISSIONS.ORG_VIEW])) {
    return
  }

  try {
    // 加载平铺的组织架构数据（用于某些场景）
    const flatResponse = await organizationApi.getAll(true)
    organizationOptions.value = flatResponse.data
    
    // 加载树形的组织架构数据（用于树形选择器）
    const treeResponse = await organizationApi.getTree(true)
    organizationTreeOptions.value = treeResponse.data
  } catch (error) {
    ErrorHandler.handle(error, 'loadOrganizationUnits')
  }
}

// loadUsers已被useTable Composable替代，删除此方法

const loadRoles = async () => {
  // 检查是否有权限
  if (!hasPermission([PERMISSIONS.ROLE_VIEW])) {
    return
  }

  try {
    const response = await getAllRoles({
      pageIndex: 1,
      pageSize: 100,
      countTotal: false
    })
    allRoles.value = response.data.items
  } catch (error) {
    ErrorHandler.handle(error, 'loadRoles')
  }
}

// handleSearch, handleSelectionChange等方法已被useTable Composable提供，删除重复方法

const showCreateDialog = () => {
  isEdit.value = false
  currentUser.value = null
  dialogVisible.value = true
}

const handleEdit = (user: User) => {
  isEdit.value = true
  currentUser.value = user
  dialogVisible.value = true
}

const handleResetPassword = async (user: User) => {
  try {
    const confirmed = await confirmDelete(`重置用户"${user.name}"的密码`)
    if (confirmed) {
      await resetPassword(user.userId)
      ElMessage.success('重置成功')
    }
  } catch (error) {
    ErrorHandler.handle(error, 'resetPassword')
  }
}

const handleRoles = (user: User) => {
  currentUser.value = user
  roleDialogVisible.value = true
}



const handleDelete = async (user: User) => {
  try {
    const confirmed = await confirmDelete(user.name)
    if (confirmed) {
      await deleteUser(user.userId)
      ElMessage.success('删除成功')
      refresh()
    }
  } catch (error) {
    ErrorHandler.handle(error, 'deleteUser')
  }
}



const formatDate = (dateString: string) => DateFormatter.toDateTimeString(dateString)

// 下载模板
const handleDownloadTemplate = async () => {
  try {
    const blob = await downloadUserTemplate()
    
    // 创建下载链接
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = '用户导入模板.xlsx'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    
    ElMessage.success('模板下载成功')
  } catch (error: any) {
    ElMessage.error('模板下载失败')
  }
}

const handleBatchDelete = async () => {
  if (selectedUsers.value.length === 0) {
    ElMessage.error('请选择要删除的用户')
    return
  }
  
  try {
    const confirmed = await confirmBatchDelete(selectedUsers.value.length)
    if (confirmed) {
      for (const user of selectedUsers.value) {
        await deleteUser(user.userId)
      }
      ElMessage.success('删除成功')
      refresh()
    }
  } catch (error) {
    ErrorHandler.handle(error, 'batchDelete')
    refresh()
  }
}

const handleBatchResetPassword = async () => {
  if (selectedUsers.value.length === 0) { 
    ElMessage.error('请选择要重置密码的用户')
    return
  }
  try {
    for (const user of selectedUsers.value) {
      await resetPassword(user.userId)
    }
    ElMessage.success('重置密码成功')
    refresh()
  } catch (error) {
    ErrorHandler.handle(error, 'batchResetPassword')
    refresh()
  }
}

// 显示导入对话框
const showImportDialog = () => {
  importDialogVisible.value = true
}

// 用户表单成功处理
const handleUserFormSuccess = () => {
  refresh()
}

// 角色分配成功处理
const handleRoleAssignSuccess = () => {
  refresh()
}

// 导入成功处理
const handleImportSuccess = () => {
  refresh()
}

onMounted(async () => {
  await Promise.all([
    loadData(), // 加载用户数据
    loadRoles(),
    loadOrganizationUnits()
  ])
})
</script>

<style scoped>
/* ===== 用户管理页面特有样式 ===== */

/* 搜索表单特有样式 */
.status-select,
.org-select {
  width: 140px;
}

</style> 