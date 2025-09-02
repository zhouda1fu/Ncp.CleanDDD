<template>
  <div class="profile-page">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>个人资料</span>
        </div>
      </template>
      
      <el-row :gutter="40">
        <el-col :span="8">
          <div class="profile-avatar">
            <el-avatar :size="120" icon="UserFilled" />
            <h3>{{ userProfile?.name }}</h3>
            <p>{{ userProfile?.email }}</p>
          </div>
        </el-col>
        
        <el-col :span="16">
          <el-form
            ref="profileFormRef"
            :model="profileForm"
            :rules="profileRules"
            label-width="100px"
          >
            <el-form-item label="用户名" prop="name">
              <el-input v-model="profileForm.name" />
            </el-form-item>
            
            <el-form-item label="邮箱" prop="email">
              <el-input v-model="profileForm.email" />
            </el-form-item>
            
            <el-form-item label="真实姓名" prop="realName">
              <el-input v-model="profileForm.realName" />
            </el-form-item>
            
            <el-form-item label="手机号" prop="phone">
              <el-input v-model="profileForm.phone" />
            </el-form-item>

            <el-form-item label="性别" prop="gender">
              <el-select v-model="profileForm.gender" placeholder="请选择性别">
                <el-option label="男" value="男" />
                <el-option label="女" value="女" />
              </el-select>
            </el-form-item>
            
            <el-form-item label="出生日期" prop="birthDate">
              <el-date-picker
                v-model="profileForm.birthDate"
                type="date"
                placeholder="请选择出生日期"
              />
            </el-form-item>
            <el-form-item label="密码" prop="password">
              <el-input v-model="profileForm.password" />
            </el-form-item>

            <el-form-item label="状态">
              <el-tag :type="profileForm.status === 1 ? 'success' : 'danger'">
                {{ profileForm.status === 1 ? '启用' : '禁用' }}
              </el-tag>
            </el-form-item>
            
            <el-form-item label="角色">
              <el-tag
                v-for="role in userProfile?.roles"
                :key="role"
                type="info"
                style="margin-right: 5px"
              >
                {{ role }}
              </el-tag>
            </el-form-item>
            
            <el-form-item label="创建时间">
              <span>{{ formatDate(userProfile?.createdAt) }}</span>
            </el-form-item>
            <el-form-item label="组织架构" prop="organizationUnitId" >
          <el-select 
            v-model="profileForm.organizationUnitId" 
            placeholder="请选择组织架构" 
            clearable
            @change="handleOrganizationUnitChange"
            disabled
          >
            <el-option
              v-for="org in organizationOptions"
              :key="org.id"
              :label="org.name"
              :value="org.id"
            />
          </el-select>
        </el-form-item>
            
            <el-form-item>
              <el-button type="primary" :loading="loading" @click="handleUpdate">
                保存修改
              </el-button>
              <el-button @click="handleReset">重置</el-button>
            </el-form-item>
          </el-form>
        </el-col>
      </el-row>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { getUserProfile, updateUser, type UserProfileResponse } from '@/api/user'
import { useAuthStore } from '@/stores/auth'
import { organizationApi, type OrganizationUnit } from '@/api/organization'
const organizationOptions = ref<OrganizationUnit[]>([])

const authStore = useAuthStore()

const profileFormRef = ref<FormInstance>()
const loading = ref(false)
const userProfile = ref<UserProfileResponse | null>(null)


const profileForm = reactive({
  name: '',
  email: '',
  realName: '',
  phone: '',
  status: 1,
  gender: '',
  age: 0,
  birthDate: '',
  organizationUnitId: 0,
  organizationUnitName: '',
  password: ''
})

const profileRules: FormRules = {
  name: [
    { required: true, message: '请输入用户名', trigger: 'onBlur' }
  ],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'onBlur' },
    { type: 'email', message: '请输入正确的邮箱格式', trigger: 'onBlur' }
  ],
  realName: [
    { required: true, message: '请输入真实姓名', trigger: 'onBlur' }
  ],
  phone: [
    { required: true, message: '请输入手机号', trigger: 'onBlur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号格式', trigger: 'onBlur' }
  ]
}


const handleOrganizationUnitChange = (value: number | undefined) => {
  const selectedOrg = organizationOptions.value.find(org => org.id === value);
  if (selectedOrg) {
    profileForm.organizationUnitName = selectedOrg.name;
  } else {
    profileForm.organizationUnitName = '';
  }
};


const loadOrganizationUnits = async () => {


  try {
    const response = await organizationApi.getAll(true)
    organizationOptions.value = response.data
  } catch (error) {
    // 错误已在全局拦截器中处理
  }
}

const loadUserProfile = async () => {
  if (!authStore.user?.userId) {
    console.log('无法获取用户信息')
    return
  }
  
  try {
    const response = await getUserProfile(authStore.user.userId)
    userProfile.value = response.data
    
    // 填充表单数据
    Object.assign(profileForm, {
      name: userProfile.value.name,
      email: userProfile.value.email,
      realName: userProfile.value.realName,
      phone: userProfile.value.phone,
      status: userProfile.value.status,
      gender: userProfile.value.gender || '',
      birthDate: userProfile.value.birthDate || '',
      organizationUnitId: userProfile.value.organizationUnitId || 0,
      organizationUnitName: userProfile.value.organizationUnitName || '',
      password:  ''
    })
  } catch (error) {
    ElMessage.error('加载用户资料失败')
  }
}

const handleUpdate = async () => {
  if (!profileFormRef.value || !authStore.user?.userId) return
  
  try {
    await profileFormRef.value.validate()
    loading.value = true
    
    await updateUser({
      userId: authStore.user.userId,
      name: profileForm.name,
      email: profileForm.email,
      realName: profileForm.realName,
      phone: profileForm.phone,
      status: profileForm.status,
      gender: profileForm.gender || '',
      age: profileForm.age || 0,
      birthDate: profileForm.birthDate || '',
      organizationUnitId: profileForm.organizationUnitId || 0,
      organizationUnitName: profileForm.organizationUnitName || '',
      password: profileForm.password || ''
    })
    ElMessage.success('更新成功')
    
    // 重新加载用户资料
    await loadUserProfile()
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '更新失败')
  } finally {
    loading.value = false
  }
}

const handleReset = () => {
  if (userProfile.value) {
    Object.assign(profileForm, {
      name: userProfile.value.name,
      email: userProfile.value.email,
      realName: userProfile.value.realName,
      phone: userProfile.value.phone,
      status: userProfile.value.status
    })
  }
}

const formatDate = (dateString?: string) => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleString('zh-CN')
}

onMounted(async () => {
  await loadUserProfile()
  await loadOrganizationUnits()
})
</script>

<style scoped>
.profile-page {
  padding: 20px;
}

.card-header {
  font-weight: bold;
  color: #333;
}

.profile-avatar {
  text-align: center;
  padding: 20px 0;
}

.profile-avatar h3 {
  margin: 15px 0 5px 0;
  color: #333;
}

.profile-avatar p {
  margin: 0;
  color: #666;
  font-size: 14px;
}
</style> 