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
          v-permission="['UserCreate']"
          type="primary" 
          size="large"
          class="create-btn"
          @click="showCreateDialog"
        >
          <el-icon><Plus /></el-icon>
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
          <el-form :inline="true" :model="searchForm" class="search-form">
            <el-form-item label="搜索">
              <el-input
                v-model="searchForm.keyword"
                placeholder="请输入用户名"
                clearable
                class="search-input"
                :prefix-icon="Search"
                @keyup.enter="handleSearch"
              />
            </el-form-item>
            <el-form-item label="状态">
              <el-select 
                v-model="searchForm.status" 
                placeholder="请选择状态" 
                clearable 
                class="status-select"
              >
                <el-option label="启用" value="1" />
                <el-option label="禁用" value="0" />
              </el-select>
            </el-form-item>
            <el-form-item label="组织架构">
              <el-tree-select
                v-model="searchForm.organizationUnitId"
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
                <el-button type="primary" class="search-btn" @click="handleSearch">
                  <el-icon><Search /></el-icon>
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
                >
                  <el-icon><Download /></el-icon>
                  下载模板
                </el-button>
                <el-button 
                  v-permission="['UserCreate']"
                  type="warning" 
                  class="import-btn" 
                  @click="showImportDialog"
                >
                  <el-icon><Upload /></el-icon>
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
            >
              <el-icon><Delete /></el-icon>
              批量删除 ({{ selectedUsers.length }})
            </el-button>
            <el-button 
              v-if="selectedUsers.length > 0"
              v-permission="['UserResetPassword']"
              type="primary" 
              size="small"
              class="batch-reset-password-btn"
              @click="handleBatchResetPassword"
            >
              <el-icon><Refresh /></el-icon>
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
              @update:page-size="handleSizeChange"
              @update:current-page="handleCurrentChange"
            />
          </div>
        </div>
      </div>
    </div>
    
    <!-- 创建/编辑用户对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="600px"
      class="management-dialog"
      :close-on-click-modal="false"
      @close="handleDialogClose"
    >
      <el-form
        ref="userFormRef"
        :model="userForm"
        :rules="userRules"
        label-width="100px"
        class="management-form"
      >
      
 
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="用户名" prop="name">
              <el-input 
                v-model="userForm.name" 
                placeholder="请输入用户名"
                :prefix-icon="User"
                clearable
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="邮箱" prop="email">
              <el-input 
                v-model="userForm.email" 
                placeholder="请输入邮箱"
                :prefix-icon="Message"
                clearable
              />
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="真实姓名" prop="realName">
              <el-input 
                v-model="userForm.realName" 
                placeholder="请输入真实姓名"
                :prefix-icon="UserFilled"
                clearable
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="手机号" prop="phone">
              <el-input 
                v-model="userForm.phone" 
                placeholder="请输入手机号"
                :prefix-icon="Phone"
                clearable
              />
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="性别" prop="gender">
              <el-select v-model="userForm.gender" placeholder="请选择性别" clearable>
                <el-option label="男" value="男" />
                <el-option label="女" value="女" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="出生日期" prop="birthDate">
              <el-date-picker 
                v-model="userForm.birthDate" 
                type="date" 
                placeholder="请选择出生日期"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-form-item label="密码" prop="password" v-if="!isEdit">
          <el-input
            v-model="userForm.password"
            type="password"
            placeholder="请输入密码"
            :prefix-icon="Lock"
            show-password
            clearable
          />
        </el-form-item>
        <el-form-item label="确认密码" prop="confirmPassword" v-if="!isEdit">
          <el-input
            v-model="userForm.confirmPassword"  
            type="password"
            placeholder="请输入确认密码"
            :prefix-icon="Lock"
            show-password
            clearable
          />
        </el-form-item>
        
        <el-form-item label="状态" prop="status">
          <el-radio-group v-model="userForm.status" class="status-radio-group">
            <el-radio :label="1" class="status-radio">
              <div class="radio-content">
                <el-icon size="16" color="#10b981"><CircleCheck /></el-icon>
                <span>启用</span>
              </div>
            </el-radio>
            <el-radio :label="0" class="status-radio">
              <div class="radio-content">
                <el-icon size="16" color="#ef4444"><CircleClose /></el-icon>
                <span>禁用</span>
              </div>
            </el-radio>
          </el-radio-group>
        </el-form-item>
        
        <el-form-item label="组织架构" prop="organizationUnitId">
          <el-tree-select
            v-model="userForm.organizationUnitId"
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
            @change="handleOrganizationUnitChange"
            style="width: 100%"
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false" size="large">取消</el-button>
          <el-button type="primary" :loading="submitLoading" @click="handleSubmit" size="large" class="submit-btn">
            <el-icon v-if="!submitLoading"><Check /></el-icon>
            {{ isEdit ? '更新' : '创建' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
    
    <!-- 分配角色对话框 -->
    <el-dialog
      v-model="roleDialogVisible"
      title="分配角色"
      width="500px"
      class="management-dialog"
      :close-on-click-modal="false"
    >
      <div class="role-dialog-content">
        <div class="user-info-card">
          <el-avatar :size="48" class="user-avatar-large">
            <el-icon><UserFilled /></el-icon>
          </el-avatar>
          <div class="user-details">
            <div class="user-name-large">{{ currentUser?.name }}</div>
            <div class="user-email-large">{{ currentUser?.email }}</div>
          </div>
        </div>
        
        <el-form label-width="80px" class="role-form">
          <el-form-item label="角色">
            <el-checkbox-group v-model="selectedRoleIds" class="role-checkbox-group">
              <el-checkbox
                v-for="role in allRoles"
                :key="role.roleId"
                :value="role.roleId"
                class="role-checkbox"
              >
                <div class="role-checkbox-content">
                  <el-icon size="16" color="#6366f1"><Setting /></el-icon>
                  <span>{{ role.name }}</span>
                </div>
              </el-checkbox>
            </el-checkbox-group>
          </el-form-item>
        </el-form>
      </div>
      
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="roleDialogVisible = false" size="large">取消</el-button>
          <el-button type="primary" :loading="roleSubmitLoading" @click="handleRoleSubmit" size="large" class="submit-btn">
            <el-icon v-if="!roleSubmitLoading"><Check /></el-icon>
            确定
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- 批量导入对话框 -->
    <el-dialog
      v-model="importDialogVisible"
      title="批量导入用户"
      width="600px"
      class="management-dialog"
      :close-on-click-modal="false"
      @close="handleImportDialogClose"
    >
      <div class="import-dialog-content"> 
        <!-- <div class="import-tips">
          <div class="tips-header">
            <el-icon size="18" color="#f59e0b"><Warning /></el-icon>
            <span class="tips-title">导入说明</span>
          </div>
          <ul class="tips-list">
            <li>请先下载导入模板，按照模板格式填写用户信息</li>
            <li>支持Excel文件格式（.xlsx, .xls）</li>
            <li>单次最多可导入1000条用户记录</li>
            <li>用户名和邮箱不能重复，重复的记录将被跳过</li>
            <li>所有导入的用户将统一分配选择的组织架构和角色</li>
          </ul>
        </div> -->

        <!-- 批量设置区域 -->
        <div class="batch-settings" v-if="!importResult">
          <div class="settings-header">
            <el-icon size="18" color="#10b981"><Setting /></el-icon>
            <span class="settings-title">批量设置</span>
          </div>
          <el-form ref="importFormRef" :model="importForm" :rules="importRules" label-width="100px" class="import-settings-form">
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="组织架构" prop="organizationUnitId">
                  <el-tree-select
                    v-model="importForm.organizationUnitId"
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
                    @change="handleImportFormOrganizationUnitChange"
                    class="full-width"
                  />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="用户角色" prop="roleIds">
                  <el-select 
                    v-model="importForm.roleIds" 
                    placeholder="请选择角色" 
                    multiple
                    clearable
                    class="full-width"
                  >
                    <el-option
                      v-for="role in allRoles"
                      :key="role.roleId"
                      :label="role.name"
                      :value="role.roleId"
                    />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
        </div>
        
        <el-upload v-if="!importResult"
          ref="uploadRef"
          class="upload-demo"
          drag
          :auto-upload="false"
          :multiple="false"
          accept=".xlsx,.xls"
          :on-change="handleFileChange"
          :file-list="fileList"
          :limit="1"
        >
          <el-icon class="el-icon--upload"><UploadFilled /></el-icon>
          <div class="el-upload__text">
            将Excel文件拖到此处，或<em>点击上传</em>
          </div>
          <template #tip>
            <div class="el-upload__tip">
              只能上传xlsx/xls文件，且不超过10MB
            </div>
          </template>
        </el-upload>
        
        <!-- 导入结果 -->
        <div v-if="importResult" class="import-result">
          <div class="result-header">
            <el-icon size="18" :color="importResult.failCount > 0 ? '#f59e0b' : '#10b981'">
              <component :is="importResult.failCount > 0 ? 'Warning' : 'CircleCheck'" />
            </el-icon>
            <span class="result-title">导入结果</span>
          </div>
          <div class="result-stats">
            <div class="stat-item">
              <span class="stat-label">总计：</span>
              <span class="stat-value">{{ importResult.totalCount }}</span>
            </div>
            <div class="stat-item success">
              <span class="stat-label">成功：</span>
              <span class="stat-value">{{ importResult.successCount }}</span>
            </div>
            <div class="stat-item error" v-if="importResult.failCount > 0">
              <span class="stat-label">失败：</span>
              <span class="stat-value">{{ importResult.failCount }}</span>
            </div>
          </div>
          
          <!-- 失败详情 -->
          <div v-if="importResult.failedRows && importResult.failedRows.length > 0" class="failed-details">
            <div class="failed-header">失败详情：</div>
            <div class="failed-list">
              <div 
                v-for="(failedRow, index) in importResult.failedRows" 
                :key="index"
                class="failed-item"
              >
                <div class="failed-row">第{{ failedRow.row }}行：</div>
                <div class="failed-errors">
                  <el-tag 
                    v-for="(error, errorIndex) in failedRow.errors" 
                    :key="errorIndex"
                    type="danger" 
                    size="small"
                  >
                    {{ error }}
                  </el-tag>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="importDialogVisible = false" size="large">取消</el-button>
          <el-button 
            type="primary" 
            :loading="importLoading" 
            :disabled="!selectedFile"
            @click="handleImportSubmit" 
            size="large"
            class="submit-btn"
          >
            <el-icon v-if="!importLoading"><Upload /></el-icon>
            开始导入
          </el-button>
        </div>
      </template>
    </el-dialog>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed, watch } from 'vue'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules, type UploadFile, type UploadFiles } from 'element-plus'
