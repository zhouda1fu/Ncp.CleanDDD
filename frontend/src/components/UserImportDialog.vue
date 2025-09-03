<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="emit('update:visible', $event)"
    title="批量导入用户"
    width="600px"
    class="management-dialog"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <div class="import-dialog-content"> 
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
                  @change="handleOrganizationUnitChange"
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
        <el-button @click="handleCancel" size="large" icon="Close">取消</el-button>
        <el-button 
          type="primary" 
          :loading="importLoading" 
          :disabled="!selectedFile"
          @click="handleSubmit" 
          size="large"
          class="submit-btn"
          icon="Upload"
        >
          开始导入
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, watch, defineProps, defineEmits } from 'vue'
import { ElMessage, type FormInstance, type FormRules, type UploadFile, type UploadFiles } from 'element-plus'
import { UploadFilled, Setting, Warning, CircleCheck } from '@element-plus/icons-vue'
import { batchImportUsers, type BatchImportUsersResponse } from '@/api/user'
import { type RoleInfo } from '@/api/role'
import { type OrganizationUnitTree } from '@/api/organization'

interface Props {
  visible: boolean
  allRoles: RoleInfo[]
  organizationTreeOptions: OrganizationUnitTree[]
}

interface Emits {
  (e: 'update:visible', value: boolean): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const importLoading = ref(false)
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

// 监听对话框显示状态，重置数据
watch(() => props.visible, (newVisible) => {
  if (newVisible) {
    importResult.value = null
    selectedFile.value = null
    fileList.value = []
    importFormRef.value?.resetFields()
    importForm.organizationUnitId = null
    importForm.organizationUnitName = ''
    importForm.roleIds = []
  }
})

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

const handleOrganizationUnitChange = (value: number | undefined) => {
  if (value) {
    const selectedOrg = findOrganizationById(props.organizationTreeOptions, value);
    if (selectedOrg) {
      importForm.organizationUnitName = selectedOrg.name;
    } else {
      importForm.organizationUnitName = '';
    }
  } else {
    importForm.organizationUnitName = '';
  }
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
const handleSubmit = async () => {
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
      emit('success')
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

const handleCancel = () => {
  emit('update:visible', false)
}

const handleClose = () => {
  importResult.value = null
  selectedFile.value = null
  fileList.value = []
  uploadRef.value?.clearFiles()
  importFormRef.value?.resetFields()
  importForm.organizationUnitId = null
  importForm.organizationUnitName = ''
  importForm.roleIds = []
}
</script>

<style scoped>
.management-dialog {
  border-radius: 12px;
}

/* 对话框内容容器 */
.import-dialog-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  max-height: 70vh;
  overflow-y: auto;
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

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

.submit-btn {
  min-width: 100px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .import-dialog-content {
    gap: 1rem;
    padding: 1rem;
  }

  .batch-settings {
    padding: 1rem;
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

  .dialog-footer {
    flex-direction: column;
  }
  
  .dialog-footer .el-button {
    width: 100%;
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

/* 动画效果 */
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

/* 应用动画类 */
.batch-settings,
.import-result {
  animation: fadeInUp 0.4s ease-out;
}
</style>
