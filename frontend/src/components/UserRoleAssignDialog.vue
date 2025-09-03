<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="emit('update:visible', $event)"
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
          <div class="user-name-large">{{ userData?.name }}</div>
          <div class="user-email-large">{{ userData?.email }}</div>
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
        <el-button @click="handleCancel" size="large">取消</el-button>
        <el-button 
          type="primary" 
          :loading="submitLoading" 
          @click="handleSubmit" 
          size="large" 
          class="submit-btn"
        >
          <el-icon v-if="!submitLoading"><Check /></el-icon>
          确定
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, watch, defineProps, defineEmits } from 'vue'
import { ElMessage } from 'element-plus'
import { UserFilled, Setting, Check } from '@element-plus/icons-vue'
import { updateUserRoles, type UserInfo } from '@/api/user'
import { type RoleInfo } from '@/api/role'

interface Props {
  visible: boolean
  userData: UserInfo | null
  allRoles: RoleInfo[]
}

interface Emits {
  (e: 'update:visible', value: boolean): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const submitLoading = ref(false)
const selectedRoleIds = ref<string[]>([])

// 监听对话框显示状态，初始化选中的角色
watch(() => props.visible, (newVisible) => {
  if (newVisible && props.userData) {
    // 根据用户当前角色设置选中的角色ID
    selectedRoleIds.value = props.userData.roles.map((role: string) => {
      const foundRole = props.allRoles.find(r => r.name === role)
      return foundRole?.roleId || ''
    }).filter(Boolean)
  }
})

const handleSubmit = async () => {
  if (!props.userData) return
  
  try {
    submitLoading.value = true
    await updateUserRoles({
      roleIds: selectedRoleIds.value,
      userId: props.userData.userId
    })
    ElMessage.success('角色分配成功')
    emit('success')
    emit('update:visible', false)
  } catch (error: any) {
    // 错误已在全局拦截器中处理
  } finally {
    submitLoading.value = false
  }
}

const handleCancel = () => {
  emit('update:visible', false)
}
</script>

<style scoped>
.management-dialog {
  border-radius: 12px;
}

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
  .role-checkbox-group {
    grid-template-columns: 1fr;
  }
  
  .user-info-card {
    flex-direction: column;
    text-align: center;
    gap: 0.75rem;
  }
  
  .dialog-footer {
    flex-direction: column;
  }
  
  .dialog-footer .el-button {
    width: 100%;
  }
}
</style>
