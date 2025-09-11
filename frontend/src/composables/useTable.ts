import { ref, reactive } from 'vue'
import type { PaginationResponse } from '@/types'
import { usePagination } from './usePagination'
import { useLoading } from './useLoading'

/**
 * 表格数据管理 Composable
 * @param fetchFn 数据获取函数
 * @param initialPageSize 初始每页大小
 * @returns 表格状态和控制方法
 */
export function useTable<T = any>(
  fetchFn: (params: any) => Promise<PaginationResponse<T>>,
  initialPageSize = 10
) {
  const { loading, withLoading } = useLoading()
  const { pagination, paginationRequest, handleSizeChange, handleCurrentChange, resetPagination, updateTotal } = usePagination(initialPageSize)

  const data = ref<T[]>([])
  const selectedRows = ref<T[]>([])
  const searchParams = reactive<Record<string, any>>({})

  const loadData = async (resetPage = false) => {
    if (resetPage) {
      resetPagination()
    }

    await withLoading(async () => {
      try {
        const params = {
          ...paginationRequest.value,
          ...searchParams
        }
        const response = await fetchFn(params)
        data.value = response.items
        updateTotal(response.total)
      } catch (error) {
        data.value = []
        updateTotal(0)
        throw error
      }
    })
  }

  const handleSearch = async (params: Record<string, any> = {}) => {
    Object.assign(searchParams, params)
    await loadData(true)
  }

  const handleReset = async () => {
    Object.keys(searchParams).forEach(key => {
      delete searchParams[key]
    })
    await loadData(true)
  }

  const handleSelectionChange = (selection: T[]) => {
    selectedRows.value = selection
  }

  const refresh = () => loadData()

  const handlePageSizeChange = async (size: number) => {
    handleSizeChange(size)
    await loadData()
  }

  const handlePageCurrentChange = async (page: number) => {
    handleCurrentChange(page)
    await loadData()
  }

  return {
    // 数据
    data,
    selectedRows,
    pagination,
    loading,
    searchParams,

    // 方法
    loadData,
    handleSearch,
    handleReset,
    handleSelectionChange,
    refresh,
    handlePageSizeChange,
    handlePageCurrentChange
  }
}
