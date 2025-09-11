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

/* 搜索表单特有样式 */
.status-select {
  width: 140px;
}

.org-select {
  width: 140px;
}

/* ===== 批量导入对话框样式 ===== */

/* 对话框内容容器 */
.import-dialog-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  max-height: 70vh;
  overflow-y: auto;
}

/* 导入说明区域 */
.import-tips {
  background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
  border: 1px solid #f59e0b;
  border-radius: 12px;
  padding: 1.25rem;
  position: relative;
  overflow: hidden;
}

.import-tips::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.import-tips .tips-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.import-tips .tips-title {
  font-weight: 600;
  color: #92400e;
  font-size: 1rem;
}

.import-tips .tips-list {
  list-style: none;
  padding: 0;
  margin: 0;
  margin-bottom: 20px;
}

.import-tips .tips-list li {
  position: relative;
  padding-left: 1.25rem;
  margin-bottom: 0.5rem;
  color: #92400e;
  font-size: 0.875rem;
  line-height: 1.5;
}

.import-tips .tips-list li::before {
  content: '⚠️';
  position: absolute;
  left: 0;
  top: 0;
  font-size: 0.75rem;
}

.import-tips .tips-list li:last-child {
  margin-bottom: 0;
}

/* 批量设置区域 */
.batch-settings {
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  border: 1px solid #0ea5e9;
  border-radius: 12px;
  padding: 1.25rem;
  position: relative;
  overflow: hidden;
}

.batch-settings::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  background: linear-gradient(135deg, #0ea5e9 0%, #0284c7 100%);
}

.batch-settings .settings-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.batch-settings .settings-title {
  font-weight: 600;
  color: #0c4a6e;
  font-size: 1rem;
}

/* 导入设置表单 */
.import-settings-form {
  background: white;
  border-radius: 8px;
  padding: 1rem;
  border: 1px solid #e2e8f0;
}

.import-settings-form .el-form-item {
  margin-bottom: 1rem;
}

.import-settings-form .el-form-item:last-child {
  margin-bottom: 0;
}

.import-settings-form .full-width {
  width: 100%;
}

/* 文件上传区域 */
.upload-demo {
  background: white;
  border: 2px dashed #d1d5db;
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.upload-demo::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  opacity: 0.5;
  z-index: -1;
}

.upload-demo:hover {
  border-color: #6366f1;
  background: linear-gradient(135deg, #f8f9ff 0%, #f0f4ff 100%);
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(99, 102, 241, 0.15);
}

.upload-demo .el-upload-dragger {
  background: transparent;
  border: none;
  width: 100%;
  height: 100%;
}

.upload-demo .el-icon {
  font-size: 3rem;
  color: #9ca3af;
  margin-bottom: 1rem;
  transition: all 0.3s ease;
}

.upload-demo:hover .el-icon {
  color: #6366f1;
  transform: scale(1.1);
}

.upload-demo .el-upload__text {
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
  font-size: 1rem;
}

.upload-demo .el-upload__text em {
  color: #6366f1;
  font-style: normal;
  font-weight: 700;
}

.upload-demo .el-upload__tip {
  color: #6b7280;
  font-size: 0.875rem;
  margin-top: 0.5rem;
}

/* 导入结果区域 */
.import-result {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 1.25rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

.import-result .result-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid #f3f4f6;
}

.import-result .result-title {
  font-weight: 600;
  color: #1f2937;
  font-size: 1rem;
}

.import-result .result-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.import-result .stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 1rem;
  border-radius: 8px;
  background: #f9fafb;
  border: 1px solid #f3f4f6;
}

.import-result .stat-item.success {
  background: #ecfdf5;
  border-color: #d1fae5;
}

.import-result .stat-item.error {
  background: #fef2f2;
  border-color: #fee2e2;
}

.import-result .stat-label {
  font-size: 0.875rem;
  color: #6b7280;
  margin-bottom: 0.25rem;
}

.import-result .stat-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
}

.import-result .stat-item.success .stat-value {
  color: #059669;
}

.import-result .stat-item.error .stat-value {
  color: #dc2626;
}

/* 失败详情区域 */
.failed-details {
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  padding: 1rem;
}

.failed-details .failed-header {
  font-weight: 600;
  color: #dc2626;
  margin-bottom: 0.75rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.failed-details .failed-header::before {
  content: '❌';
  font-size: 1rem;
}

.failed-details .failed-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.failed-details .failed-item {
  background: white;
  border: 1px solid #f3f4f6;
  border-radius: 6px;
  padding: 0.75rem;
}

.failed-details .failed-row {
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
}

.failed-details .failed-errors {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

/* 角色分配对话框样式优化 */
.role-dialog-content {
  padding: 1.5rem;
}

.user-info-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  margin-bottom: 1.5rem;
}

.user-details {
  flex: 1;
}

.user-name-large {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 0.25rem;
}

.user-email-large {
  font-size: 0.875rem;
  color: #6b7280;
}

.role-form {
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
}

.role-checkbox-group {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 0.75rem;
}

.role-checkbox {
  margin: 0;
}

.role-checkbox-content {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  border-radius: 6px;
  transition: all 0.2s ease;
}

.role-checkbox:hover .role-checkbox-content {
  background: #f8fafc;
}

/* ===== 响应式设计 ===== */
@media (max-width: 768px) {
  .import-dialog-content {
    gap: 1rem;
    padding: 1rem;
  }

  .import-tips,
  .batch-settings {
    padding: 1rem;
  }

  .import-tips .tips-list li {
    font-size: 0.8125rem;
  }

  .upload-demo {
    padding: 1.5rem;
  }

  .upload-demo .el-icon {
    font-size: 2.5rem;
  }

  .import-result .result-stats {
    grid-template-columns: 1fr;
    gap: 0.75rem;
  }

  .import-settings-form {
    padding: 0.75rem;
  }

  .role-checkbox-group {
    grid-template-columns: 1fr;
  }

  .user-info-card {
    flex-direction: column;
    text-align: center;
    gap: 0.75rem;
  }
}

@media (max-width: 480px) {
  .management-dialog {
    width: 95% !important;
    margin: 2rem auto !important;
  }

  .import-dialog-content {
    max-height: 60vh;
  }

  .batch-settings .el-col {
    margin-bottom: 0.75rem;
  }

  .batch-settings .el-col:last-child {
    margin-bottom: 0;
  }
}

/* ===== 动画效果 ===== */
.fade-in {
  animation: fadeInUp 0.4s ease-out;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.slide-in-left {
  animation: slideInLeft 0.3s ease-out;
}

@keyframes slideInLeft {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* 应用动画类 */
.import-tips,
.batch-settings,
.import-result {
  animation: fadeInUp 0.4s ease-out;
}

</style> 