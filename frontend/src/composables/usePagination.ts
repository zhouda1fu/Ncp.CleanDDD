import { reactive, computed } from 'vue'
import type { PaginationRequest } from '@/types'

/**
 * 分页管理 Composable
 * @param initialPageSize 初始每页大小
 * @returns 分页状态和控制方法
 */
export function usePagination(initialPageSize = 10) {
  const pagination = reactive({
    pageIndex: 1,
    pageSize: initialPageSize,
    total: 0
  })

  const paginationRequest = computed((): PaginationRequest => ({
    pageIndex: pagination.pageIndex,
    pageSize: pagination.pageSize,
    countTotal: true
  }))

  const handleSizeChange = (size: number) => {
    pagination.pageSize = size
    pagination.pageIndex = 1
  }

  const handleCurrentChange = (page: number) => {
    pagination.pageIndex = page
  }

  const resetPagination = () => {
    pagination.pageIndex = 1
    pagination.total = 0
  }

  const updateTotal = (total: number) => {
    pagination.total = total
  }

  return {
    pagination,
    paginationRequest,
    handleSizeChange,
    handleCurrentChange,
    resetPagination,
    updateTotal
  }
}
