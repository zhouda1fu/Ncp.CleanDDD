import { ElMessage } from 'element-plus'
import type { ApiError } from '@/types'

/**
 * 错误处理工具类
 */
export class ErrorHandler {
  /**
   * 处理API错误
   * @param error 错误对象
   * @param defaultMessage 默认错误消息
   */
  static handleApiError(error: any, defaultMessage = '操作失败'): void {
    console.error('API Error:', error)

    let message = defaultMessage

    if (error?.response?.data?.message) {
      message = error.response.data.message
    } else if (error?.message) {
      message = error.message
    } else if (typeof error === 'string') {
      message = error
    }

    ElMessage.error(message)
  }

  /**
   * 处理网络错误
   * @param error 错误对象
   */
  static handleNetworkError(error: any): void {
    console.error('Network Error:', error)

    if (!navigator.onLine) {
      ElMessage.error('网络连接已断开，请检查网络设置')
      return
    }

    if (error.code === 'ECONNABORTED') {
      ElMessage.error('请求超时，请稍后重试')
      return
    }

    ElMessage.error('网络错误，请检查网络连接')
  }

  /**
   * 处理权限错误
   * @param error 错误对象
   */
  static handlePermissionError(error: any): void {
    console.error('Permission Error:', error)
    ElMessage.error('权限不足，无法执行此操作')
  }

  /**
   * 处理验证错误
   * @param error 错误对象
   */
  static handleValidationError(error: any): void {
    console.error('Validation Error:', error)

    if (error?.response?.data?.errorData && Array.isArray(error.response.data.errorData)) {
      const errors = error.response.data.errorData
      if (errors.length > 0) {
        ElMessage.error(errors[0])
        return
      }
    }

    ElMessage.error('数据验证失败，请检查输入内容')
  }

  /**
   * 创建API错误对象
   * @param message 错误消息
   * @param code 错误代码
   * @param details 错误详情
   * @returns API错误对象
   */
  static createApiError(message: string, code = 0, details?: any): ApiError {
    return {
      message,
      code,
      details
    }
  }

  /**
   * 是否为取消操作
   * @param error 错误对象
   * @returns 是否为取消操作
   */
  static isCancelError(error: any): boolean {
    return error === 'cancel' || error?.message === 'cancel'
  }

  /**
   * 统一错误处理
   * @param error 错误对象
   * @param context 错误上下文
   */
  static handle(error: any, context = ''): void {
    // 如果是取消操作，不显示错误消息
    if (this.isCancelError(error)) {
      return
    }

    console.error(`Error in ${context}:`, error)

    // 根据错误类型选择处理方式
    if (error?.response) {
      const { status } = error.response

      switch (status) {
        case 400:
          this.handleValidationError(error)
          break
        case 401:
          // 401错误已在axios拦截器中处理
          break
        case 403:
          this.handlePermissionError(error)
          break
        case 404:
          ElMessage.error('请求的资源不存在')
          break
        case 500:
          ElMessage.error('服务器内部错误，请联系管理员')
          break
        default:
          this.handleApiError(error)
      }
    } else if (error?.code) {
      this.handleNetworkError(error)
    } else {
      this.handleApiError(error)
    }
  }
}

/**
 * 错误边界组件的错误处理
 * @param error 错误对象
 * @param info 错误信息
 */
export function handleComponentError(error: Error, info: any): void {
  console.error('Component Error:', error, info)
  ElMessage.error('组件渲染出错，请刷新页面重试')
}

/**
 * 全局错误处理
 * @param error 错误对象
 */
export function handleGlobalError(error: any): void {
  console.error('Global Error:', error)
  ElMessage.error('系统出现未知错误，请刷新页面重试')
}

/**
 * Promise 错误处理
 * @param error 错误对象
 */
export function handlePromiseRejection(error: any): void {
  console.error('Unhandled Promise Rejection:', error)
  ElMessage.error('操作失败，请重试')
}