import {
  Plus, 
  Search, 
  Refresh, 
  Download, 
  Upload, 
  Edit, 
  Delete, 
  Check, 
  User, 
  UserFilled, 
  Message, 
  Phone, 
  Lock, 
  Setting, 
  OfficeBuilding, 
  Clock, 
  DocumentRemove,
  Warning,
  UploadFilled,
  CircleCheck,
  CircleClose
} from '@element-plus/icons-vue'
import { register, resetPassword, updateUser, updateUserRoles, getUsers, deleteUser, downloadUserTemplate, batchImportUsers, type RegisterRequest, type UserInfo, type BatchImportUsersResponse } from '@/api/user'
import { getAllRoles, type RoleInfo } from '@/api/role'
import { organizationApi, type OrganizationUnit, type OrganizationUnitTree } from '@/api/organization'
import { hasPermission } from '@/utils/permission'

const loading = ref(false)
const submitLoading = ref(false)
const roleSubmitLoading = ref(false)
const importLoading = ref(false)
const dialogVisible = ref(false)
const roleDialogVisible = ref(false)
const importDialogVisible = ref(false)
const isEdit = ref(false)
const currentUserId = ref<string>('')
const currentUser = ref<any>(null)

// 批量导入相关
const selectedFile = ref<File | null>(null)
const fileList = ref<UploadFiles>([])
const importResult = ref<BatchImportUsersResponse | null>(null)
const uploadRef = ref()

