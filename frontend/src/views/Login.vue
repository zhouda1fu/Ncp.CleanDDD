<template>
  <div class="login-container">
    <div class="login-background">
      <div class="background-shapes">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
        <div class="shape shape-4"></div>
      </div>
    </div>
    
    <div class="login-content">
      <div class="logo-container">
        <div class="logo-wrapper">
          <img src="@/assets/images/logo.png" alt="Logo" class="logo-image" />
        </div>
      </div>
      <div class="login-card hover-card">
        <div class="card-header">
          <h2 class="login-title">欢迎登录</h2>
          <p class="login-subtitle">请输入您的账号信息</p>
        </div>
        <el-form
          ref="loginFormRef"
          :model="loginForm"
          :rules="loginRules"
          class="login-form"
          @submit.prevent="handleLogin"
        >
          <el-form-item prop="username" class="form-item">
            <el-input
              v-model="loginForm.username"
              placeholder="请输入用户名"
              size="large"
              class="login-input"
            >
              <template #prefix>
                <el-icon size="18" color="#94a3b8"><User /></el-icon>
              </template>
            </el-input>
          </el-form-item>
          
          <el-form-item prop="password" class="form-item">
            <el-input
              v-model="loginForm.password"
              type="password"
              placeholder="请输入密码"
              size="large"
              class="login-input"
              show-password
              @keyup.enter="handleLogin"
            >
              <template #prefix>
                <el-icon size="18" color="#94a3b8"><Lock /></el-icon>
              </template>
            </el-input>
          </el-form-item>
          
          <!-- <div class="form-options">
            <el-checkbox v-model="rememberMe" class="remember-checkbox">
              记住我
            </el-checkbox>
            <el-link type="primary" class="forgot-link">忘记密码？</el-link>
          </div> -->
          
          <el-form-item class="form-item">
            <el-button
              type="primary"
              size="large"
              :loading="loading"
              class="login-button hover-button"
              @click="handleLogin"
            >
              <el-icon v-if="!loading"><Right /></el-icon>
              <span>{{ loading ? '登录中...' : '登录' }}</span>
            </el-button>
          </el-form-item>
        </el-form>
        
        <div class="login-footer">
          <!-- <div class="divider">
            <span class="divider-text">或者</span>
          </div>
          
          <div class="social-login">
            <el-button class="social-btn" size="large">
              <el-icon size="18"><ChatDotRound /></el-icon>
              微信登录
            </el-button>
            <el-button class="social-btn" size="large">
              <el-icon size="18"><Message /></el-icon>
              邮箱登录
            </el-button>
          </div> -->
          
          <!-- <div class="register-link">
            <span class="register-text">还没有账户？</span>
            <el-link type="primary" class="register-btn" @click="$router.push('/register')">
              立即注册
            </el-link>
          </div> -->
        </div>
      </div>
      
      <!-- <div class="login-info">
        <div class="info-card">
          <div class="info-icon">
            <el-icon size="24" color="#667eea"><Shield /></el-icon>
          </div>
          <h3 class="info-title">安全可靠</h3>
          <p class="info-desc">采用最新的安全技术，保护您的数据安全</p>
        </div>
        
        <div class="info-card">
          <div class="info-icon">
            <el-icon size="24" color="#667eea"><DataAnalysis /></el-icon>
          </div>
          <h3 class="info-title">智能分析</h3>
          <p class="info-desc">强大的数据分析功能，助您做出明智决策</p>
        </div>
        
        <div class="info-card">
          <div class="info-icon">
            <el-icon size="24" color="#667eea"><Connection /></el-icon>
          </div>
          <h3 class="info-title">高效协作</h3>
          <p class="info-desc">支持多用户协作，提升团队工作效率</p>
        </div>
      </div> -->
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const loginFormRef = ref<FormInstance>()
const loading = ref(false)
// const rememberMe = ref(false)

const loginForm = reactive({
  username: '',
  password: ''
})

const loginRules: FormRules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'onBlur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'onBlur' },
    { min: 6, message: '密码长度不能少于6位', trigger: 'onBlur' }
  ]
}

