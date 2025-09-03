<template>
  <div class="register-container">
    <div class="register-box">
      <div class="register-header">
        <h2>CleanDDD 管理系统</h2>
        <p>创建新账户</p>
      </div>
      
      <el-form
        ref="registerFormRef"
        :model="registerForm"
        :rules="registerRules"
        class="register-form"
        @submit.prevent="handleRegister"
      >
        <el-form-item prop="name">
          <el-input
            v-model="registerForm.name"
            placeholder="请输入用户名"
            size="large"
            prefix-icon="User"
          />
        </el-form-item>
        
        <el-form-item prop="email">
          <el-input
            v-model="registerForm.email"
            placeholder="请输入邮箱"
            size="large"
            prefix-icon="Message"
          />
        </el-form-item>
        
        <el-form-item prop="password">
          <el-input
            v-model="registerForm.password"
            type="password"
            placeholder="请输入密码"
            size="large"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>
        
        <el-form-item prop="confirmPassword">
          <el-input
            v-model="registerForm.confirmPassword"
            type="password"
            placeholder="请确认密码"
            size="large"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>
        
        <el-form-item prop="realName">
          <el-input
            v-model="registerForm.realName"
            placeholder="请输入真实姓名"
            size="large"
            prefix-icon="UserFilled"
          />
        </el-form-item>
        
        <el-form-item prop="phone">
          <el-input
            v-model="registerForm.phone"
            placeholder="请输入手机号"
            size="large"
            prefix-icon="Phone"
          />
        </el-form-item>
        
        <el-form-item prop="roleIds">
          <el-select
            v-model="registerForm.roleIds"
            placeholder="请选择角色"
            size="large"
            multiple
            style="width: 100%"
          >
            <el-option
              v-for="role in roles"
              :key="role.roleId"
              :label="role.name"
              :value="role.roleId"
            />
          </el-select>
        </el-form-item>
        
        <el-form-item>
          <el-button
            type="primary"
            size="large"
            :loading="loading"
            class="register-button"
            @click="handleRegister"
          >
            注册
          </el-button>
        </el-form-item>
        
        <div class="register-footer">
          <el-link type="primary" @click="$router.push('/login')">
            已有账户？立即登录
          </el-link>
        </div>
      </el-form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { register, type RegisterRequest } from '@/api/user'
import { getAllRoles, type RoleInfo } from '@/api/role'

const router = useRouter()

const registerFormRef = ref<FormInstance>()
const loading = ref(false)
const roles = ref<RoleInfo[]>([])

const registerForm = reactive({
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
  realName: '',
  phone: '',
  roleIds: [] as string[]
})

const validateConfirmPassword = (_: any, value: string, callback: any) => {
  if (value === '') {
    callback(new Error('请再次输入密码'))
  } else if (value !== registerForm.password) {
    callback(new Error('两次输入密码不一致'))
  } else {
    callback()
  }
}

const registerRules: FormRules = {
  name: [
    { required: true, message: '请输入用户名', trigger: 'onBlur' },
    { min: 3, max: 20, message: '用户名长度在 3 到 20 个字符', trigger: 'onBlur' }
  ],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'onBlur' },
    { type: 'email', message: '请输入正确的邮箱格式', trigger: 'onBlur' }
  ],
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
  phone: [
    { required: true, message: '请输入手机号', trigger: 'onBlur' },
    { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号格式', trigger: 'onBlur' }
  ],
  roleIds: [
    { required: true, message: '请选择角色', trigger: 'onChange' }
  ]
}

const loadRoles = async () => {
  try {
    const response = await getAllRoles({
      pageIndex: 1,
      pageSize: 100,
      countTotal: true
    })
    roles.value = response.data.items
  } catch (error) {
    ElMessage.error('加载角色列表失败')
  }
}

const handleRegister = async () => {
  if (!registerFormRef.value) return
  
  try {
    await registerFormRef.value.validate()
    loading.value = true
    
    const registerData: RegisterRequest = {
      name: registerForm.name,
      email: registerForm.email,
      password: registerForm.password,
      phone: registerForm.phone,
      realName: registerForm.realName,
      status: 1,
      roleIds: registerForm.roleIds,
      organizationUnitId: 0,
      organizationUnitName: '',
      gender: '',
      age: 0,
      birthDate: '',
      userId: '',
      confirmPassword: registerForm.confirmPassword
    }
    
    await register(registerData)
    ElMessage.success('注册成功，请登录')
    router.push('/login')
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '注册失败')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadRoles()
})
</script>

<style scoped>
.register-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.register-box {
  width: 450px;
  padding: 40px;
  background: white;
  border-radius: 10px;
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.1);
}

.register-header {
  text-align: center;
  margin-bottom: 30px;
}

.register-header h2 {
  color: #333;
  margin-bottom: 10px;
  font-size: 24px;
}

.register-header p {
  color: #666;
  font-size: 14px;
}

.register-form {
  margin-bottom: 20px;
}

.register-button {
  width: 100%;
  height: 45px;
  font-size: 16px;
}

.register-footer {
  text-align: center;
  margin-top: 20px;
}
</style> 