// 导入表单数据
const importForm = reactive({
  organizationUnitId: null as number | null,
  organizationUnitName: '',
  roleIds: [] as string[]
})

// 批量导入表单校验
const importFormRef = ref<FormInstance>()
const importRules: FormRules = {
  organizationUnitId: [
    { required: true, message: '请选择组织架构', trigger: 'change' }
  ],
  roleIds: [
    { type: 'array', required: true, message: '请选择至少一个角色', trigger: 'change' }
  ]
}

const users = ref<UserInfo[]>([])
const selectedUsers = ref<UserInfo[]>([])
const allRoles = ref<RoleInfo[]>([])
const selectedRoleIds = ref<string[]>([])
const organizationOptions = ref<OrganizationUnit[]>([])
const organizationTreeOptions = ref<OrganizationUnitTree[]>([])

const searchForm = reactive({
  keyword: '',
  status: null as number | null,
  organizationUnitId: null as number | null
})

const pagination = reactive({
  pageIndex: 1,
  pageSize: 10,
  total: 0
})

const userForm = reactive<RegisterRequest>({
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
  phone: '',
  realName: '',
  status: 1,
  roleIds: [],
  gender: '',
  age: 0,
  organizationUnitId: 0,
  organizationUnitName: '',
  birthDate: '',
  userId: ''
})