const handleLogin = async () => {
  if (!loginFormRef.value) return
  
  try {
    await loginFormRef.value.validate()
    loading.value = true
    
    await authStore.login({ username: loginForm.username, password: loginForm.password })
    ElMessage.success('登录成功，欢迎回来！')
    router.push('/')
  } catch (error: any) {
    ElMessage.error(error.response?.data?.message || '登录失败，请检查用户名和密码')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  position: relative;
  overflow: hidden;
}

.login-background {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1;
}

.background-shapes {
  position: relative;
  width: 100%;
  height: 100%;
}

.shape {
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  animation: float 6s ease-in-out infinite;
}

.shape-1 {
  width: 80px;
  height: 80px;
  top: 20%;
  left: 10%;
  animation-delay: 0s;
}

.shape-2 {
  width: 120px;
  height: 120px;
  top: 60%;
  right: 10%;
  animation-delay: 2s;
}

.shape-3 {
  width: 60px;
  height: 60px;
  bottom: 20%;
  left: 20%;
  animation-delay: 4s;
}

.shape-4 {
  width: 100px;
  height: 100px;
  top: 10%;
  right: 20%;
  animation-delay: 1s;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px) rotate(0deg);
  }
  50% {
    transform: translateY(-20px) rotate(180deg);
  }
}

.login-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 40px;
  z-index: 2;
  position: relative;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 40px;
}

.logo-container {
  display: flex;
  justify-content: center;
  margin-bottom: 20px;
}

.logo-wrapper {
  width: 100%;
  height: 80px;
  /* background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); */
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  /* box-shadow: 0 12px 32px rgba(102, 126, 234, 0.3); */
  overflow: hidden;
  transition: all 0.3s ease;
}

/* .logo-wrapper:hover {
  transform: translateY(-4px);
  box-shadow: 0 16px 40px rgba(102, 126, 234, 0.4);
} */

.login-card {
  width: 520px;
  padding: 48px;
  background: white;
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
}



.logo-image {
  /* width: 200px; */
  height: 100%;
  object-fit: contain;
  border-radius: 8px;
}

.card-header {
  padding: 40px 40px 40px 40px;
  text-align: center;
}

.login-title {
  font-size: 28px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 8px 0;
  line-height: 1.2;
}

.login-subtitle {
  font-size: 16px;
  color: #64748b;
  margin: 0;
  font-weight: 400;
}

.login-form {
  margin-bottom: 32px;
}

.form-item {
  margin-bottom: 24px;
}

.login-input {
  height: 48px;
  border-radius: 12px;
  border: 2px solid #e2e8f0;
  transition: all 0.3s ease;
}

.login-input:hover {
  border-color: #cbd5e1;
}

.login-input:focus-within {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.remember-checkbox {
  color: #64748b;
  font-size: 14px;
}

.forgot-link {
  font-size: 14px;
  color: #667eea;
  text-decoration: none;
}

.forgot-link:hover {
  color: #4f46e5;
}

.login-button {
  width: 100%;
  height: 48px;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.3s ease;
}

.login-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 32px rgba(102, 126, 234, 0.4);
}

.login-footer {
  text-align: center;
}

.divider {
  position: relative;
  margin: 24px 0;
  text-align: center;
}

.divider::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 1px;
  background: #e2e8f0;
}

.divider-text {
  background: white;
  padding: 0 16px;
  color: #94a3b8;
  font-size: 14px;
  position: relative;
  z-index: 1;
}

.social-login {
  display: flex;
  gap: 12px;
  margin-bottom: 24px;
}

.social-btn {
  flex: 1;
  height: 44px;
  border-radius: 12px;
  border: 2px solid #e2e8f0;
  background: white;
  color: #64748b;
  font-weight: 500;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.social-btn:hover {
  border-color: #667eea;
  color: #667eea;
  background: #f8fafc;
}

.register-link {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.register-text {
  color: #64748b;
  font-size: 14px;
}

.register-btn {
  font-size: 14px;
  font-weight: 500;
}

.login-info {
  display: flex;
  flex-direction: column;
  gap: 24px;
  max-width: 300px;
}

.info-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 16px;
  padding: 24px;
  text-align: center;
  color: white;
  transition: all 0.3s ease;
}

.info-card:hover {
  transform: translateY(-4px);
  background: rgba(255, 255, 255, 0.15);
}

.info-icon {
  width: 48px;
  height: 48px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
}

.info-title {
  font-size: 18px;
  font-weight: 600;
  margin: 0 0 8px 0;
  color: white;
}

.info-desc {
  font-size: 14px;
  line-height: 1.5;
  margin: 0;
  opacity: 0.9;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .login-info {
    display: none;
  }
  
  .login-content {
    gap: 30px;
  }
}

@media (max-width: 768px) {
  .login-content {
    padding: 0 20px;
  }
  
  .login-card {
    width: 100%;
    max-width: 400px;
    padding: 32px 24px;
  }
  
  .login-title {
    font-size: 28px;
  }
  
  .social-login {
    flex-direction: column;
  }
  
  .form-options {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
}

@media (max-width: 480px) {
  .login-card {
    padding: 24px 20px;
  }
  
  .login-title {
    font-size: 24px;
  }
  
  .logo-wrapper {
    width: 80px;
    height: 80px;
  }
}
</style> 