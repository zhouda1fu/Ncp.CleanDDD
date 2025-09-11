import { ElMessageBox } from 'element-plus'

interface ConfirmOptions {
  title?: string
  message: string
  type?: 'success' | 'info' | 'warning' | 'error'
  confirmButtonText?: string
  cancelButtonText?: string
}

/**
 * 确认对话框 Composable
 * @returns 确认对话框方法
 */
export function useConfirm() {
  const confirm = async (options: ConfirmOptions): Promise<boolean> => {
    try {
      await ElMessageBox.confirm(options.message, options.title || '提示', {
        confirmButtonText: options.confirmButtonText || '确定',
        cancelButtonText: options.cancelButtonText || '取消',
        type: options.type || 'warning'
      })
      return true
    } catch {
      return false
    }
  }

  const confirmDelete = async (itemName: string): Promise<boolean> => {
    return confirm({
      message: `确定要删除"${itemName}"吗？此操作不可撤销。`,
      type: 'warning',
      title: '删除确认'
    })
  }

  const confirmBatchDelete = async (count: number): Promise<boolean> => {
    return confirm({
      message: `确定要删除选中的 ${count} 项吗？此操作不可撤销。`,
      type: 'warning',
      title: '批量删除确认'
    })
  }

  return {
    confirm,
    confirmDelete,
    confirmBatchDelete
  }
}