const userFormRef = ref<FormInstance>()

// 密码确认验证函数
const validateConfirmPassword = (_: any, value: string, callback: any) => {
  if (value === '') {
    callback(new Error('请再次输入密码'))
  } else if (value !== userForm.password) {
    callback(new Error('两次输入密码不一致'))
  } else {
    callback()
  }
}

const userRules: FormRules = {
  
  name: [
    { required: true, message: '请输入用户名', trigger: 'onBlur' }
  ],
 
  // email: [
  //   { required: true, message: '请输入邮箱', trigger: 'onBlur' },
  //   { type: 'email', message: '请输入正确的邮箱格式', trigger: 'onBlur' }
  // ],
  password: [
    { required: true, message: '请输入密码', trigger: 'onBlur' },
    { min: 6, message: '密码长度不能少于6位', trigger: 'onBlur' }
  ],
  confirmPassword: [
    { required: true, validator: validateConfirmPassword, trigger: 'onBlur' }
  ],
  realName: [
    { required: true, message: '请输入真实姓名', trigger: 'onBlur' }
  ],
  // phone: [
  //   { required: true, message: '请输入手机号', trigger: 'onBlur' }
  // ],
  gender: [
    { required: true, message: '请选择性别', trigger: 'change' }
  ],
  organizationUnitId: [
    { required: true, message: '请选择组织架构', trigger: 'change' }
  ],
  birthDate: [
    { required: true, message: '请选择出生日期', trigger: 'change' }
  ]
  // 年龄现在根据出生日期自动计算，不需要验证规则
}

const dialogTitle = computed(() => isEdit.value ? '编辑用户' : '新建用户')

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

// 计算年龄的函数
const calculateAge = (birthDate: string | Date): number => {
  if (!birthDate) return 0
  
  const birth = new Date(birthDate)
  const today = new Date()
  
  let age = today.getFullYear() - birth.getFullYear()
  const monthDiff = today.getMonth() - birth.getMonth()
  
  // 如果还没过生日，年龄减1
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
    age--
  }
  
  return age > 0 ? age : 0
}

// 监听出生日期变化，自动计算年龄
watch(() => userForm.birthDate, (newBirthDate) => {
  if (newBirthDate) {
    userForm.age = calculateAge(newBirthDate)
  } else {
    userForm.age = 0
  }
})

const loadOrganizationUnits = async () => {
  // 检查是否有权限
  if (!hasPermission(['OrganizationUnitView'])) {
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
    // 错误已在全局拦截器中处理
  }
}

const loadUsers = async () => {
  loading.value = true
  try {
    const response = await getUsers({
      pageIndex: pagination.pageIndex,
      pageSize: pagination.pageSize,
      keyword: searchForm.keyword || undefined,
      status: searchForm.status || undefined,
      organizationUnitId: searchForm.organizationUnitId || undefined,
      countTotal:true
    })
    users.value = response.data.items
    pagination.total = response.data.total
  } catch (error: any) {
    // 错误已在全局拦截器中处理
  } finally {
    loading.value = false
  }
}

const loadRoles = async () => {
  
  // 检查是否有权限
  if (!hasPermission(['RoleView'])) {
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
    // 错误已在全局拦截器中处理
  }
}

