<template>
  <div class="management-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1>角色管理</h1>
          <p class="subtitle">管理系统中的所有角色和权限分配</p>
        </div>
        <el-button 
          type="primary" 
          size="large"
          class="create-btn" 
          @click="showCreateDialog"
          icon="Plus"
        >
          新建角色
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
                v-model="searchParams.name"
                placeholder="请输入角色名称"
                clearable
                class="search-input"
                :prefix-icon="Search"
                @keyup.enter="() => handleSearch()"
              />
            </el-form-item>
            <el-form-item>
              <div class="action-buttons">
                <el-button type="primary" class="search-btn" @click="() => handleSearch()"  icon="Search">
                  搜索 
                </el-button>
                <el-button class="reset-btn" @click="handleReset"  icon="Refresh">
                  重置
                </el-button>
              </div>
            </el-form-item>
          </el-form>
        </div>
      </div>
      
      <!-- 角色列表 -->
      <div class="table-section">
        <div class="section-header">
          <div class="header-left">
            <h2>
              <el-icon class="section-icon"><Setting /></el-icon>
              角色列表
            </h2>
            <el-tag type="info" class="count-tag">{{ pagination.total }} 个角色</el-tag>
          </div>
          <div class="header-right">
            <el-button 
              v-if="selectedRoles.length > 0"
              type="danger" 
              size="small"
              class="batch-delete-btn"
              icon="Delete"
            >
              批量删除 ({{ selectedRoles.length }})
            </el-button>
          </div>
        </div>
        
        <div class="table-wrapper">
          <el-table
            v-loading="loading"
            :data="roles"
            class="data-table"
            @selection-change="handleSelectionChange"
            :header-cell-style="{ backgroundColor: '#f8fafc', color: '#374151', fontWeight: '600' }"
            stripe
          >
            <el-table-column type="selection" width="55" />
            <el-table-column prop="name" label="角色名称" min-width="150">
              <template #default="{ row }">
                <div class="entity-cell">
                  <el-avatar :size="32" class="entity-avatar" icon="Setting">
                  </el-avatar>
                  <div class="entity-info">
                    <div class="entity-name">{{ row.name }}</div>
                    <div class="entity-description">{{ row.description }}</div>
                  </div>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="permissionCount" label="权限数量" width="120" align="center">
              <template #default="{ row }">
                <el-tag type="info" class="permission-count-tag" size="small">
                  <el-icon size="12"><Key /></el-icon>
                  {{ row.permissionCodes?.length || 0 }}
                </el-tag>
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
            <el-table-column label="操作" width="120" fixed="right" align="center">
              <template #default="{ row }">
                <div class="table-actions">
                  <el-button 
                    size="small" 
                    type="primary" 
                    :icon="Edit"
                    circle
                    class="table-action-btn" 
                    @click="handleEdit(row)"
                  />
                  <el-button 
                    size="small" 
                    type="danger" 
                    :icon="Delete"
                    circle
                    class="table-action-btn" 
                    @click="handleDelete(row)"
                  />
                </div>
              </template>
            </el-table-column>
          </el-table>
          
          <div v-if="roles.length === 0" class="empty-state">
            <el-icon class="empty-icon"><DocumentRemove /></el-icon>
            <p>暂无角色数据</p>
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
    
    <!-- 创建/编辑角色对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="800px"
      class="management-dialog"
      :close-on-click-modal="false"
      @close="handleDialogClose"
    >
      <el-form
        ref="roleFormRef"
        :model="roleForm"
        :rules="roleRules"
        label-width="100px"
        class="management-form"
      >
        <el-row :gutter="20">
          <el-col :span="24">
            <el-form-item label="角色名称" prop="name">
              <el-input 
                v-model="roleForm.name" 
                placeholder="请输入角色名称"
                :prefix-icon="Setting"
                clearable
              />
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-form-item label="角色描述" prop="description">
          <el-input
            v-model="roleForm.description"
            type="textarea"
            :rows="3"
            placeholder="请输入角色描述信息"
          />
        </el-form-item>
        
        <el-form-item label="权限" prop="permissionCodes">
          <div class="tree-wrapper">
            <!-- 全选/取消全选按钮 - 固定在顶部 -->
            <div class="tree-header">
              <div class="header-left">
                <el-icon class="header-icon"><Key /></el-icon>
                <span class="header-title">权限配置</span>
                <el-tag size="small" type="info">
                  已选择 {{ roleForm.permissionCodes.length }} 项
                </el-tag>
              </div>
              <div class="header-right">
                <el-button size="small" class="select-btn" @click="handleSelectAll">
                  <el-icon size="12"><Select /></el-icon>
                  全选
                </el-button>
                <el-button size="small" class="unselect-btn" @click="handleUnselectAll">
                  <el-icon size="12"><Close /></el-icon>
                  取消全选
                </el-button>
              </div>
            </div>
            
            <!-- 权限树容器 - 可滚动 -->
            <div class="tree-container">
              <el-tree
                ref="permissionTreeRef"
                :data="permissionTreeData"
                :props="treeProps"
                :default-checked-keys="roleForm.permissionCodes"
                show-checkbox
                node-key="code"
                class="tree"
                @check="handlePermissionCheck"
              >
                <template #default="{ data }">
                  <div class="tree-node">
                    <div class="tree-content">
                      <el-icon class="tree-icon">
                        <component :is="data.children ? 'FolderOpened' : 'Key'" />
                      </el-icon>
                      <span class="tree-name">{{ data.displayName }}</span>
                    </div>
                    <el-tag v-if="!data.isEnabled" size="small" type="danger" class="disabled-tag">
                      已禁用
                    </el-tag>
                  </div>
                </template>
              </el-tree>
            </div>
          </div>
        </el-form-item>
      </el-form>
      
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false" size="large" icon="Close" >取消</el-button>
          <el-button type="primary" :loading="submitLoading" @click="handleSubmit" size="large" class="submit-btn" icon="Check">
            {{ isEdit ? '更新' : '创建' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed, nextTick } from 'vue'
import { ElMessage, type FormInstance } from 'element-plus'
import { 
  Search, 
  Delete, 
  Edit, 
  Setting, 
  Clock, 
  Key, 
  Select, 
  Close,
  DocumentRemove
} from '@element-plus/icons-vue'

import { getAllRoles, createRole, updateRole, deleteRole, type RoleQueryRequest } from '@/api/role'
import { getPermissionTree } from '@/api/permission'
import type { Role, Permission, PermissionGroup } from '@/types'
import { useTable, useConfirm, useLoading } from '@/composables'
import { DateFormatter } from '@/utils/format'
import { ErrorHandler } from '@/utils/error'

// 对话框状态
const { loading: submitLoading, withLoading } = useLoading()
const dialogVisible = ref(false)
const isEdit = ref(false)
const currentRoleId = ref('')

// 权限数据
const permissionTreeData = ref<Permission[]>([])
const permissionTreeRef = ref()

// 使用表格Composable
const {
  data: roles,
  selectedRows: selectedRoles,
  loading,
  pagination,
  searchParams,
  loadData,
  handleSearch,
  handleSelectionChange,
  refresh,
  handlePageSizeChange,
  handlePageCurrentChange
} = useTable<Role>(async (params: RoleQueryRequest) => {
  const response = await getAllRoles(params)
  return response.data
})

// 扩展搜索参数类型
Object.assign(searchParams, {
  name: ''
})

// 确认对话框
const { confirmDelete } = useConfirm()

// 角色表单
const roleForm = reactive({
  name: '',
  description: '',
  permissionCodes: [] as string[]
})

const roleFormRef = ref<FormInstance>()

// 验证规则
const roleRules = computed(() => ({
  name: [
    { required: true, message: '请输入角色名称', trigger: 'blur' },
    { min: 2, max: 50, message: '角色名称长度应为 2-50 个字符', trigger: 'blur' }
  ],
  description: [
    { required: true, message: '请输入角色描述', trigger: 'blur' },
    { max: 200, message: '描述长度不能超过 200 个字符', trigger: 'blur' }
  ]
}))

const treeProps = {
  children: 'children',
  label: 'displayName'
}

const dialogTitle = computed(() => isEdit.value ? '编辑角色' : '新建角色')

// 加载权限树数据
const loadPermissionTree = async () => {
  try {
    const response = await getPermissionTree()
    // 将权限组转换为树形数据，保持组结构
    permissionTreeData.value = response.data.map((group: PermissionGroup) => ({
      code: group.name,
      displayName: group.name,
      isEnabled: true,
      children: group.permissions
    }))
  } catch (error) {
    ErrorHandler.handle(error, 'loadPermissionTree')
  }
}

// loadRoles已被useTable Composable替代

// 重置搜索
const handleReset = () => {
  Object.keys(searchParams).forEach(key => {
    searchParams[key] = ''
  })
  handleSearch()
}

const handlePermissionCheck = (_: Permission, checkedInfo: any) => {
  // 获取所有选中的权限码
  const checkedKeys = checkedInfo.checkedKeys || []
  
  // 只使用完全选中的权限码，不包含半选中的父节点
  roleForm.permissionCodes = checkedKeys
}

const showCreateDialog = () => {
  isEdit.value = false
  currentRoleId.value = ''
  roleForm.name = ''
  roleForm.description = ''
  roleForm.permissionCodes = []
  dialogVisible.value = true
  
  // 等待DOM更新后清空树的选中状态
  nextTick(() => {
    if (permissionTreeRef.value) {
      permissionTreeRef.value.setCheckedKeys([])
    }
  })
}

const handleEdit = (role: Role) => {
  isEdit.value = true
  currentRoleId.value = role.roleId
  roleForm.name = role.name
  roleForm.description = role.description
  roleForm.permissionCodes = role.permissionCodes || []
  dialogVisible.value = true
  
  // 等待DOM更新后设置树的选中状态
  nextTick(() => {
    if (permissionTreeRef.value) {
      permissionTreeRef.value.setCheckedKeys(roleForm.permissionCodes)
    }
  })
}

const handleDelete = async (role: Role) => {
  try {
    const confirmed = await confirmDelete(role.name)
    if (confirmed) {
      await deleteRole(role.roleId)
      ElMessage.success('删除成功')
      refresh()
    }
  } catch (error) {
    ErrorHandler.handle(error, 'deleteRole')
  }
}

const handleSubmit = async () => {
  if (!roleFormRef.value) return
  
  try {
    await roleFormRef.value.validate()
    
    await withLoading(async () => {
      if (isEdit.value) {
        await updateRole({
          roleId: currentRoleId.value,
          ...roleForm
        })
        ElMessage.success('更新成功')
      } else {
        await createRole(roleForm)
        ElMessage.success('创建成功')
      }
      
      dialogVisible.value = false
      refresh()
    })
  } catch (error) {
    ErrorHandler.handle(error, 'roleForm')
  }
}

const handleDialogClose = () => {
  roleFormRef.value?.resetFields()
  // 清空树的选中状态
  if (permissionTreeRef.value) {
    permissionTreeRef.value.setCheckedKeys([])
  }
}

const formatDate = (dateString: string) => DateFormatter.toDateTimeString(dateString)

// 递归获取所有权限码
const getAllPermissionCodes = (items: Permission[]): string[] => {
  const codes: string[] = []
  items.forEach(item => {
    if (item.children && item.children.length > 0) {
      codes.push(...getAllPermissionCodes(item.children))
    } else {
      codes.push(item.code)
    }
  })
  return codes
}

const handleSelectAll = () => {
  const allCodes = getAllPermissionCodes(permissionTreeData.value)
  permissionTreeRef.value?.setCheckedKeys(allCodes)
  roleForm.permissionCodes = allCodes
}

const handleUnselectAll = () => {
  permissionTreeRef.value?.setCheckedKeys([])
  roleForm.permissionCodes = []
}

onMounted(async () => {
  await Promise.all([
    loadData(), // 加载角色数据
    loadPermissionTree()
  ])
})
</script>

<style scoped>
/* 角色管理页面特有样式 */

/* 角色页面特有的样式可以在这里添加 */
</style>