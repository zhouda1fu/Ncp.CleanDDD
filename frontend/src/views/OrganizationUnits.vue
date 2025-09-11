<template>
  <div class="management-page">
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1>组织架构管理</h1>
          <p class="subtitle">管理您的组织架构层次</p>
        </div>
        <el-button 
          v-permission="[PERMISSIONS.ORG_CREATE]"
          type="primary" 
          size="large"
          class="create-btn"
          @click="showCreateDialog = true"
          icon="Plus"
        >
          新增组织架构
        </el-button>
      </div>
    </div>

    <div class="main-content">
      <div class="tree-section">
        <div class="section-header">
          <h2>
            <el-icon class="section-icon"><OfficeBuilding /></el-icon>
            组织架构树
          </h2>
        </div>
        <div class="org-tree-wrapper">
          <el-tree
            :data="organizationTree"
            :props="treeProps"
            node-key="id"
            default-expand-all
            class="organization-tree"
            @node-click="handleNodeClick"
          >
            <template #default="{ data }">
              <div class="tree-node">
                <div class="node-content">
                  <el-icon class="node-icon"><FolderOpened /></el-icon>
                  <span class="node-text">{{ data.name }}</span>
                </div>
                <div class="node-actions">
                  <el-button 
                    v-permission="[PERMISSIONS.ORG_EDIT]"
                    size="small" 
                    type="primary" 
                    :icon="Edit"
                    circle
                    class="action-btn"
                    @click.stop="editOrganization(data)"
                  />
                  <el-button 
                    v-permission="[PERMISSIONS.ORG_DELETE]"
                    size="small" 
                    type="danger" 
                    :icon="Delete"
                    circle
                    class="action-btn"
                    @click.stop="deleteOrganization(data)"
                  />
                </div>
              </div>
            </template>
          </el-tree>
          <div v-if="organizationTree.length === 0" class="empty-state">
            <el-icon class="empty-icon"><DocumentRemove /></el-icon>
            <p>暂无组织架构数据</p>
          </div>
        </div>
      </div>

      <div class="table-section">
        <div class="section-header">
          <h2>
            <el-icon class="section-icon"><Grid /></el-icon>
            组织架构列表
          </h2>
        </div>
        <div class="table-wrapper">
          <el-table 
            :data="organizationList" 
            class="organization-table"
            :header-cell-style="{ backgroundColor: '#f8fafc', color: '#374151', fontWeight: '600' }"
            stripe
          >
            <el-table-column prop="name" label="名称" min-width="120">
              <template #default="{ row }">
                <div class="name-cell">
                  <el-icon class="name-icon"><OfficeBuilding /></el-icon>
                  <span>{{ row.name }}</span>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="description" label="描述" min-width="150">
              <template #default="{ row }">
                <span class="description-text">{{ row.description || '暂无描述' }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="sortOrder" label="排序" width="80" align="center">
              <template #default="{ row }">
                <el-tag size="small" type="info">{{ row.sortOrder }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="isActive" label="状态" width="100" align="center">
              <template #default="{ row }">
                <el-tag 
                  :type="row.isActive ? 'success' : 'danger'"
                  size="small"
                  class="status-tag"
                >
                  {{ row.isActive ? '激活' : '停用' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="createdAt" label="创建时间" width="180" align="center">
              <template #default="{ row }">
                <div class="time-cell">
                  <el-icon class="time-icon"><Clock /></el-icon>
                  <span>{{ formatDate(row.createdAt) }}</span>
                </div>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="120" align="center" fixed="right">
              <template #default="{ row }">
                <div class="table-actions">
                  <el-button 
                    v-permission="['OrganizationUnitEdit']"
                    size="small" 
                    type="primary" 
                    :icon="Edit"
                    circle
                    class="table-action-btn"
                    @click="editOrganization(row)"
                  />
                  <el-button 
                    v-permission="['OrganizationUnitDelete']"
                    size="small" 
                    type="danger" 
                    :icon="Delete"
                    circle
                    class="table-action-btn"
                    @click="deleteOrganization(row)"
                  />
                </div>
              </template>
            </el-table-column>
          </el-table>
          <div v-if="organizationList.length === 0" class="empty-state">
            <el-icon class="empty-icon"><DocumentRemove /></el-icon>
            <p>暂无组织架构数据</p>
          </div>
        </div>
      </div>
    </div>

    <!-- 创建/编辑对话框 -->
    <el-dialog
      v-model="showCreateDialog"
      :title="editingOrganization ? '编辑组织架构' : '新增组织架构'"
      width="540px"
      class="organization-dialog"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="100px"
        class="organization-form"
      >
        <el-form-item label="名称" prop="name">
          <el-input 
            v-model="form.name" 
            placeholder="请输入组织架构名称"
            :prefix-icon="OfficeBuilding"
            clearable
          />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input
            v-model="form.description"
            type="textarea"
            placeholder="请输入组织架构描述信息"
            :rows="3"
          />
        </el-form-item>
        <el-form-item label="上级组织" prop="parentId">
          <el-select
            v-model="form.parentId"
            placeholder="请选择上级组织"
            clearable
            filterable
            style="width: 100%"
            :prefix-icon="Rank"
          >
            <el-option
              v-for="item in organizationOptions"
              :key="item.id"
              :label="item.name"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="排序" prop="sortOrder">
          <el-input-number 
            v-model="form.sortOrder" 
            :min="1" 
            :max="999"
            style="width: 100%" 
            controls-position="right"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="resetFormAndClose" size="large" icon="Close">取消</el-button>
          <el-button type="primary" @click="submitForm" size="large" class="submit-btn" icon="Check">
            {{ editingOrganization ? '更新' : '创建' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, type FormInstance } from 'element-plus'
import { 
  Edit, 
  Delete, 
  OfficeBuilding, 
  FolderOpened, 
  Grid, 
  Clock, 
  DocumentRemove,
  Rank
} from '@element-plus/icons-vue'
import { organizationApi, type CreateOrganizationUnitRequest, type UpdateOrganizationUnitRequest } from '@/api/organization'
import type { OrganizationUnit, OrganizationUnitTree } from '@/types'
import { useConfirm, useLoading } from '@/composables'
import { DateFormatter } from '@/utils/format'
import { ErrorHandler } from '@/utils/error'
import { PERMISSIONS } from '@/constants'

// 数据状态
const organizationTree = ref<OrganizationUnitTree[]>([])
const organizationList = ref<OrganizationUnit[]>([])
const showCreateDialog = ref(false)
const editingOrganization = ref<OrganizationUnit | null>(null)
const { withLoading } = useLoading()

// 确认对话框
const { confirmDelete } = useConfirm()

// 表单数据
const form = reactive({
  name: '',
  description: '',
  parentId: undefined as number | undefined,
  sortOrder: 1
})

// 验证规则
const rules = computed(() => ({
  name: [
    { required: true, message: '请输入组织架构名称', trigger: 'blur' },
    { min: 2, max: 50, message: '名称长度应为 2-50 个字符', trigger: 'blur' }
  ],
  description: [
    { max: 200, message: '描述长度不能超过 200 个字符', trigger: 'blur' }
  ],
  sortOrder: [
    { required: true, message: '请输入排序', trigger: 'blur' },
    { type: 'number', min: 1, max: 999, message: '排序值应为 1-999', trigger: 'blur' }
  ]
}))

const treeProps = {
  children: 'children',
  label: 'name'
}

const formRef = ref<FormInstance>()

// 获取组织架构选项（排除当前编辑的组织架构及其子组织）
const organizationOptions = computed(() => {
  if (!editingOrganization.value) {
    return organizationList.value
  }
  return organizationList.value.filter(item => 
    item.id !== editingOrganization.value!.id
  )
})

onMounted(() => {
  loadData()
})

const loadData = async () => {
  try {
    const [treeData, listData] = await Promise.all([
      organizationApi.getTree(),
      organizationApi.getAll()
    ])
    organizationTree.value = treeData.data
    organizationList.value = listData.data
  } catch (error) {
    ErrorHandler.handle(error, 'loadOrganizationData')
  }
}

const handleNodeClick = (data: OrganizationUnitTree) => {
  console.log('点击节点:', data)
}

const editOrganization = (organization: OrganizationUnit) => {
  editingOrganization.value = organization
  form.name = organization.name
  form.description = organization.description || ''
  form.parentId = organization.parentId || undefined
  form.sortOrder = organization.sortOrder
  showCreateDialog.value = true
}

const deleteOrganization = async (organization: OrganizationUnit) => {
  try {
    const confirmed = await confirmDelete(organization.name)
    if (confirmed) {
      await organizationApi.delete(organization.id)
      ElMessage.success('删除成功')
      loadData()
    }
  } catch (error) {
    ErrorHandler.handle(error, 'deleteOrganization')
  }
}

const submitForm = async () => {
  if (!formRef.value) return
  
  try {
    await formRef.value.validate()
    
    await withLoading(async () => {
      if (editingOrganization.value) {
        const updateData: UpdateOrganizationUnitRequest = {
          id: editingOrganization.value.id,
          name: form.name,
          description: form.description,
          parentId: form.parentId,
          sortOrder: form.sortOrder
        }
        await organizationApi.update(updateData)
        ElMessage.success('更新成功')
      } else {
        const createData: CreateOrganizationUnitRequest = {
          name: form.name,
          description: form.description,
          parentId: form.parentId,
          sortOrder: form.sortOrder
        }
        await organizationApi.create(createData)
        ElMessage.success('创建成功')
      }
      
      showCreateDialog.value = false
      resetForm()
      loadData()
    })
  } catch (error) {
    ErrorHandler.handle(error, 'organizationForm')
  }
}

const resetForm = () => {
  editingOrganization.value = null
  form.name = ''
  form.description = ''
  form.parentId = undefined
  form.sortOrder = 1
  formRef.value?.resetFields()
}

const resetFormAndClose = () => {
  showCreateDialog.value = false
  resetForm()
}

const formatDate = (dateString: string) => DateFormatter.toDateTimeString(dateString)
</script>

<style scoped>



.main-content {
  max-width: 1400px;
  margin: -2rem auto 0;
  padding: 0 2rem 2rem;
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: 2rem;
  position: relative;
  z-index: 1;
}

.tree-section,
.table-section {
  background: white;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  transition: all 0.3s ease;
}

.tree-section:hover,
.table-section:hover {
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.12);
  transform: translateY(-2px);
}

.section-header {
  padding: 1.5rem 2rem;
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  border-bottom: 1px solid #e2e8f0;
}

.section-header h2 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.section-icon {
  color: #6366f1;
  font-size: 1.25rem;
}

.org-tree-wrapper,
.table-wrapper {
  padding: 1.5rem;
}



.organization-tree {
  border: none;
  background: transparent;
}

.organization-tree :deep(.el-tree-node) {
  margin-bottom: 0.5rem;
}

.organization-tree :deep(.el-tree-node__content) {
  padding: 1rem;
  border-radius: 8px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  transition: all 0.2s ease;
}

.organization-tree :deep(.el-tree-node__content:hover) {
  background: #e0e7ff;
  border-color: #c7d2fe;
  transform: translateX(4px);
}

.tree-node {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.node-content {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
}

.node-icon {
  color: #6366f1;
  font-size: 1rem;
}

.node-text {
  font-weight: 500;
  color: #374151;
}

.node-actions {
  display: flex;
  gap: 0.5rem;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.tree-node:hover .node-actions {
  opacity: 1;
}

.action-btn {
  width: 28px;
  height: 28px;
  transition: all 0.2s ease;
}

.action-btn:hover {
  transform: scale(1.1);
}

.organization-table {
  border-radius: 8px;
  overflow: hidden;
}

.organization-table :deep(.el-table__header) {
  border-radius: 8px 8px 0 0;
}

.organization-table :deep(.el-table__row) {
  transition: all 0.2s ease;
}

.organization-table :deep(.el-table__row:hover) {
  background-color: #f0f9ff !important;
}

.name-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.name-icon {
  color: #6366f1;
  font-size: 1rem;
}

.description-text {
  color: #6b7280;
  font-style: italic;
}

.time-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.25rem;
  font-size: 0.875rem;
}

.time-icon {
  color: #9ca3af;
  font-size: 0.875rem;
}

.status-tag {
  font-weight: 500;
}

.table-actions {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
}

.table-action-btn {
  width: 32px;
  height: 32px;
  transition: all 0.2s ease;
}

.table-action-btn:hover {
  transform: scale(1.1);
}

.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  color: #9ca3af;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: #d1d5db;
}

.organization-dialog :deep(.el-dialog__header) {
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
  color: white;
  padding: 1.5rem 2rem;
  margin: 0;
}

.organization-dialog :deep(.el-dialog__title) {
  color: white;
  font-weight: 600;
  font-size: 1.25rem;
}

.organization-dialog :deep(.el-dialog__headerbtn .el-dialog__close) {
  color: white;
  font-size: 1.25rem;
}

.organization-dialog :deep(.el-dialog__body) {
  padding: 2rem;
}

.organization-form :deep(.el-form-item__label) {
  font-weight: 600;
  color: #374151;
}

.organization-form :deep(.el-input__wrapper) {
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: all 0.2s ease;
}

.organization-form :deep(.el-input__wrapper:hover) {
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
}

.organization-form :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
}

.organization-form :deep(.el-textarea__inner) {
  border-radius: 8px;
  border: 1px solid #d1d5db;
  transition: all 0.2s ease;
}

.organization-form :deep(.el-textarea__inner:hover) {
  border-color: #9ca3af;
}

.organization-form :deep(.el-textarea__inner:focus) {
  border-color: #6366f1;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  /* padding: 1.5rem 2rem; */
  /* background: #f8fafc; */
  /* margin: 0 -2rem -2rem; */
}

.submit-btn {
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
  border: none;
  font-weight: 600;
  padding: 0.75rem 1.5rem;
  transition: all 0.2s ease;
}

.submit-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.4);
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .main-content {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
}

@media (max-width: 768px) {
  .page-header {
    padding: 1.5rem 1rem 2rem;
  }
  
  .header-content {
    flex-direction: column;
    gap: 1.5rem;
    align-items: stretch;
  }
  
  .title-section h1 {
    font-size: 2rem;
  }
  
  .main-content {
    margin-top: -1rem;
    padding: 0 1rem 1rem;
  }
  
  .org-tree-wrapper,
  .table-wrapper {
    padding: 1rem;
  }
  
  .organization-dialog {
    width: 90% !important;
  }
}
</style> 