const handleSearch = () => {
  pagination.pageIndex = 1
  loadUsers()
}

const handleReset = () => {
  searchForm.keyword = ''
  searchForm.status = null
  searchForm.organizationUnitId = null
  pagination.pageIndex = 1
  loadUsers()
}

const handleSizeChange = (size: number) => {
  pagination.pageSize = size
  pagination.pageIndex = 1
  loadUsers()
}

const handleCurrentChange = (page: number) => {
  pagination.pageIndex = page
  loadUsers()
}

const handleSelectionChange = (selection: UserInfo[]) => {
  selectedUsers.value = selection
}

const showCreateDialog = () => {
  isEdit.value = false
  currentUserId.value = ''
  Object.assign(userForm, {
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    phone: '',
    realName: '',
    status: 1,
    roleIds: [],
    gender: '',
    age: 0,
    organizationUnitId: undefined,
    organizationUnitName: '',
    birthDate: '',
  })
  dialogVisible.value = true
}

const handleEdit = (user: UserInfo) => {
  isEdit.value = true
  currentUserId.value = user.userId
  Object.assign(userForm, {
    name: user.name,
    email: user.email,
    password: '',
    confirmPassword: '',
    phone: user.phone,
    realName: user.realName,
    status: user.status,
    roleIds: [],
    gender: user.gender,
    age: user.age,
    organizationUnitId: user.organizationUnitId,
    organizationUnitName: user.organizationUnitName,
    birthDate: user.birthDate,
  })
  
  // 如果有出生日期，重新计算年龄
  if (user.birthDate) {
    userForm.age = calculateAge(user.birthDate)
  }
  
  // 如果用户有组织架构ID但没有名称，自动设置名称
  if (user.organizationUnitId && !user.organizationUnitName) {
    const selectedOrg = findOrganizationById(organizationTreeOptions.value, user.organizationUnitId);
    if (selectedOrg) {
      userForm.organizationUnitName = selectedOrg.name;
    }
  }
  
  dialogVisible.value = true
}

const handleResetPassword = async (user: UserInfo) => {
  await ElMessageBox.confirm(`确定要重置用户"${user.name}"的密码吗？`, '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  })
  await resetPassword(user.userId)
  ElMessage.success('重置成功')
}

const handleRoles = (user: UserInfo) => {
  currentUser.value = user
  selectedRoleIds.value = user.roles.map((role: string) => {
    const foundRole = allRoles.value.find(r => r.name === role)
    return foundRole?.roleId || ''
  }).filter(Boolean)
  roleDialogVisible.value = true
}



const handleDelete = async (user: UserInfo) => {
  try {
    await ElMessageBox.confirm(`确定要删除用户"${user.name}"吗？`, '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await deleteUser(user.userId)
    ElMessage.success('删除成功')
    loadUsers()
  } catch (error: any) {
    if (error !== 'cancel') {
      // 错误已在全局拦截器中处理
    }
  }
}

const handleSubmit = async () => {
  if (!userFormRef.value) return
  
  try {
    await userFormRef.value.validate()
    submitLoading.value = true
    
    if (isEdit.value) {
      await updateUser({
        userId: currentUserId.value,
        name: userForm.name,
        email: userForm.email,
        phone: userForm.phone,
        realName: userForm.realName,
        status: userForm.status,
        gender: userForm.gender,
        age: userForm.age,
        organizationUnitId: userForm.organizationUnitId,
        organizationUnitName: userForm.organizationUnitName,
        birthDate: userForm.birthDate,
        password:''
      })
      ElMessage.success('更新成功')
    } else {
      // 创建用户时，确保密码和确认密码一致
      if (userForm.password !== userForm.confirmPassword) {
        ElMessage.error('两次输入的密码不一致')
        return
      }
      await register(userForm)
      ElMessage.success('创建成功')
    }
    
    dialogVisible.value = false
    loadUsers()
  } catch (error: any) {
    // 错误已在全局拦截器中处理
  } finally {
    submitLoading.value = false
  }
}

const handleRoleSubmit = async () => {
  if (!currentUser.value) return
  
  try {
    roleSubmitLoading.value = true
    await updateUserRoles({
      roleIds: selectedRoleIds.value,
      userId: currentUser.value.userId
    })
    ElMessage.success('角色分配成功')
    roleDialogVisible.value = false
    loadUsers()
  } catch (error: any) {
    // 错误已在全局拦截器中处理
  } finally {
    roleSubmitLoading.value = false
  }
}

const handleOrganizationUnitChange = (value: number | undefined) => {
  if (value) {
    const selectedOrg = findOrganizationById(organizationTreeOptions.value, value);
    if (selectedOrg) {
      userForm.organizationUnitName = selectedOrg.name;
    } else {
      userForm.organizationUnitName = '';
    }
  } else {
    userForm.organizationUnitName = '';
  }
};


const handleImportFormOrganizationUnitChange = (value: number | undefined) => {
  if (value) {
    const selectedOrg = findOrganizationById(organizationTreeOptions.value, value);
    if (selectedOrg) {
      importForm.organizationUnitName = selectedOrg.name;
    } else {
      importForm.organizationUnitName = '';
    }
  } else {
    importForm.organizationUnitName = '';
  }
};


const handleDialogClose = () => {
  userFormRef.value?.resetFields()
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString('zh-CN')
}

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
    for (const user of selectedUsers.value) {
       await deleteUser(user.userId)
    }
    ElMessage.success('删除成功')
    loadUsers()
  } catch (error: any) {
   // ElMessage.error('删除失败')
    loadUsers()
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
    loadUsers()
  } catch (error: any) {
    loadUsers()
  }
}

