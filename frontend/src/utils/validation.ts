import type { FormValidationRule } from '@/types'
import { validationConfig } from '@/config'

/**
 * 通用验证规则工厂
 */
export class ValidationRules {
  // 必填验证
  static required(message: string): FormValidationRule {
    return {
      required: true,
      message,
      trigger: 'blur'
    }
  }

  // 长度验证
  static length(min: number, max: number, message: string): FormValidationRule {
    return {
      min,
      max,
      message,
      trigger: 'blur'
    }
  }

  // 模式验证
  static pattern(pattern: RegExp, message: string): FormValidationRule {
    return {
      pattern,
      message,
      trigger: 'blur'
    }
  }

  // 自定义验证
  static custom(validator: (rule: any, value: any, callback: any) => void): FormValidationRule {
    return {
      validator,
      trigger: 'blur'
    }
  }

  // 用户名验证
  static username(): FormValidationRule[] {
    return [
      this.required('请输入用户名'),
      this.length(
        validationConfig.username.minLength,
        validationConfig.username.maxLength,
        `用户名长度应为 ${validationConfig.username.minLength}-${validationConfig.username.maxLength} 个字符`
      ),
      this.pattern(validationConfig.username.pattern, '用户名只能包含字母、数字和下划线')
    ]
  }

  // 密码验证
  static password(): FormValidationRule[] {
    return [
      this.required('请输入密码'),
      this.length(
        validationConfig.password.minLength,
        validationConfig.password.maxLength,
        `密码长度应为 ${validationConfig.password.minLength}-${validationConfig.password.maxLength} 个字符`
      )
    ]
  }

  // 确认密码验证
  static confirmPassword(passwordRef: string): FormValidationRule[] {
    return [
      this.required('请再次输入密码'),
      this.custom((_rule, value, callback) => {
        if (value === '') {
          callback(new Error('请再次输入密码'))
        } else if (value !== passwordRef) {
          callback(new Error('两次输入密码不一致'))
        } else {
          callback()
        }
      })
    ]
  }

  // 邮箱验证
  static email(): FormValidationRule[] {
    return [
      this.required('请输入邮箱'),
      this.pattern(validationConfig.email.pattern, '请输入有效的邮箱地址')
    ]
  }

  // 手机号验证
  static phone(): FormValidationRule[] {
    return [
      this.required('请输入手机号'),
      this.pattern(validationConfig.phone.pattern, '请输入有效的手机号')
    ]
  }

  // 真实姓名验证
  static realName(): FormValidationRule[] {
    return [
      this.required('请输入真实姓名'),
      this.length(2, 20, '真实姓名长度应为 2-20 个字符')
    ]
  }

  // 性别验证
  static gender(): FormValidationRule[] {
    return [
      this.required('请选择性别')
    ]
  }

  // 出生日期验证
  static birthDate(): FormValidationRule[] {
    return [
      this.required('请选择出生日期')
    ]
  }

  // 组织架构验证
  static organizationUnit(): FormValidationRule[] {
    return [
      this.required('请选择组织架构')
    ]
  }
}

/**
 * 验证工具函数
 */
export const validationUtils = {
  // 验证邮箱格式
  isValidEmail: (email: string): boolean => {
    return validationConfig.email.pattern.test(email)
  },

  // 验证手机号格式
  isValidPhone: (phone: string): boolean => {
    return validationConfig.phone.pattern.test(phone)
  },

  // 验证用户名格式
  isValidUsername: (username: string): boolean => {
    return validationConfig.username.pattern.test(username) &&
           username.length >= validationConfig.username.minLength &&
           username.length <= validationConfig.username.maxLength
  },

  // 验证密码强度
  isValidPassword: (password: string): boolean => {
    return password.length >= validationConfig.password.minLength &&
           password.length <= validationConfig.password.maxLength
  }
}
