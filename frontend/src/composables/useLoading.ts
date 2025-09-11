import { ref } from 'vue'

/**
 * 加载状态管理 Composable
 * @param initialValue 初始值
 * @returns 加载状态和控制方法
 */
export function useLoading(initialValue = false) {
  const loading = ref(initialValue)

  const setLoading = (value: boolean) => {
    loading.value = value
  }

  const withLoading = async <T>(fn: () => Promise<T>): Promise<T> => {
    try {
      loading.value = true
      return await fn()
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    setLoading,
    withLoading
  }
}