// 显示导入对话框
const showImportDialog = () => {
  importDialogVisible.value = true
  importResult.value = null
  selectedFile.value = null
  fileList.value = []
  // 重置导入表单
  importFormRef.value?.resetFields()
  importForm.organizationUnitId = null
  importForm.organizationUnitName = ''
  importForm.roleIds = []
}

// 文件选择变化
const handleFileChange = (file: UploadFile, uploadFiles: UploadFiles) => {
  // 文件大小检查（10MB）
  const maxSize = 10 * 1024 * 1024
  if (file.raw && file.raw.size > maxSize) {
    ElMessage.error('文件大小不能超过 10MB')
    uploadRef.value?.clearFiles()
    return
  }
  
  // 文件格式检查
  const allowedTypes = [
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'application/vnd.ms-excel'
  ]
  if (file.raw && !allowedTypes.includes(file.raw.type)) {
    ElMessage.error('请选择Excel文件（.xlsx 或 .xls）')
    uploadRef.value?.clearFiles()
    return
  }
  
  selectedFile.value = file.raw || null
  fileList.value = uploadFiles
}

// 导入提交
const handleImportSubmit = async () => {
  if (!selectedFile.value) {
    ElMessage.error('请选择要导入的文件')
    return
  }
  
  try {
    // 验证必填项
    if (importFormRef.value) {
      await importFormRef.value.validate()
    }
    importLoading.value = true
    const response = await batchImportUsers({
      file: selectedFile.value,
      organizationUnitId: importForm.organizationUnitId || undefined,
      organizationUnitName: importForm.organizationUnitName || undefined,
      roleIds: importForm.roleIds.length > 0 ? importForm.roleIds : undefined
    })
    importResult.value = response.data
    
    if (response.data.failCount === 0) {
      ElMessage.success(`导入成功！共导入 ${response.data.successCount} 个用户`)
      // 刷新用户列表
      loadUsers()
    } else {
      ElMessage.warning(
        `导入完成！成功 ${response.data.successCount} 个，失败 ${response.data.failCount} 个`
      )
    }
  } catch (error: any) {
    ElMessage.error('导入失败，请检查文件格式和内容')
  } finally {
    importLoading.value = false
  }
}

// 导入对话框关闭
const handleImportDialogClose = () => {
  importResult.value = null
  selectedFile.value = null
  fileList.value = []
  uploadRef.value?.clearFiles()
  importFormRef.value?.resetFields()
  // 重置导入表单
  importForm.organizationUnitId = null
  importForm.organizationUnitName = ''
  importForm.roleIds = []
}

onMounted(() => {
  loadUsers()
  loadRoles()
  loadOrganizationUnits()
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