<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="emit('update:visible', $event)"
    :title="dialogTitle"
    width="600px"
    class="management-dialog"
    :close-on-click-modal="false"
    @close="handleClose"
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
              <el-option :label="GENDER.MALE" :value="GENDER.MALE" />
              <el-option :label="GENDER.FEMALE" :value="GENDER.FEMALE" />
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
          <el-radio :label="USER_STATUS.ENABLED" class="status-radio">
            <div class="radio-content">
              <el-icon size="16" color="#10b981"><CircleCheck /></el-icon>
              <span>启用</span>
            </div>
          </el-radio>
          <el-radio :label="USER_STATUS.DISABLED" class="status-radio">
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
        <el-button @click="handleCancel" size="large" icon="Close">取消</el-button>
        <el-button 
          type="primary" 
          :loading="submitLoading" 
          @click="handleSubmit" 
          size="large" 
          class="submit-btn" 
          icon="Check"
        >
          {{ isEdit ? '更新' : '创建' }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import { ElMessage, type FormInstance } from 'element-plus'
import {
  User, 
  UserFilled, 
  Message, 
  Phone, 
  Lock, 
  CircleCheck,
  CircleClose
} from '@element-plus/icons-vue'
import { register, updateUser, type RegisterRequest } from '@/api/user'
import type { User as UserType, OrganizationUnitTree } from '@/types'
import { ValidationRules } from '@/utils/validation'
import { DateFormatter } from '@/utils/format'
import { ErrorHandler } from '@/utils/error'
import { useLoading } from '@/composables'
import { GENDER, USER_STATUS } from '@/constants'

interface Props {
  visible: boolean
  isEdit: boolean
  userData?: UserType | null
  organizationTreeOptions: OrganizationUnitTree[]
}

interface Emits {
  (e: 'update:visible', value: boolean): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { loading: submitLoading, withLoading } = useLoading()
const userFormRef = ref<FormInstance>()

const userForm = reactive<RegisterRequest>({
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
  phone: '',
  realName: '',
  status: USER_STATUS.ENABLED,
  roleIds: [],
  gender: '',
  age: 0,
  organizationUnitId: 0,
  organizationUnitName: '',
  birthDate: '',
  userId: ''
})

// 验证规则
const userRules = computed(() => ({
  name: ValidationRules.username(),
  email: ValidationRules.email(),
  password: props.isEdit ? [] : ValidationRules.password(),
  confirmPassword: props.isEdit ? [] : ValidationRules.confirmPassword(userForm.password),
  realName: ValidationRules.realName(),
  phone: ValidationRules.phone(),
  gender: ValidationRules.gender(),
  organizationUnitId: ValidationRules.organizationUnit(),
  birthDate: ValidationRules.birthDate()
}))

const dialogTitle = computed(() => props.isEdit ? '编辑用户' : '新建用户')

// 使用工具函数计算年龄
const calculateAge = DateFormatter.calculateAge

// 监听出生日期变化，自动计算年龄
watch(() => userForm.birthDate, (newBirthDate) => {
  if (newBirthDate) {
    userForm.age = calculateAge(newBirthDate)
  } else {
    userForm.age = 0
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

// 监听对话框显示状态，初始化表单数据
watch(() => props.visible, (newVisible) => {
  if (newVisible) {
    if (props.isEdit && props.userData) {
      // 编辑模式，填充用户数据
      Object.assign(userForm, {
        name: props.userData.name,
        email: props.userData.email,
        password: '',
        confirmPassword: '',
        phone: props.userData.phone,
        realName: props.userData.realName,
        status: props.userData.status,
        roleIds: [],
        gender: props.userData.gender,
        age: props.userData.age,
        organizationUnitId: props.userData.organizationUnitId,
        organizationUnitName: props.userData.organizationUnitName,
        birthDate: props.userData.birthDate,
        userId: props.userData.userId
      })
      
      // 如果有出生日期，重新计算年龄
      if (props.userData.birthDate) {
        userForm.age = calculateAge(props.userData.birthDate)
      }
      
      // 如果用户有组织架构ID但没有名称，自动设置名称
      if (props.userData.organizationUnitId && !props.userData.organizationUnitName) {
        const selectedOrg = findOrganizationById(props.organizationTreeOptions, props.userData.organizationUnitId);
        if (selectedOrg) {
          userForm.organizationUnitName = selectedOrg.name;
        }
      }
    } else {
      // 创建模式，重置表单
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
        userId: ''
      })
    }
  }
})

const handleOrganizationUnitChange = (value: number | undefined) => {
  if (value) {
    const selectedOrg = findOrganizationById(props.organizationTreeOptions, value);
    if (selectedOrg) {
      userForm.organizationUnitName = selectedOrg.name;
    } else {
      userForm.organizationUnitName = '';
    }
  } else {
    userForm.organizationUnitName = '';
  }
}

const handleSubmit = async () => {
  if (!userFormRef.value) return
  
  try {
    await userFormRef.value.validate()
    
    await withLoading(async () => {
      if (props.isEdit) {
        await updateUser({
          userId: userForm.userId,
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
          password: ''
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
      
      emit('success')
      emit('update:visible', false)
    })
  } catch (error) {
    ErrorHandler.handle(error, 'userForm')
  }
}

const handleCancel = () => {
  emit('update:visible', false)
}

const handleClose = () => {
  userFormRef.value?.resetFields()
}
</script>

<style scoped>
.management-dialog {
  border-radius: 12px;
}

.management-form {
  padding: 1rem 0;
}

.status-radio-group {
  display: flex;
  gap: 2rem;
}

.status-radio {
  margin: 0;
}

.radio-content {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  border-radius: 6px;
  transition: all 0.2s ease;
}

.status-radio:hover .radio-content {
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
  .status-radio-group {
    flex-direction: column;
    gap: 1rem;
  }
  
  .dialog-footer {
    flex-direction: column;
  }
  
  .dialog-footer .el-button {
    width: 100%;
  }
}
</style>
